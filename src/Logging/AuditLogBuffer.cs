using System.Runtime.CompilerServices;
using IATec.Shared.Domain.Contracts.Dispatcher;
using Microsoft.EntityFrameworkCore;

namespace IATec.Shared.EF.Repository.Logging;

/// <summary>
/// Accumulates audit logs captured during write operations and controls when they are dispatched,
/// coordinating persistence and transaction boundaries.
/// </summary>
/// <remarks>
/// The buffer is associated with a specific <see cref="DbContext"/> instance rather than being
/// injected, so derived repositories and transactions do not need to change their constructors.
/// Because a <see cref="DbContext"/> is registered per scope and is single-threaded, one buffer
/// per context instance is sufficient and safe.
/// </remarks>
public sealed class AuditLogBuffer
{
    private static readonly ConditionalWeakTable<DbContext, AuditLogBuffer> Buffers = new();

    private readonly List<AuditLog> _pending = [];

    private AuditLogBuffer()
    {
    }

    /// <summary>
    /// Gets the buffer associated with the given <see cref="DbContext"/>, creating it on first access.
    /// The entry is released automatically when the context is garbage collected.
    /// </summary>
    /// <param name="context">The context whose buffer is requested.</param>
    /// <returns>The audit log buffer bound to the context.</returns>
    public static AuditLogBuffer For(DbContext context)
    {
        return Buffers.GetValue(context, _ => new AuditLogBuffer());
    }

    /// <summary>
    /// Indicates whether an ambient transaction is currently active. While active, logs are held
    /// until <see cref="FlushAsync"/> is called (on commit) or <see cref="Discard"/> is called (on rollback).
    /// </summary>
    public bool IsTransactionActive { get; private set; }

    /// <summary>
    /// Marks the beginning of a transactional scope, causing subsequent logs to be buffered
    /// instead of dispatched immediately.
    /// </summary>
    public void EnlistTransaction()
    {
        IsTransactionActive = true;
    }

    /// <summary>
    /// Adds captured logs to the buffer.
    /// </summary>
    /// <param name="logs">The logs to enqueue.</param>
    public void Add(IEnumerable<AuditLog> logs)
    {
        _pending.AddRange(logs);
    }

    /// <summary>
    /// Dispatches all buffered logs through the provided dispatcher and clears the buffer.
    /// </summary>
    /// <param name="dispatcher">The dispatcher used to send the logs.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    public async Task FlushAsync(ILogDispatcher dispatcher, CancellationToken cancellationToken = default)
    {
        // Copy and clear first so a failure mid-dispatch does not leave logs to be dispatched twice.
        var toDispatch = _pending.ToArray();
        _pending.Clear();
        IsTransactionActive = false;

        foreach (var log in toDispatch)
        {
            await dispatcher.DispatchAsync(
                source: log.Source,
                owner: log.Owner,
                action: log.Action,
                content: log.Content,
                cancellationToken: cancellationToken);
        }
    }

    /// <summary>
    /// Clears all buffered logs without dispatching them and ends the transactional scope.
    /// </summary>
    public void Discard()
    {
        _pending.Clear();
        IsTransactionActive = false;
    }
}
