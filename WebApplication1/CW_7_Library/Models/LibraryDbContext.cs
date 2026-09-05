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
    
    public DbSet<Category> Categories { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<LibraryUser>().HasIndex(user => user.Email).IsUnique();

        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Classic literature" },
            new Category { Id = 2, Name = "Fantasy" },
            new Category { Id = 3, Name = "Science fiction" },
            new Category { Id = 4, Name = "Detective" },
            new Category { Id = 5, Name = "Non-fiction" });
    }
    
   
    
}