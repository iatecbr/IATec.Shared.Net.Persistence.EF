# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
