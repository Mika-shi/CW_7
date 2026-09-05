using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Models;

public class LibraryDbContext : DbContext
{
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options)
    {
    }

    public DbSet<Book> Books { get; set; }
    
    public DbSet<LibraryUser> LibraryUsers { get; set; }

    public DbSet<BookIssue> BookIssues { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<LibraryUser>().HasIndex(user => user.Email).IsUnique();
    }
}