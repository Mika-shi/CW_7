using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models;

public class LibraryUser
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Enter first name")]
    public string FirstName { get; set; } = "";

    [Required(ErrorMessage = "Enter last name")]
    public string LastName { get; set; } = "";

    [Required(ErrorMessage = "Enter email")]
    [EmailAddress(ErrorMessage = "Enter correct email")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Enter phone number")]
    public string PhoneNumber { get; set; } = "";

    public List<BookIssue> BookIssues { get; set; } = new List<BookIssue>();
}