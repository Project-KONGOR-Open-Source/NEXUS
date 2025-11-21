<!--
Sync Impact Report:
Version: 1.1.0 → 1.2.0
Rationale: Enhanced Principle I to mandate legacy code verification for protocol tasks (MINOR bump for new mandatory requirement)

Changes:
- ENHANCED: Principle I - Added MANDATORY LEGACY VERIFICATION requirement
  - Agents MUST search LEGACY/HoN and LEGACY/KONGOR before implementing protocol tasks
  - MUST document legacy behavior including error handling and edge cases
  - MUST implement matching behavior unless compelling reason exists
  - Task completion notes MUST include references to verified legacy code locations
  - Verification MUST be performed BEFORE task completion, not after

Previous Version (1.1.0):
- Enhanced Principle IV with mandatory pre-edit check of copilot-instructions.md
- Added requirement to never rely on cached/memorized conventions

Previous Version (1.0.0):
- Initial constitution with 7 core principles
- Legacy parity, service architecture, chat priority
- Code style, security, simplicity, testing
- Governance framework established

Templates requiring updates:
⚠️ tasks.md - Task descriptions may benefit from explicit legacy verification checkpoints
✅ Other templates - No changes required

Follow-up TODOs:
- Consider adding legacy verification checklist to protocol implementation tasks
-->

# NEXUS Constitution

## Core Principles

### I. Legacy Parity and Source of Truth

The NEXUS project is a modernization of the KONGOR codebase, which itself replaces the original HoN (Heroes of Newerth) backend services. This principle establishes our hierarchy of truth and approach to legacy code:

**MANDATORY LEGACY VERIFICATION:** Before implementing ANY protocol-related task (authentication, channels, matchmaking, friends, clans, game server coordination, etc.), agents and developers MUST:
1. Search LEGACY/HoN and LEGACY/KONGOR directories for equivalent functionality
2. Document the legacy behavior, including error handling and edge cases
3. Implement matching behavior unless there is a compelling modernization reason
4. Document any intentional deviations with clear rationale

**Rules:**
- HoN source code (C:\Users\SADS-810\Source\NEXUS\LEGACY\HoN) is the ABSOLUTE source of truth for protocol behavior, message formats, and game mechanics
- KONGOR source code (C:\Users\SADS-810\Source\NEXUS\LEGACY\KONGOR) is the PRACTICAL reference, having run in production successfully for years
- When HoN and KONGOR conflict, HoN takes precedence unless KONGOR's approach is demonstrably superior
- All client-server protocol implementations MUST maintain backward compatibility with existing game clients and servers
- Enhancements to responses are permitted only when they improve functionality without breaking existing behavior
- When behavior is undocumented, it MUST be extrapolated from both KONGOR implementation and HoN source code
- Legacy verification MUST be performed BEFORE completing the task, not after
- Task completion notes MUST include references to verified legacy code locations

**Rationale:** The project's success depends on maintaining protocol compatibility while modernizing the codebase. HoN represents the original design intent, while KONGOR represents proven production reliability. Without systematic verification against legacy implementations, we risk introducing incompatibilities that break existing clients.

### II. Service Architecture

NEXUS is a distributed cloud-ready application built on .NET Aspire, consisting of three primary service types:

**Rules:**
- Services MUST be independently deployable and scalable
- Each service MUST have clear boundaries and responsibilities:
  - **Master Server** (KONGOR.MasterServer): REST API for game client and game server communication
  - **Chat Server** (TRANSMUTANSTEIN.ChatServer): TCP server for real-time communication (HIGHEST PRIORITY)
  - **Web Portal API** (ZORGATH.WebPortal.API): Web services for portal functionality
  - **Web Portal UI** (DAWNBRINGER.WebPortal.UI): Frontend interface
- Services MUST use the database context (MERRICK.DatabaseContext) for data persistence
- All services MUST be registered in the Aspire ApplicationHost (ASPIRE.ApplicationHost)
- Services MUST follow the Aspire architecture patterns for observability, health checks, and configuration

**Rationale:** The distributed architecture enables independent scaling, deployment, and maintenance of each component while ensuring the system can handle production workloads.

### III. Chat Server Priority (NON-NEGOTIABLE)

The Chat Server implementation is the current highest priority and requires special attention:

**Rules:**
- Chat Server work takes precedence over all other features unless explicitly overridden
- Flow of events MUST be documented as they are discovered/implemented
- Protocol behavior MUST be validated against both HoN and KONGOR implementations
- Real-time communication patterns MUST be tested for correctness and performance
- All chat protocol interactions MUST be logged at appropriate levels for debugging
- Undocumented flows MUST be researched thoroughly before implementation

**Rationale:** The chat server is the most complex component with poorly documented event flows. Its proper functioning is critical for player experience and requires careful analysis of legacy code.

### IV. Code Style and Formatting (NON-NEGOTIABLE)

**MANDATORY PRE-EDIT CHECK:** Before making ANY code edits, agents and developers MUST read and apply the principles from `.github/copilot-instructions.md`. This file is the authoritative source of truth for code style and MUST be consulted before every editing session.

**Rules:**
- `.github/copilot-instructions.md` MUST be checked before ANY code modifications
- ALL principles in that file take precedence over default behaviors or memorized conventions
- The file updates frequently - NEVER rely on cached/memorized versions
- If conflicting guidance exists between this constitution and copilot-instructions.md, the copilot-instructions.md file wins for code style matters
- Key conventions (as of last constitution update, but ALWAYS verify in source file):
  - NEVER use "var" - always use explicit type names
  - Acronyms and initialisms: uppercase in PascalCase, conditional in camelCase (UserID, userGUID, HTTPParser, accountID)
  - Full variable names for delegates and lambda parameters - NO single letters (number => not x =>)
  - Consistent vertical whitespace with existing code
  - Aligned lambda operators in switch expressions
  - Comments only when adding value - use StartCase for comments, sentence case for XML summaries
  - Symbol names in double quotes or `<see cref="..."/>` tags without parameters
  - Null-forgiving operator (!) only for EF navigation properties or truly unavoidable cases
  - "Async" suffix only when synchronous version exists
  - British English spelling throughout

