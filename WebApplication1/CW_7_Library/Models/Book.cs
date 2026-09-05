using System.ComponentModel.DataAnnotations;
using WebApplication1.Models;

namespace WebApplication1.Models;

public class Book
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Введите название книги")]
    public string Title { get; set; } = "";

    [Required(ErrorMessage = "Введите автора книги")]
    public string Author { get; set; } = "";

    [Required(ErrorMessage = "Добавьте фото обложки")]
    public string CoverImageUrl { get; set; } = "";

    public int? ReleaseYear { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public bool IsIssued { get; set; } = false;
    
    public int? CategoryId { get; set; }

    public Category? Category { get; set; }
    
    public List<BookIssue> BookIssues { get; set; } = new List<BookIssue>();
}