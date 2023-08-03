using Microsoft.EntityFrameworkCore;
using ReunionServer.Models;

namespace ReunionServer;

public partial class DataContext : DbContext
{
    public DataContext()
    {
    }

    public DataContext(DbContextOptions<DataContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CampaignSelectorMark> CampaignSelectorMarks { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_general_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<CampaignSelectorMark>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("CampaignSelectorMark")
                .UseCollation("utf8mb4_0900_ai_ci");

            entity.Property(e => e.Id)
                .HasColumnType("int(11)")
                .HasColumnName("id");
            entity.Property(e => e.Bad)
                .HasMaxLength(255)
                .HasColumnName("bad");
            entity.Property(e => e.Good)
                .HasMaxLength(255)
                .HasColumnName("good");
            entity.Property(e => e.Usname)
                .HasMaxLength(255)
                .HasColumnName("USName");
        });
    }
}