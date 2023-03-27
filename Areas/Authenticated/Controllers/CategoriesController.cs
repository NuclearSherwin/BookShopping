using BookShopping.Data;
using BookShopping.Models;
using Microsoft.AspNetCore.Mvc;

namespace BookShopping.Areas.Authenticated.Controllers;

[Area(Constants.Areas.AuthenticatedArea)]
public class CategoriesController : Microsoft.AspNetCore.Mvc.Controller
{
    private readonly ApplicationDbContext _db;

    public CategoriesController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var category = _db.Categories.ToList();
        return View(category);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new Category());
    }
    [HttpPost]
    public IActionResult Create(Category category)
    {
        _db.Categories.Add(category);
        _db.SaveChanges();
        return RedirectToAction(nameof(Index));
    }
    
    [HttpGet]
    public IActionResult Update(int categoryId)
    {
        var category = _db.Categories.Find(categoryId);
        return View(category);
    }

    [HttpPost]
    public IActionResult Update(Category category)
    {

        _db.Categories.Update(category);
        _db.SaveChanges();
        return RedirectToAction(nameof(Index));

    }
}