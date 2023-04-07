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
        var books = _db.Books.Include(_=>_.Category).Include(_=>_.FileModel).ToList();
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
        await _db.SaveChangesAsync();

        input.Book.FileId = file.Id;

        _db.Books.Add(input.Book);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Update(int bookId)
    {
        var upsertVM = new UpsetBookViewModel()
        {
            Book = _db.Books.Find(bookId),
            Categories = _db.Categories.ToList().Select(_ => new SelectListItem()
            {
                Value = _.Id.ToString(),
                Text = _.Name

            }).ToList()

        };
        return View(upsertVM);
    }
// nho sua lai Update

    public IActionResult Update(UpsetBookViewModel input)
    {

        _db.Books.Update(input.Book);
        _db.SaveChanges();
        return RedirectToAction(nameof(Index));

    }
    public IActionResult Delete(int bookId)
    {
        var book =_db.Books.Find(bookId);

        _db.Books.Remove(book);
        _db.SaveChanges();
            
        return RedirectToAction(nameof(Index));
    }
}
    