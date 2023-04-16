using BookShopping.Constants;
using BookShopping.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using BookShopping.Models;
using BookShopping.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using NToastNotify;

namespace BookShopping.Areas.Authenticated.Controllers;
[Area(Constants.Areas.AuthenticatedArea)]
[Authorize(Roles = Constants.Roles.StoreOwnerRole + "," + Constants.Roles.CustomerRole)]
public class BooksController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IToastNotification _toastNotification;

    public BooksController(ApplicationDbContext db, IToastNotification toastNotification)
    {
        _db = db;
        _toastNotification = toastNotification;
    }

    [Authorize(Roles = Constants.Roles.StoreOwnerRole)]
    public IActionResult Index()
    {
        var books = _db.Books.Include(_=>_.Category).Include(_=>_.FileModel).ToList();
        return View(books);
    }
    
    [AllowAnonymous]
    public IActionResult GetImage(int id)
    {
        var image = _db.Files.Find(id);
       
        return File(image.Data, image.ContentType);
    }
    
    [HttpGet]
    [Authorize(Roles = Constants.Roles.StoreOwnerRole)]
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
    [Authorize(Roles = Constants.Roles.StoreOwnerRole)]
    public async Task<IActionResult> Create(UpsetBookViewModel input)
    {
        if (!ModelState.IsValid)
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
            
            _toastNotification.AddSuccessToastMessage("Book created successfully!");
            
            
            return RedirectToAction(nameof(Index));
            
        }
        else
        {
            return View(input);
        }
    }

    
    [HttpGet]
    [Authorize(Roles = Constants.Roles.StoreOwnerRole)]
    public async Task<IActionResult> Update(int id)
    {
        var book = await _db.Books.FindAsync(id);

        if (book == null)
        {
            return NotFound();
        }

        var upsertVM = new UpsetBookViewModel()
        {
            Book = book,
            OldFileName = book.FileId != 0 && book.FileId != null ? _db.Files.Find(book.FileId).Name : String.Empty,
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
    [Authorize(Roles = Constants.Roles.StoreOwnerRole)]
    public async Task<IActionResult> Update(int id, UpsetBookViewModel input)
    {
        if (id != input.Book.Id)
        {
            return NotFound();
        }
        

        var book = await _db.Books.FindAsync(id);

        if (book == null)
        {
            return NotFound();
        }

        book.Name = input.Book.Name;
        book.Description = input.Book.Description;
        book.Author = input.Book.Author;
        book.Price = input.Book.Price;
        book.CategoryId = input.Book.CategoryId;
        book.NoPage = input.Book.NoPage;
        book.Total = input.Book.Total;
        var isRemoveFile = false;
        var oldFileId = book.FileId;
        if (input.File != null)
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
            await _db.Files.AddAsync(file);
            await _db.SaveChangesAsync();
            
            _toastNotification.AddSuccessToastMessage("book updated successfully!");
    
            book.FileId = file.Id;
            isRemoveFile = true;
        }
        await _db.SaveChangesAsync();
        
        if (isRemoveFile && oldFileId != 0)
        {
            // remove old file
            var oldFile = await _db.Files.FindAsync(oldFileId);
            _db.Files.Remove(oldFile);
            await _db.SaveChangesAsync();
        }
        
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Constants.Roles.StoreOwnerRole)]
    public IActionResult Delete(int bookId)
    {
        var book =_db.Books.Find(bookId);

        _db.Books.Remove(book);
        _db.SaveChanges();
        
        _toastNotification.AddSuccessToastMessage("Book delete successfully!");
            
        return RedirectToAction(nameof(Index));
    }
}
    