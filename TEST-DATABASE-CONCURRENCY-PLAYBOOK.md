# Test Database Concurrency: Problem and Fix Playbook

A portable guide for fixing flaky, slow integration tests caused by per-test database provisioning under
parallelism. It is written so it can be handed to an agent working on a **different** solution that also uses
Testcontainers but is structured differently (different test framework, fixture model, ORM, or database engine).

Read sections 1 to 3 to understand the problem, section 4 for the concrete reference fix we applied (SQL Server
plus EF Core), section 5 to adapt it to another engine, and sections 6 to 7 for the portable lessons and a
checklist.

---

## TL;DR

- **Problem.** Each test creates its own database by running migrations. Under parallel execution, many
  concurrent `CREATE DATABASE` operations serialise inside the engine and intermittently fail, and even when they
  pass the suite is slow because the full schema is built once per test.
- **Fix.** Build the schema (migrations) **once** into a template database, capture it, then give each test a
  cheap **clone** of that template instead of re-running migrations. Cloning avoids the contention and is far
  faster.
- **Plus robustness.** Build the template exactly once across threads, evict a failed template build so one
  transient error does not poison the whole run, force exclusive access before destructive database operations,
  and drop only the per-test databases (never the template other tests still need).

---

## 1. Symptoms: how to recognise this problem

- Failures that appear **only under parallel execution** and disappear when tests run serially.
- Errors about database-creation contention:
  - SQL Server: `Could not obtain exclusive lock on database 'model'` (error number **1807**).
  - PostgreSQL: `source database "..." is being accessed by other users`, or races on `database "..." already exists`.
- Test setup time dominated by repeated schema migration.
- Timeouts or deadlocks during the equivalent of `EnsureCreated` / `Migrate` at test start.

If the flakiness is instead about tests seeing each other's **data** (not creation contention), that is a
different problem (isolation, not provisioning), and section 5's "data reset" option is more relevant than the
template approach.

---

## 2. Root cause

**General.** Creating a database is a comparatively heavy, serialised operation in most engines. "Run migrations
against a brand-new database per test" multiplies that cost by the number of tests and then runs them
concurrently against a single shared server, so they queue and contend.

**SQL Server specifics.** `CREATE DATABASE` copies the `model` system database as a template, taking a
server-wide lock on `model`. Concurrent creations queue on that lock and time out, surfacing as error 1807.
EF Core's `MigrateAsync()` issues a `CREATE DATABASE` when the target does not exist, so "migrate per test"
equals "`CREATE DATABASE` per test" equals contention.

**PostgreSQL specifics.** `CREATE DATABASE ... TEMPLATE t` requires that **no other session is connected to the
template** `t`; concurrent creations from the same template serialise and can error while the template is in use.

---

## 3. The fix strategy: build once, clone per test

Three phases:

1. **Build the template once.** Create a single template database and run all migrations against it.
2. **Capture it.** SQL Server: `BACKUP` it to a `.bak` file that lives inside the container. PostgreSQL: keep the
   migrated database as a `CREATE DATABASE` template (or `pg_dump` it).
3. **Clone per test.** Each test provisions its own isolated database by **restoring the backup** (SQL Server) or
   `CREATE DATABASE test_x TEMPLATE template_x` (PostgreSQL). Cloning does not touch `model` (SQL Server) and is
   dramatically cheaper than re-migrating.

Why it removes contention: the expensive, serialised "create plus migrate" now happens exactly once per test
suite. The per-test operation is either lock-free (SQL Server `RESTORE`) or only briefly serialised and very fast
(PostgreSQL `TEMPLATE`).

---

## 4. Reference implementation (SQL Server plus EF Core)

This is what we applied. Treat the class names as illustrative and map them onto your own harness.

### 4.1 One class, two roles

A single test factory/fixture plays two roles, distinguished by a nullable field (we called it
`TemplateDatabaseNameOverride`):

- **`null` (default): per-test factory.** Its database name is `test_{guid}`. The database is created by
  restoring the shared backup, and is dropped when the factory is disposed.
