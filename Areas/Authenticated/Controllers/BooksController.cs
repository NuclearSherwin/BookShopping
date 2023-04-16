using BookShopping.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using BookShopping.Models;
using BookShopping.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BookShopping.Areas.Authenticated.Controllers;

[Area(Constants.Areas.AuthenticatedArea)]
public class BooksController : Controller
{
    private readonly ApplicationDbContext _db;

    public BooksController(ApplicationDbContext db)
    {
        _db = db;
    }

    public IActionResult Index()
    {
        var books = _db.Books.Include(_ => _.Category).Include(_ => _.FileModel).ToList();
        return View(books);
    }

    public IActionResult GetImage(int id)
    {
        var image = _db.Files.Find(id);

        return File(image.Data, image.ContentType);
    }

    [HttpGet]
    public IActionResult Create()
    {
        var upsertVM = new UpsetBookViewModel()
        {
            Book = new Book(),
            Categories = _db.Categories.ToList()
                .Where(c => c.Status == Category.StatusEnum.Approved) // filter approved categories
                .ToList()
                .Select(_ => new SelectListItem()
                {
                    Value = _.Id.ToString(),
                    Text = _.Name
                }).ToList()
        };
        return View(upsertVM);
    }

    [HttpPost]
    public async Task<IActionResult> Create(UpsetBookViewModel input)
    {
        if (input.Book.Description == null ||
            input.Book.Name == null ||
            input.Book.CategoryId == null ||
            input.Book.Author == null ||
            input.Book.Price == null ||
            input.Book.NoPage == null)
        {
            input.Categories = _db.Categories.ToList()
                .Where(c => c.Status == Category.StatusEnum.Approved) // filter approved categories
                .ToList()
                .Select(_ => new SelectListItem()
                {
                    Value = _.Id.ToString(),
                    Text = _.Name
                }).ToList(); // cap du lieu cho categorie de co the update neu khong co se khong the lay categori
            return View(input);
        }

        if (input.File == null)
        {
        }

        byte[] fileData;
        using (var ms = new MemoryStream())
        {
            await input.File.CopyToAsync(ms);
            fileData = ms.ToArray();
        }

        var file = new FileModel()
        {
            Name = input.File.FileName,
            ContentType = input.File.ContentType,
            Data = fileData
        };
        await _db.Files.AddAsync(file);
        await _db.SaveChangesAsync();

        input.Book.FileId = file.Id;

        await _db.Books.AddAsync(input.Book);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Update(int bookId)
    {
        var book = _db.Books.Find(bookId);
        var file = _db.Files.Find(book.FileId);
        var upsertVM = new UpsetBookViewModel()
        {
            Book = book,
            OldFileName = file != null ? file.Name : "No File selected",
            Categories = _db.Categories.ToList().Select(_ => new SelectListItem()
            {
                Value = _.Id.ToString(),
                Text = _.Name
            }).ToList()
        };
        return View(upsertVM);
    }
// nho sua lai Update

    [HttpPost]
    public async Task<IActionResult> Update(UpsetBookViewModel input)
    {
        var bookDb = _db.Books.Find(input.Book.Id);
        var isRemoveFile = false;
        var oldFileId = 0;
        if (input.File != null)
        {
            // add new file
            byte[] fileData;
            using (var ms = new MemoryStream())
            {
                await input.File.CopyToAsync(ms);
                fileData = ms.ToArray();
            }

            var file = new FileModel()
            {
                Name = input.File.FileName,
                ContentType = input.File.ContentType,
                Data = fileData
            };
            _db.Files.Add(file);
            _db.SaveChanges();

            oldFileId = bookDb.FileId;
            // replace file
            bookDb.FileId = file.Id;

            isRemoveFile = true;
        }

        bookDb.Name = input.Book.Name;
        bookDb.Author = input.Book.Author;
        bookDb.Description = input.Book.Description;
        bookDb.Price = input.Book.Price;
        bookDb.CategoryId = input.Book.CategoryId;
        bookDb.NoPage = input.Book.NoPage;

        _db.Books.Update(bookDb);
        _db.SaveChanges();

        if (isRemoveFile && oldFileId != 0)
        {
            // remove old file
            var oldFile = _db.Files.Find(oldFileId);
            _db.Files.Remove(oldFile);
            _db.SaveChanges();
        }

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Delete(int bookId)
    {
        var book = _db.Books.Find(bookId);

        _db.Books.Remove(book);
        _db.SaveChanges();

        return RedirectToAction(nameof(Index));
    }
}