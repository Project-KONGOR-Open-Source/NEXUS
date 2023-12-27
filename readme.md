<h3>
    <p align="center">ASPIRE</p>
    <p>The full suite of Project KONGOR services, architected as an open-source cloud-ready distributed application.</p>
    <p>If you would like to support the development of this project and buy me a coffee, please consider one of the following options: <a href="https://github.com/sponsors/K-O-N-G-O-R">GitHub Sponsors</a>, <a href="https://paypal.me/MissingLinkMedia">PayPal</a>. 💚</p>
</h3>

<hr/>

<h3 align="center">Concise Instructions For Developers</h3>

> [!NOTE]
> Non-Code Dependencies
> * SQL Server (Developer Edition): https://www.microsoft.com/en-gb/sql-server/sql-server-downloads/
> * Docker Desktop: https://www.docker.com/products/docker-desktop/ (preferably, using the WSL 2 engine)

<br/>

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

NOTE: If no launch profile is specified, then the first profile in `launchSettings.json` will be used. Normally, this default profile is a development profile, which allows developers to run the project quickly by executing just `dotnet run` without specifying a launch profile.

<br/>

Create A Database Schema Migration

1. install the Entity Framework Core CLI by executing `dotnet tool install --global dotnet-ef` or update it by executing `dotnet tool update --global dotnet-ef`
2. in the context of the solution directory, execute `dotnet ef migrations add {MigrationName} --project MERRICK.Database.Manager`

<br/>

Update The Database

```powershell
# Development Database
# In The Context Of The Solution Directory
$ENV:ASPNETCORE_ENVIRONMENT = "Development"
dotnet ef database update --project MERRICK.Database.Manager
```

```powershell
# Production Database
# In The Context Of The Solution Directory
$ENV:ASPNETCORE_ENVIRONMENT = "Production"
dotnet ef database update --project MERRICK.Database.Manager
```

<hr/>

<h3 align="center">Comprehensive Instructions For Non-Developers</h3>

1. ??? (coming soon™)
2. play HoN
<br/>
