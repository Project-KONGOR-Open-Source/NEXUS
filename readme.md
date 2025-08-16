<p align="center">
    <a href="https://github.com/Project-KONGOR-Open-Source/NEXUS/actions/workflows/dotnet.yml"><img alt="Unit/Integration Tests" src="https://github.com/Project-KONGOR-Open-Source/NEXUS/actions/workflows/dotnet.yml/badge.svg"></a>
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

<h3 align="center">Comprehensive Instructions For Developers</h3>

TODO: Update These Commands With Aspire CLI Versions

Run In Development

```powershell
# In The Context Of The Solution Directory
dotnet run --project ASPIRE.AppHost --launch-profile "Project KONGOR Development"
```

<br/>

Run In Production

```powershell
# In The Context Of The Solution Directory
dotnet run --project ASPIRE.AppHost --launch-profile "Project KONGOR Production"
```

<br/>

> [!NOTE]
> If no launch profile is specified, then the first profile in `launchSettings.json` will be used.
> Normally, this default profile is a development profile, which allows developers to run the project quickly by executing just `dotnet run` without specifying a launch profile.

<br/>

Create A Database Schema Migration And Update The Database

1. install the Entity Framework Core CLI by executing `dotnet tool install --global dotnet-ef` or update it by executing `dotnet tool update --global dotnet-ef`
2. in the context of the solution directory, execute `dotnet ef migrations add {MigrationName} --project MERRICK.Database`
3. in the context of the solution directory, execute `dotnet ef database update --project MERRICK.Database`

<br/>

Update The Database Schema

```powershell
# Development Database
# In The Context Of The Solution Directory
$ENV:ASPNETCORE_ENVIRONMENT = "Development"
dotnet ef database update --project MERRICK.Database
```

```powershell
# Production Database
# In The Context Of The Solution Directory
$ENV:ASPNETCORE_ENVIRONMENT = "Production"
dotnet ef database update --project MERRICK.Database
```

<br/>

> [!NOTE]
> Updating the database schema manually is only required during development, when there is a potential for database schema migrations to fail.
> If the migrations are stable, then manually updating the database can be skipped, as that happens automatically at runtime.
> This also means that a database will be fully scaffolded at runtime if it does not already exist.

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

1. launch Project KONGOR in development mode, by using the `ASPIRE.AppHost Development` profile
2. start the HoN client with the following command line parameters: `-masterserver 127.0.0.1:8888 -webserver 127.0.0.1:8888 -messageserver 127.0.0.1:8888`; to proxy the the HoN server through Fiddler, set the master server to `127.0.0.1:8888` in the COMPEL configuration file
3. in Fiddler, in the bottom-left corner, make sure that the application type filter is set to `All Processes`
4. in Fiddler, click in the bottom-left corner to disable traffic capturing, which removes the noise from implicitly captured traffic; anything explicitly sent to the Fiddler proxy with default port 8888 will still be captured
5. in Fiddler, go to `Rules > Customize Rules`, then `Go > to OnBeforeRequest`, and add `oSession.url = oSession.url.Replace("127.0.0.1:8888", "127.0.0.1:55555");` and `oSession.url = oSession.url.Replace("0.0.0.0:8888", "127.0.0.1:55555");` to forward traffic to the Project KONGOR development server once it's been captured

<hr/>

<h3 align="center">Concise Instructions For Non-Developers</h3>

1. ??? (coming soon™)
2. Play HoN !

<hr/>

<h3 align="center">TODOs</h3>

- [ ] add instructions for publishing to Azure, Kubernetes, etc.
- [ ] support one-click start-up for non-technical users

<br/>
