using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModel;

public class TakeBookViewModel
{
    public int BookId { get; set; }

    [Required(ErrorMessage = "Enter email")]
    [EmailAddress(ErrorMessage = "Enter correct email")]
    public string Email { get; set; } = "";
}