using BookShopping.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookShopping.Areas.Authenticated.Controllers;

[Area(Constants.Areas.AuthenticatedArea)]
public class OrdersController : Microsoft.AspNetCore.Mvc.Controller
{
    private readonly ApplicationDbContext _context;

    public OrdersController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var orders = _context.Orders.ToList();
        return View(orders);
    }

    public IActionResult Details(int id)
    {
        var order = _context.Orders.FirstOrDefault(o => o.Id == id);
        if (order == null)
        {
            return NotFound();
        }

        return View(order);
    }

    public IActionResult OrderDetails(int id)
    {
        var orderDetails = _context.OrderDetails
            .Where(od => od.OrderId == id)
            .Include(od => od.Book)
            .ToList();

        return View(orderDetails);
    }
}