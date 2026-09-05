using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

public class BookIssueController : Controller
{
    private readonly LibraryDbContext _context;

    public BookIssueController(LibraryDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        List<BookIssue> issuedBooks = _context.BookIssues.Include(issue => issue.Book).Include(issue => issue.LibraryUser).Where(issue => issue.ReturnedAt == null).OrderByDescending(issue => issue.IssuedAt).ToList();

        return View(issuedBooks);
    }
}