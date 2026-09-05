using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

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
        List<LibraryUser> users = _context.LibraryUsers.OrderBy(user => user.LastName).ThenBy(user => user.FirstName).ToList();

        return View(users);
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
}