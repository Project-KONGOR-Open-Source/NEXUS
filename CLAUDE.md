# NEXUS Development Guidelines

Auto-generated from all feature plans. Last updated: 2025-01-16

## ⚠️ MANDATORY: Before Making ANY Code Edits

**Constitutional Requirement (Principle IV - v1.2.0):**
- **ALWAYS** read `.github/agents/copilot-instructions.md` before making ANY code modifications
- This file is the authoritative source of truth for code style
- The file updates frequently - NEVER rely on cached/memorized versions
- These principles take precedence over all other guidance

See `.specify/memory/constitution.md` (v1.2.0) for full constitutional principles.

## Active Technologies
- C# with .NET 10 + .NET Aspire, Entity Framework Core, System.Net.Sockets, System.Diagnostics.Metrics (spec-kit)
- SQL Server via MERRICK.DatabaseContext (EF Core), In-memory ConcurrentDictionary for transient state, Redis caching via Aspire (spec-kit)

**Language/Framework:**
- C# with .NET 10 (or version specified in solution)
- .NET Aspire for distributed application orchestration
- Entity Framework Core for data persistence

**Primary Services:**
- **ASPIRE.ApplicationHost**: Application host for orchestration
- **KONGOR.MasterServer**: REST API for game client/server communication
- **TRANSMUTANSTEIN.ChatServer**: TCP server for real-time chat (HIGHEST PRIORITY)
- **ZORGATH.WebPortal.API**: Web portal backend API
- **DAWNBRINGER.WebPortal.UI**: Web portal frontend
- **MERRICK.DatabaseContext**: Database context for EF Core

**Testing:**
- xUnit in ASPIRE.Tests project

**Platform:**
- Cross-platform (Windows/Linux) with Docker support

## Project Structure

```text
NEXUS/
├── ASPIRE.ApplicationHost/              # Aspire orchestration host
├── ASPIRE.Common/               # Shared common utilities
├── ASPIRE.Tests/                # Test project (xUnit)
├── KONGOR.MasterServer/         # Master Server (REST API)
├── TRANSMUTANSTEIN.ChatServer/  # Chat Server (TCP) - PRIORITY
├── ZORGATH.WebPortal.API/       # Web Portal API
├── DAWNBRINGER.WebPortal.UI/    # Web Portal UI
├── MERRICK.DatabaseContext/     # Database context (EF Core)
└── LEGACY/                      # Legacy reference code
    ├── HoN/                     # Original HoN source (ABSOLUTE TRUTH)
    └── KONGOR/                  # Production KONGOR code (PRACTICAL REFERENCE)
```

## Commands

**Development:**
```powershell
# Run in development mode
dotnet run --project ASPIRE.ApplicationHost --launch-profile "ASPIRE.ApplicationHost Development"

# Or using Aspire CLI (auto-detects ApplicationHost)
aspire run

# With debugging
aspire run --debug --wait-for-debugger
```

**Database Migrations:**
```powershell
# Create migration (requires Aspire host running)
aspire exec --resource database-context -- dotnet ef migrations add {NAME}

# Update development database
$ENV:ASPNETCORE_ENVIRONMENT = "Development"
aspire exec --resource database-context -- dotnet ef database update

# Update production database
$ENV:ASPNETCORE_ENVIRONMENT = "Production"
aspire exec --resource database-context -- dotnet ef database update
```

**Testing:**
```powershell
# Run all tests
dotnet test

# Run specific test project
dotnet test ASPIRE.Tests
```

**Tools Management:**
```powershell
# Restore .NET tools (EF Core CLI, Aspire CLI, etc.)
dotnet tool restore

# Update all local tools
dotnet tool update --all --local
```

## Code Style

**⚠️ CRITICAL: Always consult `.github/agents/copilot-instructions.md` for the latest rules**

**C# Conventions Summary (verify against source file before editing):**

- **NEVER use "var"** - always use explicit type names
- **Acronyms/Initialisms**:
  - PascalCase: Always uppercase (UserID, GetGUID, HTMLParser)
  - camelCase: Uppercase only if not at start (userGUID, accountID, httpStatusCode)
- **Lambda parameters**: Full variable names, never single letters
  - ✅ `numbers.Select(number => number * number)`
  - ❌ `numbers.Select(x => x * x)`
- **Switch expressions**: Align lambda operators
- **Comments**: StartCase for comments (Every Word Capitalised), sentence case for XML summaries
- **Symbol names**: Always use full words, NEVER abbreviations
  - ✅ configuration, administrator, implementation
  - ❌ config, admin, impl
- **Symbol references**: Use `<see cref="MethodName"/>` (no parameters in tag)
- **Null-forgiving operator (!)**: Only for EF navigation properties or truly unavoidable cases
- **"Async" suffix**: Only when synchronous version exists
- **Language**: British English throughout (colour, behaviour, serialise)

**Architecture Conventions:**
- Services use MERRICK.DatabaseContext for persistence
- All services registered in ASPIRE.ApplicationHost
- Follow Aspire patterns for configuration and observability
- Protocol implementations maintain HoN/KONGOR compatibility

## Legacy Code Reference

**Hierarchy of Truth:**
1. **HoN** (C:\Users\SADS-810\Source
EXUS\LEGACY\HoN): Absolute source of truth for protocols
2. **KONGOR** (C:\Users\SADS-810\Source
EXUS\LEGACY\KONGOR): Practical production reference

**When consulting legacy code:**
- Document behavior and intent, not just implementation
- Identify technical debt or issues in legacy approach
- Consider modern .NET patterns for implementation
- Honor protocol structures, modernize implementation
- Keep traceability between legacy and NEXUS components

## Chat Server Priority

**HIGHEST PRIORITY**: TRANSMUTANSTEIN.ChatServer implementation

**Requirements:**
- Document event flows as discovered
- Validate protocol against both HoN and KONGOR
- Test real-time performance
- Log all protocol interactions for debugging
- Research undocumented flows thoroughly

## Recent Changes

### 2025-11-16
- **Constitution v1.2.0**: Added MANDATORY LEGACY VERIFICATION requirement to Principle I
  - All protocol-related tasks MUST verify against HoN and KONGOR before completion
  - Task completion notes MUST include references to verified legacy code locations
- **Task T054**: Completed clan channel validation in ChatChannel.Join() with legacy-verified silent rejection behavior
- **Codebase Cleanup**: Removed 13 empty source files and 9 empty directories
- **Class Renaming**: SilenceUser → SilenceChannelMember, ChannelMessage → SendChannelMessage

### 2025-01-16
- **Constitution v1.1.0**: Added MANDATORY pre-edit check requirement for copilot-instructions.md
- **copilot-instructions.md**: Added "full words for symbols" rule (no abbreviations like admin, config, impl)
- **Code Style**: Updated all session code to comply with StartCase comments and full word requirements
- **Tasks T045-T049**: Completed channel command processors (JoinChannel, LeaveChannel, ChannelMessage, KickFromChannel, SilenceUser)
- **ChatChannel**: Added silence tracking with IsSilenced() and Silence() methods

### 2025-01-13
- spec-kit: Added C# with .NET 10 + .NET Aspire, Entity Framework Core, System.Net.Sockets, System.Diagnostics.Metrics
- Constitution v1.0.0: Initial ratification with 7 core principles

<!-- MANUAL ADDITIONS START -->
<!-- MANUAL ADDITIONS END -->
