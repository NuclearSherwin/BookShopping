using System.Security.Claims;
using BookShopping.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookShopping.Areas.Authenticated.Controllers;

[Area(Constants.Areas.AuthenticatedArea)]
[Authorize(Roles = Constants.Roles.StoreOwnerRole)]
public class ManagementsController : Microsoft.AspNetCore.Mvc.Controller
{
    private readonly ApplicationDbContext _db;

    public ManagementsController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public IActionResult Index()
    {
        List<int> listId = new List<int>();

        var claimsIdentity = (ClaimsIdentity)User.Identity!;
        var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

        var orderList = _db.Orders
            .Include(o => o.User)
            .ToList();

        return View(orderList);
    }

    
    [HttpGet]
    public IActionResult Detail(int managementId)
    {
        var managementDetail = _db.OrderDetails
            .Where(od => od.OrderId == managementId)
            .Include(od => od.Book)
            .ToList();
        
        return View(managementDetail);
    }
}