- **set: the template builder.** Its database name **is** the template name. In this role it only runs migrations
  (it must not try to build a template of its own), and it must **not** drop its database on dispose, because the
  caller still needs to back it up and then drop it explicitly.

The name resolver:

```csharp
private string DatabaseName => TemplateDatabaseNameOverride ?? $"test_{GUID:N}";
```

The dispose guard (only per-test databases are dropped):

```csharp
if (UseSqlServerContainer && IsInitialised && TemplateDatabaseNameOverride is null)
{
    await DropDatabase();
}
```

### 4.2 Build the template exactly once across threads

A `static ConcurrentDictionary<string, Lazy<Task>>` keyed per suite (we keyed by assembly name) guarantees the
template is built once even when many tests start concurrently. Every test awaits the **same** task.

```csharp
private static readonly ConcurrentDictionary<string, Lazy<Task>> TemplateTasks = new();
```

`Lazy<Task>` with the default thread-safety mode is the important part: only one thread executes the build
delegate; everyone else awaits the cached task.

### 4.3 Evict a faulted template build (critical robustness fix)

`Lazy<Task>` caches the **faulted** task if the build throws, so without eviction one transient failure poisons
every later test in the suite (they all re-await the same exception). Evict the exact cached entry on failure and
rethrow, so the next test rebuilds:

```csharp
Lazy<Task> templateTask = TemplateTasks.GetOrAdd(suiteKey, key =>
    new Lazy<Task>(() => CreateAndBackUpTemplateDatabase(templateDatabaseName, backupFileName)));

try
{
    await templateTask.Value;
}

catch
{
    // Evict only this entry (key plus value overload) so a concurrent retry's fresh entry is not clobbered.
    TemplateTasks.TryRemove(new KeyValuePair<string, Lazy<Task>>(suiteKey, templateTask));

    throw;
}

await RestoreDatabaseFromBackup(backupFileName);
```

### 4.4 Create and back up the template (SQL Server, run once)

1. `CREATE DATABASE [template_x]` (guarded with `IF DB_ID(...) IS NULL`).
2. Point the ORM/host at `template_x` and run migrations against it.
3. `ALTER DATABASE [template_x] SET RECOVERY SIMPLE` to keep the backup small and fast.
4. `BACKUP DATABASE [template_x] TO DISK = '/var/opt/mssql/data/template_x.bak' WITH FORMAT, INIT`.
5. Drop the template database; the `.bak` file is what persists for the suite:
   ```sql
   ALTER DATABASE [template_x] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
   DROP DATABASE [template_x];
   ```

### 4.5 Restore a per-test database from the backup (SQL Server, per test)

Read the logical file names from the backup, then restore relocating the physical files to **per-database** paths
so concurrent restores do not fight over the template's original file names:

```sql
RESTORE FILELISTONLY FROM DISK = '/var/opt/mssql/data/template_x.bak';
-- discover the logical data and log names, then:
RESTORE DATABASE [test_guid] FROM DISK = '/var/opt/mssql/data/template_x.bak'
WITH MOVE '<logical_data_name>' TO '/var/opt/mssql/data/test_guid.mdf',
     MOVE '<logical_log_name>'  TO '/var/opt/mssql/data/test_guid_log.ldf',
     REPLACE;
```

The `MOVE` clauses are essential. Without them every restore targets the template's original physical paths and
they collide.

### 4.6 Cleanup

- Drop each per-test database on dispose, forcing out pooled connections first:
  ```sql
  ALTER DATABASE [test_guid] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
  DROP DATABASE [test_guid];
  ```
- Make cleanup best-effort and log failures to stderr. The container is torn down at the end of the run anyway,
  but visible failures expose leaks.

---

## 5. Adapting to other engines

### PostgreSQL (native template, simplest)

