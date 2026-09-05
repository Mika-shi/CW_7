using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using WebApplication1.ViewModel;

namespace WebApplication1.Controllers;

public class LibraryUserController : Controller
{
    private readonly LibraryDbContext _context;

    public LibraryUserController(LibraryDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(LibraryUser user)
    {
        if (ModelState.IsValid)
        {
            bool emailExists = _context.LibraryUsers.Any(existingUser => existingUser.Email == user.Email);

            if (emailExists)
            {
                ModelState.AddModelError("Email", "User with this email already exists.");
                return View(user);
            }

            _context.LibraryUsers.Add(user);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        return View(user);
    }

    public IActionResult Details(int id)
    {
        LibraryUser? user = _context.LibraryUsers.Include(user => user.BookIssues).ThenInclude(issue => issue.Book).FirstOrDefault(user => user.Id == id);

        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ReturnBook(int issueId, string? email)
    {
        BookIssue? issue = _context.BookIssues
            .Include(issue => issue.Book)
            .FirstOrDefault(issue => issue.Id == issueId && issue.ReturnedAt == null);

        if (issue == null)
        {
            return NotFound();
        }

        issue.ReturnedAt = DateTime.UtcNow;

        if (issue.Book != null)
        {
            issue.Book.IsIssued = false;
        }

        _context.SaveChanges();
        
        if (!string.IsNullOrWhiteSpace(email))
        {
            return RedirectToAction("Cabinet", new { email = email });
        }

        return RedirectToAction("Details", new { id = issue.LibraryUserId });
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult FindByEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            TempData["CabinetError"] = "Enter email.";
            return RedirectToAction("Index");
        }

        string userEmail = email.Trim().ToLower();

        LibraryUser? user = _context.LibraryUsers
            .FirstOrDefault(user => user.Email.ToLower() == userEmail);

        if (user == null)
        {
            TempData["CabinetError"] = "User with this email was not found.";
            return RedirectToAction("Index");
        }

        return RedirectToAction("Details", new { id = user.Id });
    }
}