# IATec.Shared.EF.Repository

Shared Entity Framework repository abstractions and implementations for .NET projects at IATec.

This library provides generic repository patterns, transaction management, and query extensions that help teams standardize data access layers across microservices and applications.

---

## Features

- **Generic Repositories**
  - `GenericReadRepository<T>` - Basic read operations (`GetAllAsync`, `GetByIdAsync`)
  - `GenericWriteRepository<T>` - Write operations with automatic audit logging (`AddAsync`, `UpdateAsync`, `RemoveAsync`, `AddRangeAsync`, `UpdateRangeAsync`, `RemoveRangeAsync`)
  - `GenericRepositoryQuery` - Flexible query construction with eager-loading support (`Include`)

- **Transaction Management**
  - `GenericTransaction` - Wrapper for EF Core database transactions (`BeginTransaction`, `CommitTransaction`, `RollbackTransaction`)

- **Query Extensions**
  - `QueryableExtension.Ordering<T>` - Dynamic ordering by property name with support for ascending/descending and secondary ordering

- **Audit Logging**
  - Automatic dispatch of `ILogDispatcher` events on entity changes (Add, Update, Remove)

---

## Installation

Add the package reference to your `.csproj`:

```xml
<PackageReference Include="IATec.Shared.EF.Repository" Version="1.2.0" />
```

---

## Dependencies

| Package | Version |
|---------|---------|
| `Microsoft.EntityFrameworkCore` | `10.0.8` |
| `IATec.Shared.Domain` | `2.0.0` |
| `IATec.Shared.Domain.EF` | `1.3.0` |

---

## Quick Start

### 1. Register Services

In your `Program.cs` or startup configuration:

```csharp
using IATec.Shared.EF.Repository.Configurations;

// Register GenericRepositoryQuery scoped to your DbContext
builder.Services.AddAdditionalPersistenceData<ApplicationDbContext>();

// Register your custom repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductTransaction, ProductTransaction>();
```

### 2. Create Custom Repositories

```csharp
using IATec.Shared.EF.Repository.Repositories;
using IATec.Shared.Domain.Contracts.Dispatcher;

public class ProductRepository(ApplicationDbContext context, ILogDispatcher logDispatcher)
    : GenericWriteRepository<Product>(context, logDispatcher), IProductRepository
{
    // Add custom query methods here
}
```

### 3. Use in Services / Handlers

```csharp
public class ProductService(IProductRepository repository)
{
    public async Task CreateAsync(Product product)
    {
        await repository.AddAsync(product);
        await repository.SaveChangesAsync();
    }

    public async Task CreateBatchAsync(IEnumerable<Product> products)
    {
        await repository.AddRangeAsync(products);
        await repository.SaveChangesAsync();
    }
}
```

---

## Transaction Usage

```csharp
public class ProductTransaction(ApplicationDbContext context)
    : GenericTransaction(context), IProductTransaction
{
}

// Usage
public async Task TransferAsync(int fromId, int toId, decimal amount)
{
    try
    {
        transaction.BeginTransaction();
        // ... perform operations ...
        await repository.SaveChangesAsync();
        transaction.CommitTransaction();
    }
    catch
    {
        transaction.RollbackTransaction();
        throw;
    }
}
```

---

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for version history.

---

## License

Copyright © IATec Solutions. All rights reserved.
