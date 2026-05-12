# .NET 10.0 Upgrade Plan

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Migration Strategy](#migration-strategy)
3. [Detailed Dependency Analysis](#detailed-dependency-analysis)
4. [Project-by-Project Plans](#project-by-project-plans)
5. [Risk Management](#risk-management)
6. [Testing & Validation Strategy](#testing--validation-strategy)
7. [Complexity & Effort Assessment](#complexity--effort-assessment)
8. [Source Control Strategy](#source-control-strategy)
9. [Success Criteria](#success-criteria)

---

## Executive Summary

### Selected Strategy
**All-At-Once Strategy** - All projects upgraded simultaneously in single operation.

**Rationale**: 
- 2 projects (small solution with simple dependency structure)
- Main project (foundation) + Test project (dependent)
- Clear linear dependency: Tests depend only on main project
- All packages have compatible target framework versions available
- No security vulnerabilities requiring separate remediation

### Discovered Metrics
- **Total Projects**: 2 (1 main application + 1 test project)
- **Total Issues**: 1,797 (1,283 mandatory, 503 potential, 11 optional)
- **Affected Files**: 63 across both projects
- **Dependency Depth**: 1 level (simple linear structure)
- **Risk Indicators**: High issue count but concentrated in ASP.NET Framework migration patterns
- **Security Vulnerabilities**: None detected

### Complexity Classification
**Medium Complexity** - Single major technology migration with predictable patterns

**Justification**: 
- Large number of issues (1,797) due to comprehensive ASP.NET Framework ? ASP.NET Core migration
- Issues are concentrated in well-documented migration patterns (System.Web, Autofac, Entity Framework, OWIN)
- No circular dependencies or complex architectural challenges
- All breaking changes have established migration paths

### Critical Issues Summary
1. **ASP.NET Framework ? Core Migration** (1,429 issues): Complete web stack transformation
2. **API Breaking Changes** (1,563 issues): Binary and source incompatibility requiring updates
3. **Package Incompatibilities** (84 issues): Package replacements and upgrades needed
4. **Project System Modernization** (2 issues): SDK-style project conversion

### Expected Remaining Iterations
Based on All-At-Once strategy for medium complexity solution: **5-6 iterations**
- Phase 2: Foundation (3 iterations) - Dependency analysis, migration strategy, project stubs
- Phase 3: Dynamic Detail Generation (2-3 iterations) - Detailed project migration plans grouped by complexity

## Migration Strategy

### Approach Selection and Justification

**Selected Strategy: All-At-Once Strategy**

**Why All-At-Once vs Incremental:**

? **All-At-Once Factors Present:**
- **Small Solution**: Only 2 projects 
- **Simple Dependencies**: Clear linear structure (Main ? Tests)
- **Homogeneous Technology Stack**: Both projects use consistent .NET Framework patterns
- **Compatible Packages**: Assessment confirms all packages have target framework versions available
- **No Security Blocking Issues**: Zero vulnerabilities requiring separate remediation phase

? **Incremental Not Needed:**
- No large solution complexity (< 30 projects threshold)
- No complex dependency web or circular references
- No mixed framework versions requiring staged approach
- No mission-critical uptime requirements

### All-At-Once Strategy Implementation

**Core Principle**: Maximum consolidation into single atomic upgrade operation

**Atomic Operations Batch:**
1. **Project System Modernization** - Convert both projects to SDK-style simultaneously
2. **Framework Target Update** - Update TargetFramework to net10.0 in both projects
3. **Package Reference Updates** - Update all package references across both projects  
4. **ASP.NET Core Migration** - Transform web application stack in single coordinated operation
5. **Dependency System Updates** - Convert Autofac to built-in DI, OWIN to ASP.NET Core middleware
6. **Build & Compilation** - Fix all breaking changes and build solution to 0 errors
7. **Test Validation** - Execute upgraded test suite to verify functionality

**Dependency-Based Execution Order:**
While all updates happen in same operation, internal sequencing respects dependencies:
1. Foundation project updates (Kartverket.Metadatakatalog) 
2. Dependent project updates (Tests) - can reference updated foundation
3. Solution-wide validation and testing

### Parallel vs Sequential Execution Decision

**Sequential Within Atomic Operation**: Due to dependency relationship, updates follow this sequence:
- Main application project file and package updates ? Tests project updates
- All compilation fixes applied together
- Final validation confirms both projects work together

### Risk Management Alignment

All-At-Once strategy aligns with risk profile:
- **High Issue Count** (1,797) but **concentrated patterns** make batch processing efficient
- **Well-Documented Migration Paths** for all major features (ASP.NET Core, Entity Framework, dependency injection)
- **Single Integration Point** eliminates multi-version compatibility issues
- **Faster Overall Completion** reduces exposure window to hybrid states

## Detailed Dependency Analysis

### Dependency Graph Summary
The solution has a simple **linear dependency structure** with no circular dependencies or complex relationships:

```
Level 0 (Foundation - No Dependencies):
??? Kartverket.Metadatakatalog.csproj [Main Application]
    ? • Issues: 1,727 (1,228 mandatory)
    ? • Type: Web Application Project (WAP)
    ? • Current: .NET Framework 4.8
    ? • Target: .NET 10.0

Level 1 (Dependent Projects):
??? Kartverket.Metadatakatalog.Tests.csproj [Test Project]
    ? • Issues: 70 (55 mandatory)  
    ? • Type: Classic Class Library
    ? • Dependencies: Kartverket.Metadatakatalog
    ? • Current: .NET Framework 4.8
    ? • Target: .NET 10.0
```

### Project Groupings by Migration Phase
Following All-At-Once Strategy, both projects will be upgraded simultaneously, but in dependency order:

**Single Atomic Phase**: All Projects Simultaneously
- **Primary**: Kartverket.Metadatakatalog.csproj (foundation)
- **Secondary**: Kartverket.Metadatakatalog.Tests.csproj (depends on primary)

### Critical Path Identification
**Critical Path**: Main Application ? Tests
- The main application must be fully functional before test execution
- Tests validate the upgraded application functionality
- No parallel execution possible due to dependency relationship

### Circular Dependency Analysis
? **No circular dependencies detected** - Clean linear dependency structure enables straightforward migration approach.

## Project-by-Project Plans

### Project: Kartverket.Metadatakatalog.csproj
**Current State**: .NET Framework 4.8, WAP (Web Application Project), 438 files, 1,727 issues (1,228 mandatory)

**Target State**: .NET 10.0, ASP.NET Core Web Application

**Migration Steps**:

#### 1. Prerequisites
- Ensure .NET 10.0 SDK installed
- Verify all dependencies ready for upgrade

#### 2. Project System Modernization
- Convert from legacy WAP format to SDK-style project
- Update TargetFramework from `net48` to `net10.0`
- Remove Web.config (migrate to appsettings.json)
- Update project structure for ASP.NET Core

#### 3. Package Reference Updates
**Remove Incompatible Packages** (24 packages):
- `Autofac.Mvc5`, `Autofac.Mvc5.Owin`, `Autofac.Owin`, `Autofac.WebApi2` ? Replace with ASP.NET Core DI
- `Microsoft.AspNet.Web.Optimization` ? Replace with built-in bundling/minification
- `Microsoft.Owin.*` packages ? Replace with ASP.NET Core middleware
- `Geonorge.AuthLib.NetFull` ? Update to .NET compatible version

**Upgrade Recommended Packages** (40 packages):
- `EntityFramework` 6.3.0 ? 6.5.1 (then migrate to EF Core)
- Microsoft Extensions packages: Update all to 10.0.3
- `Microsoft.AspNetCore.Authentication.OpenIdConnect` 2.3.0 ? 10.0.3

**Remove Framework-Included Packages** (70+ packages):
- Remove packages now included in .NET 10.0 framework reference
- ASP.NET packages, System.* packages, etc.

#### 4. ASP.NET Framework ? ASP.NET Core Migration
- **Global.asax.cs** ? Convert to Program.cs/Startup.cs
- **RouteCollection** ? ASP.NET Core routing
- **GlobalFilterCollection** ? ASP.NET Core middleware
- **System.Web.Optimization** ? Built-in bundling or manual HTML references
- **Web.config** ? appsettings.json + middleware configuration

#### 5. Dependency Injection Migration
- **Autofac IoC** ? ASP.NET Core built-in DI
- Update all controller constructors
- Register services in Program.cs
- Convert Autofac modules to service registration

#### 6. Entity Framework Migration  
- **Entity Framework 6** ? **Entity Framework Core**
- Update connection strings and configuration
- Migrate DbContext initialization
- Update LINQ queries for EF Core differences

#### 7. OWIN ? ASP.NET Core Middleware
- Convert OWIN pipeline to ASP.NET Core middleware
- Update authentication/authorization middleware
- Migrate custom OWIN components

#### 8. Breaking Changes Resolution
- Address 1,563 API compatibility issues systematically
- Update System.Web references to ASP.NET Core equivalents
- Fix obsolete API usage across 438 files
- Update configuration access patterns

#### 9. Validation
- ? Project builds without errors  
- ? Project builds without warnings
- ? All dependencies resolve correctly
- ? Web application starts successfully
- ? Core functionality verified

---

### Project: Kartverket.Metadatakatalog.Tests.csproj  
**Current State**: .NET Framework 4.8, Classic Class Library, 11 files, 70 issues (55 mandatory)

**Target State**: .NET 10.0, Modern Test Project

**Migration Steps**:

#### 1. Prerequisites  
- Main application project must be successfully migrated first
- Test dependencies on updated application APIs

#### 2. Project System Modernization
- Convert from Classic Class Library to SDK-style project
- Update TargetFramework from `net48` to `net10.0`
- Update project structure for modern .NET testing

#### 3. Package Reference Updates
**Remove Framework-Included Packages** (44 packages):
- Remove System.* packages included in .NET 10.0
- Remove Microsoft.NETCore.Platforms, NETStandard.Library

**Upgrade Recommended Packages** (12 packages):
- Microsoft Extensions packages: Update to 10.0.3  
- `System.Text.Json` 4.7.2 ? 10.0.3
- `System.Text.Encodings.Web` 6.0.0 ? 10.0.3

**Remove Incompatible Packages** (4 packages):
- Update or remove any ASP.NET Framework specific test dependencies

#### 4. Test Framework Compatibility
- Verify xUnit packages (2.4.1) compatibility with .NET 10.0
- Update test runner and analyzers if needed
- Ensure FluentAssertions (5.9.0) works with new framework

#### 5. Test Code Updates
- Update test references to new application APIs
- Fix any System.Web dependencies in tests
- Update mocking framework usage (Moq 4.13.0)

#### 6. Validation
- ? Project builds without errors
- ? Project builds without warnings  
- ? All test dependencies resolve
- ? Tests can be discovered and executed
- ? All tests pass

---

## Testing & Validation Strategy

### Multi-Level Testing Approach

#### 1. Project-Level Testing
**After Each Project Migration:**
- **Build Validation**: No compilation errors or warnings
- **Dependency Resolution**: All package references resolve correctly  
- **Basic Functionality**: Core services instantiate properly
- **Reference Validation**: Project-to-project references work correctly

#### 2. Phase Testing (All-At-Once Validation)
**After Atomic Upgrade Completion:**
- **Integration Testing**: Main application + Tests work together
- **End-to-End Scenarios**: Critical user workflows function correctly
- **Performance Baseline**: No significant performance regression
- **Configuration Validation**: All configuration sources load correctly

#### 3. Full Solution Testing
**Final Validation Checkpoint:**
- **Complete Build**: Entire solution builds successfully
- **Test Suite Execution**: All automated tests pass
- **Manual Validation**: Key application features verified manually
- **Production Readiness**: Application ready for deployment

### Testing Execution Sequence

1. **Main Application Migration** ? Build + Basic functionality validation
2. **Test Project Migration** ? Build + Test discovery validation  
3. **Atomic Operation Complete** ? Full test suite execution
4. **Integration Validation** ? End-to-end scenario testing
5. **Final Sign-off** ? Production readiness confirmation

### Rollback Strategy

**If Critical Issues Emerge:**
- Git branch isolation allows clean rollback to pre-upgrade state
- Documented breaking changes enable targeted fixes
- Incremental commit strategy provides multiple rollback points

---

## Source Control Strategy

### Branching Strategy
**Branch Structure:**
- **Source Branch**: `beta` (starting point)
- **Upgrade Branch**: `upgrade-to-NET10` (all migration work)
- **Target Branch**: `main/master` (final merge destination)

### Commit Strategy 
**All-At-Once Strategy Commit Approach:**

**Single Comprehensive Commit** (Preferred):
- Atomic commit containing all project file updates, package changes, and compilation fixes
- Commit message: "feat: Upgrade solution to .NET 10.0 - All-At-Once migration"
- Benefits: Clean history, atomic rollback capability, clear migration boundary

**Alternative: Logical Checkpoint Commits:**
- Commit 1: "feat: Convert projects to SDK-style and update target frameworks"
- Commit 2: "feat: Update all package references for .NET 10.0 compatibility"  
- Commit 3: "fix: Resolve compilation errors and breaking changes"
- Commit 4: "test: Validate upgraded solution and test execution"

### Review and Merge Process

**Pre-Merge Checklist:**
- ? All automated tests pass
- ? Solution builds without errors/warnings
- ? All package vulnerabilities resolved
- ? Performance benchmarks acceptable
- ? Manual validation completed

**Merge Requirements:**
- Pull Request with detailed change summary
- Code review focusing on breaking changes resolution
- Final validation in staging environment
- Approved merge to main branch

---

## Success Criteria

### Technical Criteria  
- ? **Target Framework**: Both projects target .NET 10.0
- ? **Build Success**: Solution builds without errors or warnings
- ? **Package Compatibility**: All 186 packages updated/removed as required
- ? **Zero Vulnerabilities**: No security vulnerabilities in dependencies
- ? **Test Execution**: All tests discoverable and executable
- ? **Test Results**: All tests pass or failures explained/accepted

### Functional Criteria
- ? **Web Application Starts**: ASP.NET Core application launches successfully  
- ? **Core Features Work**: Primary metadata service operations function
- ? **Authentication Works**: OIDC authentication integration functional
- ? **Database Access**: Entity Framework Core data access operational
- ? **API Endpoints**: All Web API endpoints respond correctly

### Quality Criteria
- ? **Performance**: No significant performance regression (< 10% degradation)
- ? **Memory Usage**: Memory footprint within acceptable limits
- ? **Code Quality**: No new code analysis warnings introduced
- ? **Documentation**: Migration changes documented appropriately

### Process Criteria  
- ? **Strategy Followed**: All-At-Once approach executed as planned
- ? **All Issues Addressed**: All 1,797 assessment issues resolved
- ? **Source Control**: Proper branching and commit strategy followed
- ? **Validation Complete**: All testing phases completed successfully

### Migration Completeness
- ? **ASP.NET Core**: Complete migration from ASP.NET Framework
- ? **Dependency Injection**: Autofac successfully replaced with built-in DI
- ? **Entity Framework**: EF6 migrated to EF Core
- ? **OWIN Middleware**: Converted to ASP.NET Core middleware
- ? **Configuration**: Web.config migrated to appsettings.json
- ? **Package Modernization**: All packages compatible with .NET 10.0

**Migration Success Definition**: Solution runs successfully on .NET 10.0 with equivalent functionality to pre-upgrade state, all technical debt addressed, and modern .NET patterns implemented throughout.

---

## Risk Management

### High-Risk Changes
| Project | Risk Level | Description | Mitigation |
|---------|------------|-------------|------------|
| Kartverket.Metadatakatalog | **High** | Complete ASP.NET Framework ? Core migration (1,727 issues) | Comprehensive testing, incremental compilation fixes, established migration patterns |
| Kartverket.Metadatakatalog.Tests | **Medium** | Test framework compatibility, package updates (70 issues) | Validate after main project migration, separate test execution validation |

### Security Vulnerabilities
? **No security vulnerabilities detected** in current NuGet packages

### Contingency Plans
- **Build Failures**: Comprehensive breaking changes catalog with established solutions
- **Package Incompatibilities**: All packages have known compatible versions or replacements
- **Performance Regression**: Benchmark critical paths before/after migration
- **Functionality Regression**: Extensive test coverage validation at each checkpoint

---

## Complexity & Effort Assessment

### Per-Project Complexity Assessment
| Project | Complexity Rating | Dependencies | Risk Level | Key Challenges |
|---------|------------------|---------------|------------|----------------|
| **Kartverket.Metadatakatalog** | **High** | Foundation (0 project dependencies) | High | ASP.NET Core migration, DI conversion, middleware updates |
| **Kartverket.Metadatakatalog.Tests** | **Low** | Simple (1 project dependency) | Medium | Test framework compatibility, reference updates |

### All-At-Once Strategy Complexity Factors

**High Issue Concentration** (1,797 total):
- **ASP.NET Framework Migration**: 1,429 issues - well-documented patterns
- **API Breaking Changes**: 1,563 issues - systematic replacements available  
- **Package Updates**: 84 issues - compatible versions identified

**Simplifying Factors**:
- **Linear Dependencies**: No circular references or complex webs
- **Established Patterns**: All major migrations (Autofac?DI, OWIN?ASP.NET Core, EF6?EF Core) have proven approaches
- **Modern Tooling**: SDK-style projects and .NET 10.0 provide excellent compatibility

### Resource Requirements
- **Skill Level Required**: Experienced with ASP.NET Core migrations
- **Parallel Work Capacity**: Sequential due to dependency relationship
- **Knowledge Areas**: ASP.NET Core, Dependency Injection, Entity Framework Core, modern .NET patterns

## Source Control Strategy
[To be filled]

## Success Criteria
[To be filled]

---

*Generated by .NET Upgrade Assistant*