**Rationale:** The copilot-instructions.md file contains frequently-updated coding standards that must be consistently applied. By mandating a fresh read before each editing session, we ensure compliance with the latest conventions and prevent style drift.

### V. Security and Correctness

Code MUST be secure, correct, and maintainable:

**Rules:**
- MUST prevent OWASP Top 10 vulnerabilities: SQL injection, XSS, command injection, etc.
- MUST verify syntactic correctness and adherence to latest C#/.NET standards
- MUST follow best practices for performance optimization unless readability significantly suffers
- Generated code MUST be consistent with existing codebase style and architecture
- MUST inspect existing codebase patterns before implementing new features
- Code MUST be targeted and avoid unnecessary complexity (YAGNI principle)

**Rationale:** Security vulnerabilities and correctness issues in a game backend can lead to exploits, data breaches, and poor player experience.

### VI. Simplicity and Maintainability

The codebase MUST remain simple and maintainable:

**Rules:**
- Prefer simplicity and clarity over complexity
- Avoid over-engineering and unnecessary features not explicitly requested
- Keep code straightforward and easy to understand
- Minimize abstractions, layers, and components that don't add significant value
- Present options to users when multiple approaches exist
- Justify any complexity that is introduced

**Rationale:** NEXUS is a modernization effort - the goal is to make the codebase MORE maintainable, not less. Unnecessary complexity makes the system harder to understand, debug, and extend.

### VII. Testing and Validation

Quality MUST be verified through appropriate testing:

**Rules:**
- Integration tests REQUIRED for:
  - Protocol communication with game clients
  - Protocol communication with game servers
  - Chat server message flows
  - Database operations via MERRICK.DatabaseContext
- Tests MUST validate legacy parity with HoN/KONGOR behavior
- Performance testing REQUIRED for real-time components (Chat Server)
- Tests located in ASPIRE.Tests project
- Test failures MUST be addressed before implementation is considered complete

**Rationale:** Given the complex legacy protocols and real-time requirements, thorough testing is essential to ensure behavioral parity and system reliability.

## Legacy Code Treatment

**Purpose:** Define how to interact with and learn from legacy codebases.

**Rules:**
- Legacy code MUST be treated as reference material, not as code to be copied verbatim
- When analyzing legacy code:
  - Document the behavior and intent, not just the implementation
  - Identify potential issues or technical debt in legacy approach
  - Consider if modern .NET patterns provide better solutions
  - Note any hardcoded values, magic numbers, or assumptions
- Legacy protocol structures MUST be honored, but their implementation can be modernized
- When legacy code conflicts with modern best practices, justify the modernization approach
- Keep a mapping between legacy components and new NEXUS components for traceability

**Rationale:** The legacy codebases contain valuable protocol knowledge but may use outdated patterns. We modernize implementation while preserving protocol behavior.

## Development Standards

**Purpose:** Define workflow and quality expectations.

**Technology Stack:**
- **Language**: C# with .NET 9 (or version specified in solution)
- **Framework**: .NET Aspire for distributed application orchestration
- **Database**: Entity Framework Core via MERRICK.DatabaseContext
- **Testing**: xUnit in ASPIRE.Tests project
- **Platform**: Cross-platform (Windows/Linux) with Docker support
- **Tools**: PowerShell Core, EF Core CLI, Aspire CLI (as per README.md)

**Workflow Requirements:**
- Development via Aspire ApplicationHost launch profiles (Development/Production)
- Database migrations managed through `aspire exec --resource database-context`
- All services MUST be testable independently and as part of the distributed application
- Configuration MUST follow Aspire patterns for environment-specific settings
- Observability MUST be built-in (logging, health checks, metrics)

**Documentation Requirements:**
- Protocol discoveries MUST be documented as they are found
- Chat server event flows MUST be captured in design documents
- Legacy behavior differences MUST be noted when modernizing
- Public APIs MUST have XML documentation summaries

**Commit Standards:**
- Commits MUST be atomic and focused
- Breaking changes MUST be clearly marked and documented
- Protocol changes MUST reference legacy behavior being modified

## Governance

**Amendment Process:**
- Constitution amendments require documented rationale
- Changes MUST be versioned semantically:
  - MAJOR: Breaking changes to principles or governance
  - MINOR: New principles or materially expanded guidance
  - PATCH: Clarifications, wording improvements, non-semantic fixes
- All dependent templates MUST be updated when constitution changes
- Sync Impact Report MUST be maintained at top of document

**Compliance:**
- All PRs and code reviews MUST verify compliance with these principles
- Non-compliance MUST be justified and documented
- Constitution supersedes project-specific guidance files when conflicts arise
- Use `.specify/templates/agent-file-template.md` for runtime development guidance

**Version Control:**
- Current version tracked in this document
- Version history maintained through git commits
- Breaking changes require team review and approval

**Ratification Date vs Last Amended:**
- Ratification Date: The date this constitution was first adopted (today)
- Last Amended Date: The date of most recent substantive changes (today for initial creation)

**Version**: 1.2.0 | **Ratified**: 2025-01-13 | **Last Amended**: 2025-11-16
