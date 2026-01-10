<p align="center">
    <a href="https://github.com/Project-KONGOR-Open-Source/NEXUS/actions/workflows/run-unit-tests.yml"><img alt="Unit/Integration Tests" src="https://github.com/Project-KONGOR-Open-Source/NEXUS/actions/workflows/run-unit-tests.yml/badge.svg"></a>
    <img alt="Work Items" src="https://img.shields.io/github/issues/Project-KONGOR-Open-Source/NEXUS?label=Work%20Items&color=limegreen">
    <img alt="Code Contributors" src="https://img.shields.io/github/contributors/Project-KONGOR-Open-Source/NEXUS?label=Code%20Contributors&color=limegreen">
    <img alt="Total Commits" src="https://img.shields.io/github/commit-activity/t/Project-KONGOR-Open-Source/NEXUS?label=Total%20Commits&color=limegreen">
    <img alt="GitHub Sponsors" src="https://img.shields.io/github/sponsors/K-O-N-G-O-R?label=GitHub%20Sponsors&color=limegreen">
</p>

<h3>
    <p align="center">🎮 NEXUS 🎮</p>
    <p align="center">The ultimate suite of Project KONGOR services. Open-source, cloud-ready, and built for speed.</p>
    <p align="center">This is the MASTER SERVER code</p>
    <p align="center">
        <b>Support the Devs (Buy us a Potion 🧪):</b><br>
        <a href="https://github.com/sponsors/K-O-N-G-O-R">GitHub Sponsors</a> | 
        <a href="https://www.patreon.com/newerth">Patreon</a> | 
        <a href="https://paypal.me/MissingLinkMedia">PayPal</a> 💚
    </p>
</h3>

<hr/>

<h3 align="center">📜 Terms of Service (The Rules)</h3>

> [!CAUTION]
> **READ BEFORE PLAYING:** By accessing this code, you agree to the rules below. These supersede standard license clauses where they conflict.
>
> 🛑 **NO PAY-TO-WIN:** No financial gain allowed. You cannot sell, rent, or monetize this code or any derivative works.
>
> 🛡️ **PRIVATE SERVERS ONLY:** Personal/Private use only. You cannot run a public service without explicit written consent.
>
> 👁️ **ADMIN MODE:** If you use this code, you grant the author access to inspect your deployment/infrastructure.
>
> `Project KONGOR always was and forever will be completely free.`

<hr/>

<h3 align="center">🎒 Inventory (Required Tools)</h3>

* **.NET 10** 🛠️: [Download](https://dotnet.microsoft.com/)
* **Docker** 🐳: [Download](https://www.docker.com/)

<hr/>

<h3 align="center">🕹️ Developer's Guide (Cheatsheet)</h3>

#### 🏁 Press Start (Run in Development)

```powershell
# 🟢 LOCAL CO-OP MODE (Development Profile)
dotnet run --project ASPIRE.ApplicationHost --launch-profile "ASPIRE.ApplicationHost Development"
```

*Or use the Aspire CLI:*

```powershell
# ⚡ SPEED RUN
aspire run
```

```powershell
# 🐞 DEBUG MODE (Wait for Attach)
aspire run --debug --wait-for-debugger
```

<br/>

#### 🚀 Launch to Production

```powershell
# 🔴 RANKED MODE (Production Profile)
dotnet run --project ASPIRE.ApplicationHost --launch-profile "ASPIRE.ApplicationHost Production"
```

<br/>

#### 💾 Save Data (Database Migrations)

**Create New Save File (Migration):**
1. Restore tools: `dotnet tool restore`
2. Save Game: `aspire exec --resource database-context -- dotnet ef migrations add {NAME}`

**Load Save File (Update Database):**

```powershell
# 🛠️ DEV REALM
$ENV:ASPNETCORE_ENVIRONMENT = "Development"
aspire exec --resource database-context -- dotnet ef database update
```

```powershell
# 🌍 PROD REALM
$ENV:ASPNETCORE_ENVIRONMENT = "Production"
aspire exec --resource database-context -- dotnet ef database update
```

> [!TIP]
> Always manually save/update via command line for better loot drops (debugging info).

<br/>

#### ☁️ Deploy to Cloud (Azure Realm)

1. **Equip Azure CLI:** `winget install microsoft.azd`
2. **Initialize Quest:** `azd init` (Choose `use code in current directory`, Name: `nexus`)
3. **Scout Terrain:** `azd infra synth`
4. **Deploy Base:** `azd up`

<br/>

#### 🔌 Network Troubleshooting (Port Forwarding)

Fix those pesky firewall blockers on Windows:

```powershell
# 🛑 STOP NAT (Clear Path)
net stop winnat

# 🔓 UNLOCK HTTP PORTS
netsh int ipv4 delete excludedportrange protocol=tcp startport=5550 numberofports=8 store=persistent
netsh int ipv4 add excludedportrange protocol=tcp startport=5550 numberofports=8 store=persistent

# 🔓 UNLOCK GAME PORTS
netsh int ipv4 delete excludedportrange protocol=tcp startport=11031 numberofports=3 store=persistent
netsh int ipv4 add excludedportrange protocol=tcp startport=11031 numberofports=3 store=persistent

# 🔓 UNLOCK MATCH PORTS
netsh int ipv4 delete excludedportrange protocol=tcp startport=11235 numberofports=5 store=persistent
netsh int ipv4 add excludedportrange protocol=tcp startport=11235 numberofports=5 store=persistent

# 🔓 UNLOCK VOICE PORTS
netsh int ipv4 delete excludedportrange protocol=tcp startport=11435 numberofports=5 store=persistent
netsh int ipv4 add excludedportrange protocol=tcp startport=11435 numberofports=5 store=persistent

# 🟢 RESTART NAT
net start winnat
```

<hr/>

<h3 align="center">🏆 GG WP (Dedication)</h3>

<p align="center">
    <b>This project is a labor of love.</b> ❤️<br><br>
    We built this purely to keep the game alive for the dedicated community who have poured thousands of hours into the lanes of Newerth.<br>
    Our mission is simple: ensure that the players can continue to enjoy the game they love, today and forever.<br><br>
    <i>"Legends never die."</i>
</p>

<hr/>
