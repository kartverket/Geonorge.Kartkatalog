# Geonorge.Kartkatalog .NET 10.0 Upgrade Tasks

## Overview

This document tracks the execution of the upgrade of both projects in the solution to .NET 10.0 using an atomic, all-at-once approach. All project files, package references, and breaking changes will be addressed in a single coordinated operation, followed by full solution testing and a final commit.

**Progress**: 4/4 tasks complete (100%) ![0%](https://progress-bar.xyz/100)

---

## Tasks

### [✓] TASK-001: Verify prerequisites *(Completed: 2026-02-23 09:32)*
**References**: Plan §Project-by-Project Plans, Plan §Prerequisites

- [✓] (1) Verify .NET 10.0 SDK is installed per Plan §Project-by-Project Plans
- [✓] (2) SDK version meets minimum requirements (**Verify**)
- [✓] (3) Check for presence and compatibility of global.json (if present)
- [✓] (4) global.json (if present) is compatible with .NET 10.0 (**Verify**)

---

### [✓] TASK-002: Atomic framework, package, and breaking changes upgrade for all projects *(Completed: 2026-02-23 09:53)*
**References**: Plan §Project-by-Project Plans, Plan §Detailed Dependency Analysis, Plan §Migration Steps, Plan §Breaking Changes Resolution

- [✓] (1) Convert both projects (`Kartverket.Metadatakatalog.csproj`, `Kartverket.Metadatakatalog.Tests.csproj`) to SDK-style project format per Plan §Project-by-Project Plans
- [✓] (2) Update TargetFramework in both projects from `net48` to `net10.0` per Plan §Project-by-Project Plans
- [✓] (3) Remove legacy configuration files (e.g., Web.config) and migrate to appsettings.json per Plan §Project-by-Project Plans
- [✓] (4) Update project structure for ASP.NET Core and modern .NET testing per Plan §Project-by-Project Plans
- [✓] (5) Update all package references across both projects per Plan §Package Reference Updates (remove incompatible, upgrade recommended, remove framework-included)
- [✓] (6) Restore all dependencies for both projects
- [✓] (7) All dependencies restored successfully (**Verify**)
- [✓] (8) Address all breaking changes and migrate APIs per Plan §Breaking Changes Resolution (ASP.NET Core migration, DI migration, EF Core migration, OWIN middleware conversion)
- [✓] (9) Build the solution and fix all compilation errors per Plan §Breaking Changes Resolution
- [✓] (10) Solution builds with 0 errors (**Verify**)
- [✓] (11) Solution builds with 0 warnings (**Verify**)

---

### [✓] TASK-003: Run full test suite and validate upgrade *(Completed: 2026-02-23 09:54)*
**References**: Plan §Testing & Validation Strategy, Plan §Project-by-Project Plans

- [✓] (1) Run tests in `Kartverket.Metadatakatalog.Tests.csproj` project
- [✓] (2) Fix any test failures (reference Plan §Breaking Changes for common issues)
- [✓] (3) Re-run tests after fixes
- [✓] (4) All tests pass with 0 failures (**Verify**)

---

### [✓] TASK-004: Final commit *(Completed: 2026-02-23 09:56)*
**References**: Plan §Source Control Strategy

- [✓] (1) Commit all changes with message: "feat: Upgrade solution to .NET 10.0 - All-At-Once migration"

---















