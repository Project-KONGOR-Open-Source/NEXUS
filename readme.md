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
    <p>If you would like to support the development of this project and buy me a coffee, please consider one of the following options: <a href="https://github.com/sponsors/K-O-N-G-O-R">GitHub Sponsors</a>, <a href="https://paypal.me/MissingLinkMedia">PayPal</a>. 💚</p>
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

These tools are defined as dependencies in the `.config/dotnet-tools.json` file. In order to restore them, execute `dotnet tool restore` in the context of the solution directory.

To update the tools locally, execute `dotnet tool update --local {name}` for each tool, where `{name}` is the tool name, or just `dotnet tool update --all --local`.

Optionally, but recommended on development machines, also install these tools globally with `dotnet tool install --global {name}` and keep them updated with `dotnet tool update --global {name}`, where `{name}` is the tool name, , or just `dotnet tool update --all --global`.

<hr/>

<h3 align="center">AI Tools</h3>

* Claude Code: https://claude.ai/

  ```powershell
  # Add MCP Servers

  claude mcp add --transport http microsoft-learn https://learn.microsoft.com/api/mcp
  claude mcp add --transport http context7 https://mcp.context7.com/mcp --header "CONTEXT7_API_KEY: YOUR_API_KEY"

  claude mcp list
  ```

<hr/>

<h3 align="center">Comprehensive Instructions For Developers</h3>

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

1. restore the Entity Framework Core CLI and the Aspire CLI by executing `dotnet tool restore`
2. in the context of the solution directory, execute `aspire exec --resource database-context -- dotnet ef migrations add {NAME}`

> [!NOTE]
> Because the code-first database project is an Aspire resource, it needs the Aspire application host to be running when managing migrations and updating the database, so that Entity Framework Core can gain awareness of resources generated dynamically at run time, such as the connection string. Therefore, it is not possible to run `dotnet ef` commands directly against such a project, because on its own it doesn't have awareness of how to connect to the database server, since this information is passed downstream by the application host at run time.
> More information on resource-aware CLI commands is available here: https://learn.microsoft.com/en-gb/dotnet/aspire/cli-reference/aspire-exec.

<br/>

Update The Database Schema

```powershell
# Development Database
# In The Context Of The Solution Directory
$ENV:ASPNETCORE_ENVIRONMENT = "Development"
aspire exec --resource database-context -- dotnet ef database update
```

```powershell
# Production Database
# In The Context Of The Solution Directory
$ENV:ASPNETCORE_ENVIRONMENT = "Production"
aspire exec --resource database-context -- dotnet ef database update
```

> [!NOTE]
> While updating the database happens automatically at run time, through code, it is still recommended to update databases manually from the command line, due to the significantly better debugging experience.

<br/>

Install/Update .NET Aspire

> [!NOTE]
> The Aspire NuGet packages referenced by the respective projects need to be in-sync with each other and with the Aspire SDK.

1. update the Aspire NuGet packages to the latest version
2. manually (for now), update the Aspire SDK in the application host project file
3. optionally (but good practice), ensure that the service defaults are on the latest version of the project template
    1. make sure that the latest Aspire project templates are installed by executing `dotnet new install Aspire.ProjectTemplates@X.Y.Z --force`, where `X.Y.Z` is the required Aspire version, which would ideally be the latest released version
    2. optionally, update all project templates by executing `dotnet new update`
    3. make sure that the Aspire project templates are correctly installed, by executing `dotnet new list aspire --type project`
    4. create a temporary service defaults project by executing `dotnet new aspire-servicedefaults`
    5. copy the content of the generated extensions class over the already existing extensions class, and then delete the temporary project

> [!NOTE]
> Mode in-depth information is available here: https://learn.microsoft.com/en-gb/dotnet/aspire/fundamentals/setup-tooling.

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
2. start the HoN client with the following command line parameters: `-masterserver 127.0.0.1:8888 -webserver 127.0.0.1:8888 -messageserver 127.0.0.1:8888`; to proxy the the HoN server through Fiddler, set the master server to `127.0.0.1:8888` in the COMPEL configuration file
3. in Fiddler, in the bottom-left corner, make sure that the application type filter is set to `All Processes`
4. in Fiddler, click in the bottom-left corner to disable traffic capturing, which removes the noise from implicitly captured traffic; anything explicitly sent to the Fiddler proxy with default port 8888 will still be captured
5. in Fiddler, go to `Rules > Customize Rules`, then `Go > to OnBeforeRequest`, and add `oSession.url = oSession.url.Replace("127.0.0.1:8888", "127.0.0.1:55555");` and `oSession.url = oSession.url.Replace("0.0.0.0:8888", "127.0.0.1:55555");` to forward traffic to the Project KONGOR development server once it's been captured

<br/>

Troubleshoot Port Conflicts On Windows

```powershell
# Stop Windows NAT To Clear Dynamic Exclusions
net stop winnat

# Reserve TCP Server Ports
netsh int ipv4 add excludedportrange protocol=tcp startport=11031 numberofports=3 store=persistent

# Reserve HTTP Services Ports
netsh int ipv4 add excludedportrange protocol=tcp startport=55550 numberofports=8 store=persistent

# Start Windows NAT
net start winnat
```

<hr/>

<h3 align="center">Discord Authentication Configuration</h3>

To enable Discord authentication in development, you must configure **credentials** and the **callback URL**:

1.  **Credentials**: Configure Aspire User Secrets for the `ASPIRE.ApplicationHost` project.
    It is easy to do this through the dashboard or you can 
    Run the following commands in the `NEXUS/ASPIRE.ApplicationHost` directory:
    ```powershell
    dotnet user-secrets init
    dotnet user-secrets set "Parameters:discord-client-id" "YOUR_CLIENT_ID"
    dotnet user-secrets set "Parameters:discord-client-secret" "YOUR_CLIENT_SECRET"
    ```

2.  **Redirect URI**: In the Discord Developer Portal, add this Redirect URI:
    ```
    https://localhost:55556/Auth/Discord/Callback
    ```

3.  **Scopes**: Ensure the following scopes are selected:
    *   `identify`
    *   `email`

<hr/>

<h3 align="center">User Accounts & Matchmaking Eligibility</h3>

NEXUS implements a tiered account system to ensure matchmaking quality. Account types are automatically assigned based on Discord verification status during registration and login.

| Account Type | ID | Condition | Queue Eligibility |
| :--- | :--- | :--- | :--- |
| **Trial** | `1` | Default / Unverified | <span style="color:red">restricted</span> |
| **Normal** | `3` | **Verified Email** on Discord | ✅ **Public Matches** |
| **Legacy** | `4` | **MFA Enabled** on Discord | ✅ **Public & Ranked Matches** |

> [!NOTE]
> Existing Legacy/Staff/VIP accounts are not downgraded. New accounts start as Trial and upgrade automatically upon verification.
> Link your Discord account to upgrade your eligibility!

<hr/>

<h3 align="center">Concise Instructions For Non-Developers</h3>

1. ??? (coming soon™)
2. Play HoN !

<hr/>

<h3 align="center">TODOs</h3>

- [ ] add instructions for publishing to Azure, Kubernetes, etc.
- [ ] support one-click start-up for non-technical users

<br/>
