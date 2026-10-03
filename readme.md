<p align="center">
    <a href="https://github.com/Project-KONGOR-Open-Source/NEXUS/actions/workflows/run-unit-tests.yml"><img alt="Unit/Integration Tests" src="https://github.com/Project-KONGOR-Open-Source/NEXUS/actions/workflows/run-unit-tests.yml/badge.svg"></a>
    <img alt="Work Items" src="https://img.shields.io/github/issues/Project-KONGOR-Open-Source/NEXUS?label=Work%20Items&color=limegreen">
    <img alt="Code Contributors" src="https://img.shields.io/github/contributors/Project-KONGOR-Open-Source/NEXUS?label=Code%20Contributors&color=limegreen">
    <img alt="Total Commits" src="https://img.shields.io/github/commit-activity/t/Project-KONGOR-Open-Source/NEXUS?label=Total%20Commits&color=limegreen">
    <img alt="GitHub Sponsors" src="https://img.shields.io/github/sponsors/K-O-N-G-O-R?label=GitHub%20Sponsors&color=limegreen">
</p>

<h3>
    <p align="center">NEXUS</p>
    <p>The full suite of Project KONGOR services, architected as an open-source cloud-ready distributed application.</p>
    <p>If you would like to support the development of this project and buy me a coffee, please consider one of the following options: <a href="https://github.com/sponsors/K-O-N-G-O-R">GitHub Sponsors</a>, <a href="https://www.patreon.com/newerth">Patreon</a>, <a href="https://paypal.me/MissingLinkMedia">PayPal</a>. 💚</p>
</h3>

<hr/>

<h3 align="center">Terms And Conditions</h3>

> [!CAUTION]
> In addition to the clauses in the license attached to this code base, the Terms And Conditions below also apply.
> If the Terms And Conditions specified below conflict with any clauses in the license attached to this code base, the Terms And Conditions specified below take precedence.
> The purpose of these Terms And Conditions which complement the license is to allow any and every individual to host their own services for fun or for experimentation, while at the same time preventing large entities with many resources from converting any parts of this code base into a heartless business opportunity.
> `Project KONGOR always was and forever will be completely free.`

- no parts of this code base shall be used for any financial gain of any kind, including but not limited to direct financial gain, or indirect financial gain via any intermediary systems, mechanisms, platforms, processes, currencies, etc. of any kind
- usage of any parts of this code base for purposes beyond private or personal is strictly forbidden without the author's explicit consent in writing; this consent may be revoked by the author at any time, for any reason, and without any prior notice
- any entities using any parts of this code base for purposes beyond private or personal shall grant the author unrestricted access to databases, deployment servers, hosting servers, and any and all other infrastructure resources used for operational purposes

<hr/>

<h3 align="center">Required Tools</h3>

* .NET: https://dotnet.microsoft.com/

  the version needs to match the one used by the code base

* Docker: https://www.docker.com/

  install in one of the following ways:
  - manually, using the Docker Desktop installer: https://www.docker.com/products/docker-desktop/
  - with `winget` on Windows: `winget install --id=Docker.DockerDesktop --exact`
  - with online installation script, for Linux: `curl -fsSL https://get.docker.com | sh`

<hr/>

<h3 align="center">Optional Tools</h3>

* PowerShell Core: https://learn.microsoft.com/en-gb/powershell/
* Entity Framework Core Tools: https://www.nuget.org/packages/dotnet-ef/
* Aspire CLI: https://www.nuget.org/packages/Aspire.CLI/

> [!IMPORTANT]
> While these tools are not required, most or all guides and code snippets will prefer using them over other methods.

These tools are defined as dependencies in the `.config/dotnet-tools.json` manifest file, which makes them local tools. Local tools are scoped to the solution, and the manifest file pins an exact version for each tool, so that every development machine and every continuous integration run executes the same tool versions. In order to restore them, execute `dotnet tool restore` in the context of the solution directory.

```powershell
# In The Context Of The Solution Directory
dotnet tool restore
```

Restored local tools are invoked through the .NET CLI, by executing `dotnet {command}`, where `{command}` is the command name declared in the manifest file. The complete list of restored tools and of the commands they provide is available by executing `dotnet tool list --local`.

> [!NOTE]
> The `commands` array of each entry in the manifest file needs to match the command names which the tool package actually provides. If it does not, then the restore operation still reports success, but the command is unavailable; and if two entries declare the same command name, then the entry which comes first in the manifest file shadows the other.

To update the tools locally, execute `dotnet tool update --local {name}` for each tool, where `{name}` is the tool name, or just `dotnet tool update --all --local`. Updating a local tool rewrites the manifest file, which is tracked by source control, so the resulting change needs to be reviewed and committed like any other change.

