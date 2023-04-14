using BookShopping.Data;
using BookShopping.IRepository;
using BookShopping.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;

namespace BookShopping.Areas.Authenticated.Controllers;

[Area(Constants.Areas.AuthenticatedArea)]
[Authorize(Roles = Constants.Roles.StoreOwnerRole)]
public class CategoriesController : Microsoft.AspNetCore.Mvc.Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IToastNotification _toastNotification;

    public CategoriesController(ApplicationDbContext db, IToastNotification toastNotification)
    {
        _db = db;
        _toastNotification = toastNotification;
    }

    [HttpGet]
    public IActionResult Index()
    {
        // to filter all categories that have been approved by the admin
        var categories = _db.Categories.Where(c => c.Status == Category.StatusEnum.Approved == true).ToList();
        
        return View(categories);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new Category());
    }
    
    [HttpPost]
    public IActionResult Create(Category category)
    {
        // By default, the status is set to false
        if (!ModelState.IsValid)
        {
            return View(category);
        }
        
        var pendingCategory =  Category.StatusEnum.Pending;
        category.Status = pendingCategory;
        
        _db.Categories.Add(category);
        _db.SaveChanges();
        _toastNotification.AddSuccessToastMessage("Category created successfully.");
        
        
        return RedirectToAction(nameof(Index));
    }
    
    [HttpGet]
    public IActionResult Update(int categoryId)
    {
        var category = _db.Categories.Find(categoryId);
        if (category == null)
        {
            return NotFound();
        }
        
        return View(category);
    }

    [HttpPost]
    public IActionResult Update(Category category)
    {
        if (!ModelState.IsValid)
        {
            return View(category);
        }
        
        _db.Categories.Update(category);
        _db.SaveChanges();
        _toastNotification.AddSuccessToastMessage("Category created successfully.");
        
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Delete(int categoryId)
    {
        var category = _db.Categories.Find(categoryId);
        if (category == null)
        {
            return NotFound();
        }
        
        _db.Categories.Remove(category);
        _db.SaveChanges();
        _toastNotification.AddSuccessToastMessage("Category deleted successfully.");
        
        
        return RedirectToAction(nameof(Index));
    }
}