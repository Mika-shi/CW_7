using WebApplication1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.ViewModel;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApplication1.Controllers;

public class BookController : Controller
{
    private readonly LibraryDbContext _context;
    public BookController(LibraryDbContext context)
    {
        _context = context;
    }

    public IActionResult Index(int  page = 1)
    {
        int pageSize = 2;
        
        IQueryable<Book> books = _context.Books.OrderByDescending(book => book.CreatedOn);
        
        int count = books.Count();

        if (page < 1)
        {
            page = 1;
        }
        int totalPages = (int)Math.Ceiling(count / (double)pageSize);

        if (totalPages > 0 && page > totalPages)
        {
            page =  totalPages;
        }
        
        List<Book> items = books.Skip((page -1) * pageSize).Take(pageSize).ToList();
        
        ViewBag.Page = page;
        ViewBag.totalPages = totalPages;
        
        
        return View(items);
    }

    public IActionResult Details(int id)
    {
        Book? book = _context.Books.Include(book => book.Category).FirstOrDefault(book => book.Id == id);
        if (book == null)
        {
            return NotFound("Book not found");
        }
        return View(book);
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.Categories = new SelectList(
            _context.Categories.OrderBy(category => category.Name).ToList(),
            "Id",
            "Name"
        );
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Book book)
    {
        if (book.CategoryId == null)
        {
            ModelState.AddModelError("CategoryId", "Choose category");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Categories = new SelectList(
                _context.Categories.OrderBy(category => category.Name).ToList(),
                "Id",
                "Name",
                book.CategoryId
            );

            return View(book);
        }

        book.CreatedOn = DateTime.UtcNow;
        book.IsIssued = false;

        _context.Books.Add(book);
        _context.SaveChanges();

        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        Book? book = _context.Books.FirstOrDefault(book => book.Id == id);

        if (book == null)
        {
            return NotFound();
        }
        
        ViewBag.Categories = new SelectList(
            _context.Categories.OrderBy(category => category.Name).ToList(),
            "Id",
            "Name",
            book.CategoryId
        );
        return View(book);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(int id, Book book)
    {
        if (id != book.Id)
        {
            return NotFound();
        }

        if (book.CategoryId == null)
        {
            ModelState.AddModelError("CategoryId", "Choose category");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Categories = new SelectList(
                _context.Categories.OrderBy(category => category.Name).ToList(),
                "Id",
                "Name",
                book.CategoryId
            );

            return View(book);
        }

        Book? existingBook = _context.Books.FirstOrDefault(existingBook => existingBook.Id == id);

        if (existingBook == null)
        {
            return NotFound();
        }

        existingBook.Title = book.Title;
        existingBook.Author = book.Author;
        existingBook.CoverImageUrl = book.CoverImageUrl;
        existingBook.ReleaseYear = book.ReleaseYear;
        existingBook.Description = book.Description;
        existingBook.CategoryId = book.CategoryId;

        _context.SaveChanges();

        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Delete(int id)
    {
        Book? book = _context.Books.FirstOrDefault(book => book.Id == id);

        if (book == null)
        {
            return NotFound();
        }
        return View(book);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        Book? book = _context.Books.FirstOrDefault(book => book.Id == id);

        if (book == null)
        {
            return NotFound();
        }
        
        _context.Books.Remove(book);
        _context.SaveChanges();
        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Take(int id)
    {
        Book? book = _context.Books.FirstOrDefault(book => book.Id == id);

        if (book == null)
        {
            return NotFound();
        }

        if (book.IsIssued)
        {
            TempData["Message"] = "This book is already issued.";
            return RedirectToAction("Details", new { id = id });
        }

        TakeBookViewModel model = new TakeBookViewModel
        {
            BookId = id
        };

        ViewBag.Book = book;

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Take(TakeBookViewModel model)
    {
        Book? book = _context.Books.FirstOrDefault(book => book.Id == model.BookId);

        if (book == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Book = book;
            return View(model);
        }

        if (book.IsIssued)
        {
            ModelState.AddModelError("", "This book is already issued.");
            ViewBag.Book = book;
            return View(model);
        }

        string email = model.Email.Trim().ToLower();

        LibraryUser? user = _context.LibraryUsers
            .FirstOrDefault(user => user.Email.ToLower() == email);

        if (user == null)
        {
            ModelState.AddModelError("", "User with this email was not found.");
            ViewBag.Book = book;
            return View(model);
        }

        int activeBooksCount = _context.BookIssues
            .Count(issue => issue.LibraryUserId == user.Id && issue.ReturnedAt == null);

        if (activeBooksCount >= 3)
        {
            ModelState.AddModelError("", "User cannot take more than 3 books.");
            ViewBag.Book = book;
            return View(model);
        }

        BookIssue bookIssue = new BookIssue
        {
            BookId = book.Id,
            LibraryUserId = user.Id,
            IssuedAt = DateTime.UtcNow,
            ReturnedAt = null
        };

        _context.BookIssues.Add(bookIssue);

        book.IsIssued = true;

        _context.SaveChanges();

        return RedirectToAction("Index");
    }
}