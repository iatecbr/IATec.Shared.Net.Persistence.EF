using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IATec.Shared.EF.Repository.Extensions;

public static class OutboxLogExtensions
{
    public static void AddOutboxLogEntity(this ModelBuilder modelBuilder, Action<EntityTypeBuilder<OutboxLog>>? callback = null)
    {
        var outboxLog = modelBuilder.Entity<OutboxLog>();

        outboxLog.ConfigureInboxStateEntity();

        callback?.Invoke(outboxLog);
    }
    
    private static void ConfigureInboxStateEntity(this EntityTypeBuilder<OutboxLog> inbox)
    {
        inbox.OptOutOfEntityFrameworkConventions();

        inbox.Property(p => p.Id);
        inbox.HasKey(p => p.Id);

        inbox.Property(p => p.Source).HasMaxLength(255);
        inbox.Property(p => p.Owner).HasMaxLength(255);
        inbox.Property(p => p.Action).HasMaxLength(255);
        inbox.Property(p => p.Content);
        
        inbox.Property(p => p.OccurredOnUtc);
        inbox.HasIndex(p => p.OccurredOnUtc);
        
        inbox.Property(p => p.ProcessedOnUtc);
        inbox.HasIndex(p => p.ProcessedOnUtc);
        
        inbox.Property(p => p.Error);
        
        inbox.Property(p => p.Status);
        inbox.HasIndex(p => p.Status);
    }

    private static void OptOutOfEntityFrameworkConventions(this EntityTypeBuilder builder)
    {
        foreach (var properties in builder.Metadata.GetProperties())
            properties.SetMaxLength(null);
    }
}