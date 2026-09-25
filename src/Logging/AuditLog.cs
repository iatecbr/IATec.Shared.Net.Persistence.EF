namespace IATec.Shared.EF.Repository.Logging;

/// <summary>
/// Immutable snapshot of the information required to dispatch a single audit log entry.
/// </summary>
/// <remarks>
/// The snapshot is captured from the change tracker before persistence so it reflects the
/// entity state and content at save time, independent of any later state transitions.
/// </remarks>
/// <param name="Source">The source type value of the entity.</param>
/// <param name="Owner">The owner associated with the entity.</param>
/// <param name="Action">The action performed (added, modified, or deleted).</param>
/// <param name="Content">Optional content to log. Null for delete actions.</param>
public sealed record AuditLog(string Source, string Owner, string Action, object? Content);