- Build and migrate the template database once.
- Per test: `CREATE DATABASE "test_guid" TEMPLATE "template_x";` This is fast and needs no dump or restore.
- Constraint: **no active connections to the template** while a clone is created. Ensure the template builder
  closes its connection and that connection pooling to the template is disabled or drained before tests start
  cloning. If you see "being accessed by other users", serialise the `CREATE DATABASE ... TEMPLATE` calls behind a
  lock (they are fast, so serialising them is cheap).
- Drop per-test databases with force (PostgreSQL 13 and later): `DROP DATABASE "test_guid" WITH (FORCE);`.

### PostgreSQL (dump and restore alternative)

- `pg_dump` the migrated template once, `pg_restore` per test. Heavier than `TEMPLATE`; use only if the template
  connection constraint is awkward in your harness.

### MySQL or MariaDB

- No cheap native clone. Either `mysqldump` the template once and import per test, or prefer the data-reset option
  below if the schema is stable.

### Engine-agnostic alternative: data reset instead of per-test databases

If the schema changes rarely and tests mostly need a clean **data** slate, skip per-test database creation
entirely: keep one shared migrated database and isolate tests by either wrapping each test in a transaction that
rolls back, or wiping data between tests (for example the Respawn library on .NET). Trade-offs: a rollback-per-test
strategy prevents a test from committing and observing its own transactions, and parallel tests on one shared
database need careful data partitioning. The template-clone approach gives each test a real, independent database,
which is usually the more faithful and more parallel-friendly option.

---

## 6. Portable lessons (apply regardless of engine)

1. **Understand the test lifecycle first.** Determine whether your fixture is created per-test, per-class, or
   per-assembly, and how many run in parallel. We initially carried "reset state between reuses" logic that was
   dead code, because every test actually received a fresh fixture, so the reset path never ran. Match the
   solution to the real lifecycle; do not add reuse/reset paths that never execute.
2. **Know your container-sharing model.** One container per suite versus one shared across everything determines
   where the contention is and where the template and its backup must live.
3. **Build the template exactly once** (`Lazy<Task>` or equivalent one-time init), and make the cheap clone the
   only per-test database operation.
4. **Evict a faulted one-time build** so a single transient failure does not cascade into every subsequent test.
5. **Destructive database operations need exclusive access.** Force-disconnect before `DROP` or `RESTORE`
   (SQL Server `SET SINGLE_USER WITH ROLLBACK IMMEDIATE`; PostgreSQL `WITH (FORCE)`). Idle pooled connections
   still count as "in use".
6. **Unique per-test database names and unique physical file paths.** Reusing either causes collisions under
   parallelism.
7. **Never drop a template other tests still depend on.** Only per-test databases are dropped; the captured
   backup or template persists for the suite.
8. **Keep cleanup best-effort and logged.** The container dies at the end, but surfaced cleanup failures reveal
   leaks early.
9. **Match error codes, not message text, for any retry logic.** For example SQL Server error 1807 rather than a
   substring of the English message, which is fragile across locales and versions.
10. **Beware caching faulted async work in general.** Any "memoise this Task" pattern (`Lazy<Task>`, a cached
    field, a `ConcurrentDictionary<..., Task>`) caches failures too. Decide explicitly whether a failure should be
    cached or evicted.

---

## 7. Checklist for the target solution

- [ ] Identify the database engine and the Testcontainers image in use.
- [ ] Determine the fixture lifecycle, the degree of parallelism, and the container-sharing model.
- [ ] Confirm the symptom is **creation contention** (section 1), not data bleed between tests.
- [ ] Implement a build-once template: create one database, migrate it once, capture it.
- [ ] Implement a cheap per-test clone: `RESTORE` (SQL Server), `CREATE DATABASE ... TEMPLATE` (PostgreSQL), or
      dump-import.
- [ ] Add a one-time-build guard plus faulted-build eviction.
- [ ] Give every test a unique database name and unique physical file paths.
- [ ] Force exclusive access before any `DROP` or `RESTORE`.
- [ ] Drop per-test databases on dispose; never drop the template mid-run.
- [ ] Verify: the suite is green under full parallelism, the template is built exactly once, and per-test setup
      time has dropped noticeably.
