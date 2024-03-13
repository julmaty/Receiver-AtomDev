
using API2;
using Microsoft.EntityFrameworkCore;

public class ApplicationContext : DbContext
{
    public ApplicationContext()
    {

    }
    public DbSet<PeriodTableModel> Periods { get; set; } = null!;
    public DbSet<Report> Reports { get; set; } = null!;
    public DbSet<ReportTextContent> ReportTextContents { get; set; } = null!;
    public DbSet<ReportDescription> ReportDescription { get; set; } = default!;
    public ApplicationContext(DbContextOptions<ApplicationContext> options)
        : base(options)
    {
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //=> optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=applicationdbearth;");
    => optionsBuilder.UseMySql("Server=localhost;Database=API;User=apiadmin;Password=12345atomdev;", new MySqlServerVersion(new Version(8, 0, 36)));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        modelBuilder.Entity<ReportDescription>().HasData(
                    new ReportDescription { Id = 1, Description = "Здоровье" },
                    new ReportDescription { Id = 2, Description = "Климат, параметры атмосферы" },
                    new ReportDescription { Id = 3, Description = "Исследования, научная база" },
                    new ReportDescription { Id = 4, Description = "Ресурсы" }
            );
    }
    }

