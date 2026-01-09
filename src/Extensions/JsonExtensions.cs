using System.Text.Json;

namespace IATec.Shared.EF.Repository.Extensions;

public static class JsonExtensions
{
    public static string? Serialize(this object? logContent)
    {
        return logContent == null ? null : JsonSerializer.Serialize(logContent);
    }
}