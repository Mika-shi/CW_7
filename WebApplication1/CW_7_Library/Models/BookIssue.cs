using WebApplication1.Models;

namespace WebApplication1.Models;

public class BookIssue
{
    public int Id { get; set; }

    public int LibraryUserId { get; set; }

    public LibraryUser? LibraryUser { get; set; }

    public int BookId { get; set; }

    public Book? Book { get; set; }

    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReturnedAt { get; set; }
}