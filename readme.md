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

<h3 align="center">Non-Code Dependencies</h3>

* SQL Server (Developer Edition): https://www.microsoft.com/en-gb/sql-server/sql-server-downloads/
* Docker (Personal Edition): https://www.docker.com/products/docker-desktop/ (Docker Desktop)

> [!IMPORTANT]
> The editions suggested in parentheses are for development purposes only.
> Commercial usage will likely require paid tiers of these software products.

<hr/>

<h3 align="center">Concise Instructions For Developers</h3>

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

Create A Database Schema Migration

1. install the Entity Framework Core CLI by executing `dotnet tool install --global dotnet-ef` or update it by executing `dotnet tool update --global dotnet-ef`
2. in the context of the solution directory, execute `dotnet ef migrations add {MigrationName} --project MERRICK.Database`

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

Generate .NET Aspire Deployment Artefacts & Deploy To Azure/Kubernetes

> [!NOTE]
> Azure deployments require the [Azure Developer CLI](https://github.com/Azure/azure-dev), and Kubernetes deployments require [Aspir8](https://github.com/prom3theu5/aspirational-manifests).
> The documentation of each tool should be inspected for more complex deployment configurations.

```powershell
# Azure
winget install microsoft.azd

# In The Context Of The ASPIRE.AppHost Project Directory
azd init
azd up
```

```powershell
# Kubernetes
dotnet tool install -g aspirate

# In The Context Of The ASPIRE.AppHost Project Directory
aspirate init
aspirate generate
aspirate apply
```

```powershell
# Manifest
# In The Context Of The Solution Directory
dotnet run --project ASPIRE.AppHost\ASPIRE.AppHost.csproj -- --publisher manifest --output-path manifest.json
```

<br/>

Install/Update .NET Aspire

> [!NOTE]
> The Aspire NuGet packages referenced by the respective projects need to be in-sync with the Aspire dotnet workload.

```powershell
# Install
dotnet workload install aspire
```

```powershell
# Update
dotnet workload update
```

```powershell
# Check The Installed Version
dotnet workload list
```

> [!NOTE]
> Mode in-depth information is available here: https://learn.microsoft.com/en-gb/dotnet/aspire/fundamentals/setup-tooling.

<br/>

Debug HTTP Traffic With Fiddler

1. launch Project KONGOR in development mode, by using the `ASPIRE.AppHost Development` profile
2. start the HoN client with the following command line parameters: `-masterserver 127.0.0.1:8888 -webserver 127.0.0.1:8888 -messageserver 127.0.0.1:8888`; to proxy the the HoN server through Fiddler, set the master server to `127.0.0.1:8888` in the COMPEL configuration file
3. in Fiddler, in the bottom-left corner, make sure that the application type filter is set to `All Processes`
4. in Fiddler, click in the bottom-left corner to disable traffic capturing, which removes the noise from implicitly captured traffic; anything explicitly sent to the Fiddler proxy with default port 8888 will still be captured
5. in Fiddler, go to `Rules > Customize Rules`, then `Go > to OnBeforeRequest`, and add `oSession.url = oSession.url.Replace("127.0.0.1:8888", "127.0.0.1:55555");` and `oSession.url = oSession.url.Replace("0.0.0.0:8888", "127.0.0.1:55555");` to forward traffic to the Project KONGOR development server once it's been captured

<hr/>

<h3 align="center">Comprehensive Instructions For Non-Developers</h3>

1. ??? (coming soon™)
2. Play HoN !

<br/>
