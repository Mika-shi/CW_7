using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models;

public class Category
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Enter category name")]
    public string Name { get; set; } = "";

    public List<Book> Books { get; set; } = new List<Book>();
}