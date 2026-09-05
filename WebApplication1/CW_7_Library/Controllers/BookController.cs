using WebApplication1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            pageSize = 1;
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
        Book? book = _context.Books.FirstOrDefault(book => book.Id == id);
        if (book == null)
        {
            return NotFound("Book not found");
        }
        return View(book);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Book book)
    {
        if (ModelState.IsValid)
        {
            book.CreatedOn =  DateTime.UtcNow;
            book.IsIssued = false;
            
            _context.Books.Add(book);
            _context.SaveChanges();
            
            return RedirectToAction(nameof(Index));
        }
        return View(book);
        
    }

    [HttpGet]
    public IActionResult Edit(int id)
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
    public IActionResult Edit(Book book)
    {
        if (ModelState.IsValid)
        {
            Book? existingBook = _context.Books.FirstOrDefault(item => item.Id == book.Id);

            if (existingBook == null)
            {
                return NotFound();
            }
            
            existingBook.Title = book.Title;
            existingBook.Author = book.Author;
            existingBook.CoverImageUrl = book.CoverImageUrl;
            existingBook.ReleaseYear = book.ReleaseYear;
            existingBook.Description = book.Description;
            
            _context.SaveChanges();
            
            return RedirectToAction("Index");
        }
        return View(book);
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Take(int id)
    {
        Book? book = _context.Books.FirstOrDefault(book => book.Id == id);

        if (book == null)
        {
            return NotFound();
        }

        if (!book.IsIssued)
        {
            book.IsIssued = true;
            _context.SaveChanges();
        }
        
        return RedirectToAction("Index");
    }
}