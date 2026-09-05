using WebApplication1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.ViewModel;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApplication1.Controllers;

public class BookController : Controller
{
    private readonly LibraryDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public BookController(LibraryDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    public IActionResult Index(string? title, string? author, string? status, string? sortOrder, int  page = 1)
    {
        int pageSize = 2;
        
        IQueryable<Book> books = _context.Books;
        
        if (!string.IsNullOrWhiteSpace(title))
        {
            string titleSearch = title.Trim().ToLower();

            books = books.Where(book => book.Title.ToLower().Contains(titleSearch));
        }

        if (!string.IsNullOrWhiteSpace(author))
        {
            string authorSearch = author.Trim().ToLower();

            books = books.Where(book => book.Author.ToLower().Contains(authorSearch));
        }
        
        if (status == "available")
        {
            books = books.Where(book => !book.IsIssued);
        }
        else if (status == "issued")
        {
            books = books.Where(book => book.IsIssued);
        }
        
        books = sortOrder switch
        {
            "title" => books.OrderBy(book => book.Title),
            "title_desc" => books.OrderByDescending(book => book.Title),

            "author" => books.OrderBy(book => book.Author),
            "author_desc" => books.OrderByDescending(book => book.Author),

            "status" => books.OrderBy(book => book.IsIssued),
            "status_desc" => books.OrderByDescending(book => book.IsIssued),

            _ => books.OrderByDescending(book => book.CreatedOn)
        };
        
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
        
        ViewBag.TitleFilter = title;
        ViewBag.AuthorFilter = author;
        ViewBag.StatusFilter = status;
        ViewBag.SortOrder = sortOrder;
        
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
    public IActionResult Create(Book book, IFormFile? pdfFile)
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

        if (pdfFile != null && pdfFile.Length > 0)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "book-files");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(pdfFile.FileName);
            string filePath = Path.Combine(uploadsFolder, fileName);

            using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
            {
                pdfFile.CopyTo(fileStream);
            }

            book.PdfFilePath = "/book-files/" + fileName;
        }
        
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
    public IActionResult Edit(int id, Book book, IFormFile? pdfFile)
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
        
        if (pdfFile != null && pdfFile.Length > 0)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "book-files");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(pdfFile.FileName);
            string filePath = Path.Combine(uploadsFolder, fileName);

            using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
            {
                pdfFile.CopyTo(fileStream);
            }

            existingBook.PdfFilePath = "/book-files/" + fileName;
        }

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