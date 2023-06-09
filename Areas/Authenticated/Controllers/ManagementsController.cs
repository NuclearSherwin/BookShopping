﻿using System.Security.Claims;
using BookShopping.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookShopping.Areas.Authenticated.Controllers;

[Area(Constants.Areas.AuthenticatedArea)]

public class ManagementsController : Microsoft.AspNetCore.Mvc.Controller
{
    private readonly ApplicationDbContext _db;

    public ManagementsController(ApplicationDbContext db)
    {
        _db = db;
    }

    // GET
    

    [HttpGet]
    [Authorize(Roles = Constants.Roles.StoreOwnerRole + "," + Constants.Roles.CustomerRole)]
    public IActionResult Index()
    {
        List<int> listId = new List<int>();

        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

        // check if user has the "storeowner" role
        bool isStoreOwner = User.IsInRole(Constants.Roles.StoreOwnerRole);

        if (isStoreOwner)
        {
            // if user has "storeowner" role, display all orders
            var orderList = _db.Orders
                .Include(o => o.User)
                .ToList();

            return View(orderList);
        }
        else
        {
            // if user has any other role, display only orders belonging to that user
            var orderList = _db.Orders
                .Where(x => x.UserId == claims.Value)
                .Include(o => o.User)
                .ToList();

            return View(orderList);
        }
    }


    [HttpGet]
    [Authorize(Roles = Constants.Roles.StoreOwnerRole)]
    public IActionResult Detail(int managementId)
    {
        var managementDetail = _db.OrderDetails
            .Where(od => od.OrderId == managementId)
            .Include(od => od.Book)
            .ToList();
        
        return View(managementDetail);
    }
}