```powershell
# Update A Single Tool, Where {name} Is The Tool Name
dotnet tool update --local {name}

# Update Every Tool
dotnet tool update --all --local
```

Optionally, but recommended on development machines, also install these tools globally with `dotnet tool install --global {name}` and keep them updated with `dotnet tool update --global {name}`, where `{name}` is the tool name, or just `dotnet tool update --all --global`. Global tools are invoked directly, without the `dotnet` prefix, and from any directory.

```powershell
# Install A Single Tool, Where {name} Is The Tool Name
dotnet tool install --global {name}

# Update A Single Tool, Where {name} Is The Tool Name
dotnet tool update --global {name}

# Update Every Tool
dotnet tool update --all --global
```

> [!NOTE]
> Global tools are scoped to the current user rather than to the solution, and they are not backed by a manifest file. There is consequently no global equivalent of the `dotnet tool restore` command, and no option to install every tool at once, so each global tool needs to be installed explicitly. The complete list of installed global tools is available by executing `dotnet tool list --global`.

<hr/>

<h3 align="center">Comprehensive Instructions For Developers</h3>

> [!IMPORTANT]
> The guides below invoke the Aspire CLI as `aspire`, which requires the tool to be installed globally. Both installations use the same tool package, but they differ in the way the command is exposed. A global installation generates an executable which is named after the command declared by the tool package, and places it in a directory which is on the `PATH`, so `aspire` resolves on its own and from any directory. A local installation generates no executable, and the tool is instead dispatched by the .NET CLI through the tool manifest, so the command resolves only as `dotnet aspire`, and only in the context of the solution directory.

> [!NOTE]
> The Entity Framework Core CLI is not affected by this distinction, because the command which it declares is already named `dotnet-ef`, which the .NET CLI resolves in both cases.

Run In Development ...

```powershell
# In The Context Of The Solution Directory
dotnet run --project ASPIRE.ApplicationHost --launch-profile "ASPIRE.ApplicationHost Development"
```

... Or Using The Aspire CLI

```powershell
# Quick Start With Automatic Application Host Detection
aspire run
```

```powershell
# ... And With Debug Logging And Attached Debugger
aspire run --debug --wait-for-debugger
```

> [!NOTE]
> There is currently no way to pass a launch profile to the `aspire run` command, so it will always use the first profile defined in `launchSettings.json`.

<br/>

Run In Production

```powershell
# In The Context Of The Solution Directory
dotnet run --project ASPIRE.ApplicationHost --launch-profile "ASPIRE.ApplicationHost Production"
```

<br/>

> [!NOTE]
> If no launch profile is specified, then the first profile in `launchSettings.json` will be used.
> Normally, this default profile is a development profile, which allows developers to run the project quickly by executing just `dotnet run` without specifying a launch profile.

<br/>

Create A Database Schema Migration

1. in the context of the solution directory, restore the Entity Framework Core CLI and the Aspire CLI by executing `dotnet tool restore`
2. in the context of the solution directory, execute `dotnet ef migrations add {NAME} --project MERRICK.DatabaseContext`

> [!NOTE]
> More information on Entity Framework Core in Aspire is available here: https://aspire.dev/integrations/databases/efcore/migrations.

<br/>

Update The Database Schema

The database schema is updated by invoking `Aspire.Hosting.EntityFrameworkCore` commands which target the `database-migrations` resource. The application host needs to be running during the execution of `Aspire.Hosting.EntityFrameworkCore` commands, so that the connection string which the `database-migrations` resource requires is resolved automatically from the application host. The application host must therefore be running, and the database which is updated is selected by the launch profile used to start the application host.

```powershell
# Development Database
aspire start --environment Development
aspire resource database-migrations ef-database-update
aspire stop
```

```powershell
# Production Database
aspire start --environment Production
aspire resource database-migrations ef-database-update
aspire stop
```

> [!NOTE]
> While updating the database happens automatically at run time, through code, it is still recommended to update databases manually from the command line, due to the significantly better debugging experience.

