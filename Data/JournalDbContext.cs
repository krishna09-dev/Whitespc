using Microsoft.EntityFrameworkCore;
using whitespc.Models;

namespace whitespc.Data;

public class JournalDbContext : DbContext
{
    public DbSet<JournalEntry> JournalEntries { get; set; } = null!;
    public DbSet<Tag> Tags { get; set; } = null!;
    public DbSet<UserSettings> UserSettings { get; set; } = null!;

    public JournalDbContext(DbContextOptions<JournalDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure JournalEntry
        modelBuilder.Entity<JournalEntry>(entity =>
        {
            entity.HasIndex(e => e.EntryDate).IsUnique(); // ✅ One entry per day

            entity.HasMany(e => e.Tags)
                .WithMany(t => t.JournalEntries)
                .UsingEntity(j => j.ToTable("JournalEntryTags"));
        });

        // Configure Tag
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Seed prebuilt tags
        var prebuiltTags = Tag.PrebuiltTags
            .Select((name, index) => new Tag
            {
                Id = index + 1,
                Name = name,
                IsPrebuilt = true
            })
            .ToList();

        modelBuilder.Entity<Tag>().HasData(prebuiltTags);

        // Seed default user settings
        modelBuilder.Entity<UserSettings>().HasData(new UserSettings
        {
            Id = 1,
            IsDarkMode = false,
            IsLocked = false
        });
    }
}