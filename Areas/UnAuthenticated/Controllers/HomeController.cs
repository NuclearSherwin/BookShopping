using System.Diagnostics;
using BookShopping.Data;
using BookShopping.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookShopping.Areas.UnAuthenticated.Controllers
{
    [Area(Constants.Areas.UnAuthenticatedArea)]
    public class HomeController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;
        private readonly int _recordPerPage = 40;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public IActionResult Index(int id, string searchString = "")
        {
            var books = _db.Books
                .Where(b => b.Name.Contains(searchString) || b.Author.Contains(searchString) ||
                            b.Category.Name.Contains(searchString)
                            || b.Description.Contains(searchString))
                .Include(p => p.Category)
                .ToList();

            int numOfRecord = books.Count();
            int numOfPages = (int)Math.Ceiling((double)numOfRecord / _recordPerPage);

            ViewBag.numOfPages = numOfPages;
            ViewBag.currentPage = id;
            ViewData["Current Filter"] = searchString;

            var bookList = books.Skip(id * numOfPages).Take(_recordPerPage).ToList();
            ViewData["Message"] = "Welcome!";
            
            return View(bookList);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}