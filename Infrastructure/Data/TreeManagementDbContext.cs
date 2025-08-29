using Microsoft.EntityFrameworkCore;
using TreeManagementApi.Domain.Entities;

namespace TreeManagementApi.Infrastructure.Data;

/// <summary>
/// Database context for the Tree Management API.
/// Configures PostgreSQL database with proper entity mappings and constraints.
/// </summary>
public class TreeManagementDbContext : DbContext
{
    public TreeManagementDbContext(DbContextOptions<TreeManagementDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Tree nodes table
    /// </summary>
    public DbSet<TreeNode> TreeNodes { get; set; }

    /// <summary>
    /// Exception journal table for logging all exceptions
    /// </summary>
    public DbSet<ExceptionJournal> ExceptionJournals { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureTreeNode(modelBuilder);
        ConfigureExceptionJournal(modelBuilder);
    }

    private static void ConfigureTreeNode(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<TreeNode>();

        // Table name
        entity.ToTable("TreeNodes");

        // Primary key
        entity.HasKey(e => e.Id);

        // Auto-increment primary key
        entity.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .UseIdentityColumn();

        // Required fields
        entity.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(255);

        entity.Property(e => e.TreeId)
            .IsRequired();

        entity.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()"); // PostgreSQL function for current timestamp

        // Optional fields
        entity.Property(e => e.ParentId)
            .IsRequired(false);

        entity.Property(e => e.Path)
            .HasMaxLength(4000)
            .IsRequired(false);

        // Self-referencing foreign key to parent
        entity.HasOne(e => e.Parent)
            .WithMany(e => e.Children)
            .HasForeignKey(e => e.ParentId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent cascading deletes

        // Indexes for performance
        entity.HasIndex(e => new { e.TreeId, e.Id })
            .HasDatabaseName("IX_TreeNodes_TreeId_Id")
            .IsUnique();

        entity.HasIndex(e => e.TreeId)
            .HasDatabaseName("IX_TreeNodes_TreeId");

        entity.HasIndex(e => e.ParentId)
            .HasDatabaseName("IX_TreeNodes_ParentId");

        entity.HasIndex(e => e.Path)
            .HasDatabaseName("IX_TreeNodes_Path");

        // Custom constraints will be added via raw SQL in migrations
        // to ensure ParentId references a node with the same TreeId
    }

    private static void ConfigureExceptionJournal(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<ExceptionJournal>();

        // Table name
        entity.ToTable("ExceptionJournals");

        // Primary key
        entity.HasKey(e => e.Id);

        // Auto-increment primary key
        entity.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .UseIdentityColumn();

        // Required fields
        entity.Property(e => e.Timestamp)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        entity.Property(e => e.StackTrace)
            .IsRequired();

        entity.Property(e => e.ExceptionType)
            .IsRequired()
            .HasMaxLength(500);

        entity.Property(e => e.ExceptionMessage)
            .IsRequired();

        // Optional fields with size limits
        entity.Property(e => e.QueryParameters)
            .IsRequired(false);

        entity.Property(e => e.BodyParameters)
            .IsRequired(false);

        entity.Property(e => e.HttpMethod)
            .HasMaxLength(10)
            .IsRequired(false);

        entity.Property(e => e.RequestPath)
            .HasMaxLength(2000)
            .IsRequired(false);

        entity.Property(e => e.UserAgent)
            .HasMaxLength(500)
            .IsRequired(false);

        entity.Property(e => e.ClientIpAddress)
            .HasMaxLength(45)
            .IsRequired(false);

        // Indexes for querying
        entity.HasIndex(e => e.Timestamp)
            .HasDatabaseName("IX_ExceptionJournals_Timestamp");

        entity.HasIndex(e => e.ExceptionType)
            .HasDatabaseName("IX_ExceptionJournals_ExceptionType");
    }

    /// <summary>
    /// Save changes with automatic exception logging
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // Log the exception to the ExceptionJournals table if it's not already an exception logging operation
            if (!IsExceptionLoggingOperation())
            {
                var exceptionLog = ExceptionJournal.Create(ex);
                ExceptionJournals.Add(exceptionLog);
                
                // Try to save the exception log
                try
                {
                    await base.SaveChangesAsync(cancellationToken);
                }
                catch
                {
                    // If we can't save the exception log, we can't do much more
                }
            }
            
            throw;
        }
    }

    /// <summary>
    /// Check if the current operation is related to exception logging to prevent infinite loops
    /// </summary>
    private bool IsExceptionLoggingOperation()
    {
        return ChangeTracker.Entries<ExceptionJournal>()
            .Any(e => e.State == EntityState.Added || e.State == EntityState.Modified);
    }
}