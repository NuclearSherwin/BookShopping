using System.Diagnostics;
using System.Security.Claims;
using BookShopping.Data;
using BookShopping.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;

namespace BookShopping.Areas.UnAuthenticated.Controllers
{
    [Area(Constants.Areas.UnAuthenticatedArea)]
    public class HomeController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;
        private readonly int _recordPerPage = 40;
        private readonly IToastNotification _toastNotification;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db, IToastNotification toastNotification)
        {
            _logger = logger;
            _db = db;
            _toastNotification = toastNotification;
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
            
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            
            // get the role of current signed in
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1234"),
                new Claim(ClaimTypes.Role, "Admin")
            };
            var identity = new ClaimsIdentity(claims, "MyAuthType");

            var roleClaim = claimsIdentity.FindFirst(ClaimTypes.Role);
            if (roleClaim == null)
            {
                return View(bookList);
            }
            
            string role = roleClaim.Value;
            
            
            _toastNotification.AddInfoToastMessage("Welcome " + role + " To Book store!^^");

            
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
        
        // product detail
        [HttpGet]
        public IActionResult Detail(int id)
        {
            var bookSelected = _db.Books
                .Include(_ => _.Category)
                .FirstOrDefault(_ => _.Id == id);

            Cart cart = new Cart()
            {
                Book = bookSelected,
                BookId = bookSelected.Id
            };

            return View(cart);
        }
        
        // add product to cart
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public IActionResult Detail(Cart cartObj)
        {
            cartObj.Id = 0;
            
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            cartObj.UserId = claim.Value;

            if (cartObj.UserId == null || cartObj.BookId == null)
            {
                var bookFromDb = _db.Books
                    .Include(_ => _.Category)
                    .FirstOrDefault(b => b.Id == cartObj.BookId);

                Cart cart = new Cart()
                {
                    Book = bookFromDb,
                    BookId = bookFromDb.Id
                };

                return View(cart);
            }
            
            Cart cartFromDb = _db.Carts
                .FirstOrDefault(c => c.UserId == cartObj.UserId && c.BookId == cartObj.BookId);

            if (cartFromDb == null)
            {
                var bookFromDb = _db.Books
                    .Include(_ => _.Category)
                    .FirstOrDefault(b => b.Id == cartObj.BookId);
                
                if (bookFromDb.Total < cartObj.Count)
                {
                    ViewData["Message"] = "Total of number books is not enough!";

                    Cart cart = new Cart()
                    {
                        Book = bookFromDb,
                        BookId = bookFromDb.Id
                    };

                    return View(cart);  
                }

                _db.Carts.Add(cartObj);
                ViewData["Message"] = "Order successfully!";
            }
            else
            {
                var bookFromDb = _db.Books
                    .Include(_ => _.Category)
                    .FirstOrDefault(b => b.Id == cartObj.BookId);
                
                cartFromDb.Count += cartFromDb.Count;
                if (bookFromDb.Total < cartFromDb.Count)
                {
                    ViewData["Message"] = "Total of number books is not enough!";

                    Cart cart = new Cart()
                    {
                        Book = bookFromDb,
                        BookId = bookFromDb.Id
                    };

                    return View(cart);
                }
                
                _db.Carts.Update(cartFromDb);
            }

            _db.SaveChanges();
            
            // count product by using session
            var count = _db.Carts
                .Where(c => c.UserId == cartObj.UserId)
                .ToList().Count();
            
            
            HttpContext.Session.SetInt32(Constants.Session.ssShoppingCart, count);
            
            _toastNotification.AddSuccessToastMessage("Add to cart successfully.");

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Helper()
        {
            return View();
        }
    }
}