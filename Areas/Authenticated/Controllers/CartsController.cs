using System.Security.Claims;
using BookShopping.Data;
using BookShopping.Models;
using BookShopping.Utils;
using BookShopping.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;

namespace BookShopping.Areas.Authenticated.Controllers;

[Area(Constants.Areas.AuthenticatedArea)]
// [Authorize(Roles = Constants.Roles.CustomerRole)]
public class CartsController : Microsoft.AspNetCore.Mvc.Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IToastNotification _toastNotification;


    public CartsController(ApplicationDbContext db, IToastNotification toastNotification)
    {
        _toastNotification = toastNotification;
        _db = db;
    }

    [BindProperty] public ShoppingCartVM ShoppingCart { get; set; }

    //get 
    public IActionResult Index()
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

        ShoppingCart = new ShoppingCartVM()
        {
            Orders = new Models.Order(),
            ListCart = _db.Carts.Where(u => u.UserId == claim.Value).Include(p => p.Book.Category)
        };
        ShoppingCart.Orders.Total = 0;
        ShoppingCart.Orders.User = _db.Users
            .FirstOrDefault(u => u.Id == claim.Value);

        foreach (var list in ShoppingCart.ListCart)
        {
            list.Price = list.Book.Price;
            ShoppingCart.Orders.Total += (list.Price * list.Count);
            if (list.Book.Description.Length > 100)
            {
                list.Book.Description = list.Book.Description.Substring(0, 99) + "...";
            }
        }

        return View(ShoppingCart);
    }

    // plus 
    public IActionResult Plus(int CartId)
    {
        var cart = _db.Carts.Include(p => p.Book).FirstOrDefault(c => c.Id == CartId);

        cart.Count += 1;
        cart.Price = cart.Book.Price;
        if (cart.Book.Total >= cart.Count)
        {
            _db.SaveChanges();
        }
        
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Minus(int CartId)
    {
        var cart = _db.Carts.Include(p => p.Book).FirstOrDefault(c => c.Id == CartId);
        if (cart.Count == 1)
        {
            var cnt = _db.Carts.Where(u => u.UserId == cart.UserId).ToList().Count;
            _db.Carts.Remove(cart);
            _db.SaveChanges();
        }
        else
        {
            cart.Count -= 1;
            cart.Price = cart.Book.Price;
            _db.SaveChanges();
        }

        return RedirectToAction(nameof(Index));
    }

    // delete
    [HttpDelete]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var cart = _db.Carts.Include(p => p.Book)
                .FirstOrDefault(c => c.Id == id);

            // get all ids
            var cnt = _db.Carts.Where(u => u.UserId == cart.UserId).ToList().Count;
            if (cart == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            _db.Carts.Remove(cart);
            await _db.SaveChangesAsync();
            HttpContext.Session.SetInt32(Constants.Session.ssShoppingCart, cnt - 1);
            return Json(new { success = true, message = "Delete successfully!" });
        }
        catch (Exception e)
        {
            return Json(new { success = false, message = e.Message });
        }
    }

    // summary
    [HttpGet]
    public IActionResult Summary()
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

        ShoppingCart = new ShoppingCartVM()
        {
            Orders = new Models.Order(),
            ListCart = _db.Carts.Where(u => u.UserId == claim.Value)
                .Include(c => c.Book)
        };

        ShoppingCart.Orders.User = _db.Users
            .FirstOrDefault(u => u.Id == claim.Value);

        foreach (var list in ShoppingCart.ListCart)
        {
            list.Price = list.Book.Price;
            ShoppingCart.Orders.Total += (list.Price + list.Count);
        }

        ShoppingCart.Orders.Address = ShoppingCart.Orders.User.Address;
        ShoppingCart.Orders.OrderDate = DateTime.Now;

        return View(ShoppingCart);
    }

    // summary post
    [HttpPost]
    [ActionName("Summary")]
    [ValidateAntiForgeryToken]
    public IActionResult SummaryPost()
    {
        // lay id cua user
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

        // lay thong tin user dang mua
        // lay toan bo list cart
        ShoppingCart.Orders.User = _db.Users
            .FirstOrDefault(c => c.Id == claim.Value);
        ShoppingCart.ListCart = _db.Carts.Where(c => c.UserId == claim.Value)
            .Include(c => c.Book);


        // assign value for each field in order header
        ShoppingCart.Orders.UserId = claim.Value;
        ShoppingCart.Orders.Address = ShoppingCart.Orders.User.Address;
        ShoppingCart.Orders.OrderDate = DateTime.Now;

        // add to order header and save changes to get order header id
        _db.Orders.Add(ShoppingCart.Orders);
        _db.SaveChanges();

        // moi san pham add vao order detail
        foreach (var item in ShoppingCart.ListCart)
        {
            item.Price = item.Book.Price;
            OrderDetail orderDetail = new OrderDetail()
            {
                BookId = item.BookId,
                OrderId = ShoppingCart.Orders.Id,
                Price = item.Price,
                Quantity = item.Count
            };

            // calculate total for order header and add to order detail
            ShoppingCart.Orders.Total += orderDetail.Quantity + orderDetail.Price;
            _db.OrderDetails.Add(orderDetail);
            
            var book = _db.Books.Find(item.BookId);
            book.Total -= item.Count;
            _db.Books.Update(book);
        }

        // remove that item from cart
        _db.Carts.RemoveRange(ShoppingCart.ListCart);
        _db.SaveChanges();
        HttpContext.Session.SetInt32(Constants.Session.ssShoppingCart, 0);
        
        _toastNotification.AddSuccessToastMessage("Order successfully");

        return RedirectToAction("OrderConfirmation", "Carts",
            new { id = ShoppingCart.Orders.Id });
    }

    // order confirm 
    public IActionResult OrderConfirmation(int id)
    {
        var claimIdentity = (ClaimsIdentity)User.Identity;
        var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
        var emailDb = _db.Users.Find(claim.Value).Email;

        MailContent content = new MailContent()
        {
            To = emailDb,
            Subject = "Thanks for order! Let's explore more",
            Body = "<p>Order successfully!</p>"
        };


        return View(id);
    }
}