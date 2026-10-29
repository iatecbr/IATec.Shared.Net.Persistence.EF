# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [IATec.Shared.Persistence.EF.AuditOutbox 1.0.0] - Unreleased

### Changed

- `AuditOutboxOptions.PollInterval` default changed from `00:00:05` to `00:00:10` (10 seconds).

### Fixed

- Entries stuck in `Processing` after a worker/host crash or restart are no longer orphaned. A new nullable `ProcessingSince` column records when an entry was claimed; the worker now also selects `Processing` entries whose claim is older than the configurable `ProcessingTimeout` and re-delivers them. Entries claimed more recently are assumed to be in flight on another active replica and are left untouched (no double-send).

### Added

- `AuditOutboxOptions.ProcessingTimeout` (`TimeSpan`, default `00:05:00`): maximum time an entry may remain `Processing` before it is considered abandoned and rescued by a later poll cycle.
- `AuditLogEntry.ProcessingSince` (`DateTime?`): UTC timestamp stamped when the worker claims an entry; cleared when a delivery fails (the entry returns to `Failed`).

## [2.0.0] - Unreleased

### Changed

- **BREAKING:** `GenericWriteRepository<T>` no longer takes an `ILogDispatcher` in its constructor. Its signature is now `GenericWriteRepository<T>(DbContext dbWriteContext)`. Derived repositories must drop the `ILogDispatcher` parameter from their base call.
- `GenericWriteRepository<T>.SaveChangesAsync` now only persists changes; it no longer captures entities from the change tracker nor dispatches audit logs.
- `GenericTransaction` no longer coordinates audit-log buffering/dispatch on commit/rollback; it now only wraps `BeginTransaction`/`CommitTransaction`/`RollbackTransaction`.

### Removed

- **BREAKING:** Removed the automatic `ILogDispatcher`-based audit dispatch from the core. Auditing is now provided exclusively by the optional `IATec.Shared.Persistence.EF.AuditOutbox` package, whose save-changes interceptor writes audit entries within the same transaction as the business operation.
- Removed internal `AuditLog` record and `AuditLogBuffer` class (no longer used).

## [1.3.0] - 2026-09-22

### Fixed

- Fixed audit logging in `GenericWriteRepository.SaveChangesAsync` where `GetOwner()` returned the default `"0"` for entities in the `Added` state. Owner resolution now happens **after** persistence so database-generated identity keys are already assigned. Log content is still captured before saving (required for `Deleted` entities), while the owner is resolved from the tracked entity reference once the save completes.

## [1.2.0] - 2026-05-21

### Added

- Added XML documentation (`<summary>`) to all public classes and methods across the library (`GenericWriteRepository`, `GenericReadRepository`, `GenericRepositoryQuery`, `GenericTransaction`, `QueryableExtension`, `PersistenceExtension`) to improve IntelliSense for library consumers.

### Changed

- Updated package dependencies:
  - `IATec.Shared.Domain` to `2.0.0`
  - `IATec.Shared.Domain.EF` to `1.3.0`
  - `Microsoft.EntityFrameworkCore` to `10.0.8`

## [1.1.0] - 2026-01-12

### Changed

- Upgraded target framework to .NET 10.0.
- Updated package dependencies:
  - `Microsoft.EntityFrameworkCore` to `10.0.8`
  - `IATec.Shared.Domain` to `1.2.0`
  - `IATec.Shared.Domain.EF` to `1.2.0`

## [1.0.0] - 2025-09-03

### Added

- Implemented query ordering support in generic repositories.
- Added `IWriteRepository<T>` interface segregation for write operations.
- Added `ILogDispatcher` integration to automatically dispatch audit logs on entity changes.

### Changed

- Refactored generic repositories (`GenericReadRepository`, `GenericWriteRepository`, `GenericRepositoryQuery`) for clearer separation of concerns and improved API definitions.
- Converted `GenericWriteRepository` from abstract class with interface inheritance to a more cohesive pattern.
- Improved project metadata, namespace organization and package configuration.
- Updated pipeline configurations (Azure DevOps) for better build and release management.

### Removed

- Removed internal `SaveChangesAsync` call inside the `SaveLogAsync` helper method in `GenericWriteRepository` to prevent unintended premature database commits during batch operations.

## [0.0.1-RC2] - 2025

### Added

- Initial project scaffolding for Entity Framework generic repository and transaction abstractions.
- `GenericTransaction` class for managing EF Core database transactions with automatic commit/rollback semantics.
- `GenericReadRepository<T>` and `GenericWriteRepository<T>` base implementations.
- `QueryableExtension` with pagination helpers (`ToPagedListAsync`).
- Azure Pipelines CI/CD configuration.

### Changed

- Improved namespaces to follow `IATec.Shared.EF.Repository` convention.
- Enhanced generic transaction error handling and disposal patterns.
- Updated pipeline authentication and external feed access configurations.
