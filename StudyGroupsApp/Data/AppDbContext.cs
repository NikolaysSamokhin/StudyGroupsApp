using Microsoft.EntityFrameworkCore;
using StudyGroupsApp.Models;

namespace StudyGroupsApp.Data;

/// <summary>
/// Application database context for managing StudyGroups and Users.
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppDbContext"/> class.
    /// </summary>
    /// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the study groups in the database.
    /// </summary>
    public DbSet<StudyGroup> StudyGroups { get; set; }

    /// <summary>
    /// Gets or sets the users in the database.
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// Configures the entity mappings and relationships.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configuring StudyGroup entity
        modelBuilder.Entity<StudyGroup>(entity =>
        {
            entity.HasKey(sg => sg.StudyGroupId);

            entity.Property(sg => sg.Name)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(sg => sg.Subject)
                .IsRequired();

            entity.Property(sg => sg.CreateDate)
                .HasDefaultValueSql("GETUTCDATE()");

            // Many-to-many: StudyGroup <--> User
            entity.HasMany(sg => sg.Users)
                .WithMany()
                .UsingEntity(j => j.ToTable("StudyGroupUsers"));
        });

        // Configuring User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);

            entity.Property(u => u.Name)
                .IsRequired()
                .HasMaxLength(50);
        });

        base.OnModelCreating(modelBuilder);
    }
}