> [!NOTE]
> The `database-migrations` resource exposes multiple commands, which are listed by executing `aspire resource database-migrations --help`, including `ef-database-status`, `ef-migrations-add --name {NAME}`, `ef-migrations-remove`, `ef-database-update`, `ef-database-drop`, and `ef-database-reset`. The same commands are also available as buttons on the resource in the Aspire dashboard. Additional information can be discovered by exploring the `Aspire.Hosting.EntityFrameworkCore` integration [source code](https://github.com/microsoft/aspire/tree/main/src/Aspire.Hosting.EntityFrameworkCore).

<br/>

Update .NET Aspire

> [!NOTE]
> The Aspire NuGet packages referenced by the respective projects need to be in-sync with each other and with the Aspire SDK.

1. update the Aspire SDK and NuGet packages to the latest version by running `aspire update`
2. optionally (but good practice), ensure that the service defaults are on the latest version of the project template
    1. make sure that the latest Aspire project templates are installed by executing `dotnet new install Aspire.ProjectTemplates`
    2. optionally, update all project templates by executing `dotnet new update`
    3. make sure that the Aspire project templates are correctly installed, by executing `dotnet new list aspire --type project`
    4. create a temporary service defaults project by executing `dotnet new aspire-servicedefaults`
    5. copy the content of the generated extensions class over the already existing extensions class, and then delete the temporary project

> [!NOTE]
> Mode in-depth information is available at the following resources:
> - https://aspire.dev/get-started/prerequisites
> - https://aspire.dev/get-started/aspire-sdk-templates
> - https://aspire.dev/whats-new/upgrade-aspire

<br/>

Deploy To Azure

1. install or update the Azure Developer CLI

    ```powershell
    # powershell install
    winget install microsoft.azd
    ```

    ```bash
    # bash install
    curl -fsSL https://aka.ms/install-azd.sh | bash
    ```

    ```powershell
    # powershell update
    winget upgrade microsoft.azd
    ```

    ```bash
    # bash update
    curl -fsSL https://aka.ms/install-azd.sh | bash
    ```

2. in the context of the solution, execute `azd init` and follow the instructions
    a. application initialization type: `use code in the current directory`
    b. unique environment name: `nexus`

3. preview the IaC (infrastructure as code) definition, by executing `azd `azd infra synth``
    - if `azd infra synth` is not enabled, execute `azd config set alpha.infraSynth on`

4. execute `azd up` to provision the required resources and deploy the distributed application

> [!NOTE]
> Mode in-depth information is available here: https://learn.microsoft.com/en-gb/azure/developer/azure-developer-cli/reference.

<br/>

Debug HTTP Traffic With Fiddler

1. launch Project KONGOR in development mode, by using the `ASPIRE.ApplicationHost Development` profile
2. start the HoN client with WILLOWMAKER set to `127.0.0.1:8888` or the HoN client directly with the following command line parameters: `-masterserver 127.0.0.1:8888 -webserver 127.0.0.1:8888 -messageserver 127.0.0.1:8888`; to proxy the HoN server traffic through Fiddler, set the master server to `127.0.0.1:8888` in the COMPEL configuration file before spawning any servers
3. in Fiddler, in the bottom-left corner, make sure that the application type filter is set to `All Processes`
4. in Fiddler, in the bottom-left corner, disable traffic capturing, which removes the noise from implicitly captured traffic; anything explicitly sent to the Fiddler proxy with default port 8888 will still be captured
5. in Fiddler, go to `Rules > Customize Rules`, then `Go > to OnBeforeRequest`, and add `oSession.url = oSession.url.Replace("127.0.0.1:8888", "127.0.0.1:5555");` and `oSession.url = oSession.url.Replace("0.0.0.0:8888", "127.0.0.1:5555");` to forward traffic to the Project KONGOR development server once it's been captured

<br/>

Troubleshoot Port Conflicts On Windows

```powershell
# Verify Port Reservations
netsh int ipv4 show excludedportrange protocol=tcp

# Stop Windows NAT To Clear Dynamic Exclusions
net stop winnat

# Reserve HTTP Services Ports
netsh int ipv4 delete excludedportrange protocol=tcp startport=5550 numberofports=8 store=persistent
netsh int ipv4 add excludedportrange protocol=tcp startport=5550 numberofports=8 store=persistent

# Reserve TCP Server Ports
netsh int ipv4 delete excludedportrange protocol=tcp startport=11031 numberofports=3 store=persistent
netsh int ipv4 add excludedportrange protocol=tcp startport=11031 numberofports=3 store=persistent

# Reserve Match Server Ports
netsh int ipv4 delete excludedportrange protocol=tcp startport=11235 numberofports=5 store=persistent
netsh int ipv4 add excludedportrange protocol=tcp startport=11235 numberofports=5 store=persistent

# Reserve Match Server Voice Ports
netsh int ipv4 delete excludedportrange protocol=tcp startport=11435 numberofports=5 store=persistent
netsh int ipv4 add excludedportrange protocol=tcp startport=11435 numberofports=5 store=persistent

# Start Windows NAT
net start winnat

# NOTE: on deletion, make sure to delete the actual range reserved otherwise the delete operation will fail
```

<hr/>

<h3 align="center">Concise Instructions For Non-Developers</h3>

1. ??? (coming soon™)
2. Play HoN !

<hr/>

<h3 align="center">TODOs</h3>

- [ ] add instructions for publishing to Azure, Kubernetes, etc.
- [ ] support one-click start-up for non-technical users

<br/>
