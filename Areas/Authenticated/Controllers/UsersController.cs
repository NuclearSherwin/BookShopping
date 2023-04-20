using System.Security.Claims;
using BookShopping.Constants;
using BookShopping.Data;
using BookShopping.Models;
using BookShopping.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using NToastNotify;

namespace BookShopping.Areas.Authenticated.Controllers;

[Area(Constants.Areas.AuthenticatedArea)]
[Authorize(Roles = Constants.Roles.AdminRole)]
public class UsersController : Microsoft.AspNetCore.Mvc.Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IToastNotification _toastNotification;

    public UsersController(ApplicationDbContext db, 
        UserManager<IdentityUser> userManager, 
        RoleManager<IdentityRole> roleManager,
        IToastNotification toastNotification
    )
    {
        _db = db;
        _userManager = userManager;
        _roleManager = roleManager;
        _toastNotification = toastNotification;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // taking current login user ID
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

        var userList = _db.Users.Where(x => x.Id != claim.Value);
        
        foreach (var user in userList)
        {
            var userTemp = await _userManager.FindByIdAsync(user.Id);
            var roleTemp = await _userManager.GetRolesAsync(userTemp);
            user.Role = roleTemp.FirstOrDefault();
        }
        
        return View(userList.ToList());

        // var users = _db.Users.ToList();
        //
        // return View(users);
    }

    [HttpGet]
    public async Task<IActionResult> LockUnLock(string id)
    {
        var claimsIdentity = (ClaimsIdentity)User.Identity;
        var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

        var userNeedToLock = _db.Users.First(x => x.Id == id);

        if (userNeedToLock.Id == claims.Value)
        {
            // show the error that you are locking the your account
        }

        if (userNeedToLock.LockoutEnd != null && userNeedToLock.LockoutEnd > DateTime.Now)
            userNeedToLock.LockoutEnd = DateTime.Now;
        else
            userNeedToLock.LockoutEnd = DateTime.Now.AddYears(1000);

        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }



    [HttpGet]
    public async Task<IActionResult> Delete(string id)
    {
        var userToDelete = await _userManager.FindByIdAsync(id);
        if (userToDelete == null) return NotFound();
        await _userManager.DeleteAsync(userToDelete);

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    // public async Task<IActionResult> ResetPassword(string? token, string? email)
    // {
    //     if (token == null || email == null) ModelState.AddModelError("", "Invalid password reset token");
    //
    //     var resetPasswordViewModel = new ResetPasswordViewModel
    //     {
    //         Email = email,
    //         Token = token
    //     };
    //
    //     return View(resetPasswordViewModel);
    // }
    //
    // [HttpPost]
    // public async Task<IActionResult> ResetPassword(ResetPasswordViewModel resetPasswordViewModel)
    // {
    //     if (ModelState.IsValid)
    //     {
    //         var user = await _userManager.FindByEmailAsync(resetPasswordViewModel.Email);
    //         if (user != null)
    //         {
    //             var result = await _userManager.ResetPasswordAsync(user, resetPasswordViewModel.Token,
    //                 resetPasswordViewModel.Password);
    //
    //             if (result.Succeeded) return RedirectToAction(nameof(Index));
    //         }
    //     }
    //     return View(resetPasswordViewModel);
    // }

    // Reset pass
    [HttpGet]
    public async Task<IActionResult> ResetPassword()
    {
        var resetPasswordViewModel = new ResetPasswordViewModel();
        return View(resetPasswordViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel resetPasswordViewModel)
    {
        if (ModelState.IsValid)
        {
            
            var user = await _userManager.FindByEmailAsync(resetPasswordViewModel.Email);
            var userEmail = user.Email;
            if (user != null)
            {
                // Set the password without a token
                var removePasswordResult = await _userManager.RemovePasswordAsync(user);
                if (removePasswordResult.Succeeded)
                {
                    var addPasswordResult = await _userManager.AddPasswordAsync(user, resetPasswordViewModel.Password);
                    _toastNotification.AddSuccessToastMessage("Reset password for " + userEmail + "Successfully.");
                    if (addPasswordResult.Succeeded) return RedirectToAction(nameof(Index));
                }
            }
        }
        
        return View(resetPasswordViewModel);
    }
        
    // get all pending categories
    [HttpGet]
    public IActionResult Categories()
    {
        // to filter all categories that have been approved by the admin
        var categories = _db.Categories.Where(c => c.Status == Category.StatusEnum.Pending).ToList();
        
        return View(categories);
    }
    
    [HttpGet]
    public async Task<IActionResult> ApproveCategory(int categoryId)
    {
        var category = await _db.Categories.FindAsync(categoryId);
        return View(category);
    }
    
    [HttpPost]
    public async Task<IActionResult> ApproveCategory(Category category)
    {
        if (!ModelState.IsValid)
        {
            return View(category);
        }
        
        // change to status to approve
        var approvedTheCategory =  Category.StatusEnum.Approved;
        category.Status = approvedTheCategory;
        _db.Categories.Update(category);
        await _db.SaveChangesAsync();
        
        return RedirectToAction(nameof(Categories));
    }
    
    [HttpGet]
    public IActionResult RejectCategory(int categoryId)
    {
        var category = _db.Categories.Find(categoryId);
        if (category == null)
        {
            return NotFound();
        }

        category.Status = Category.StatusEnum.Rejected;
        
        _db.Categories.Remove(category);
        _db.SaveChanges();
        
        
        return RedirectToAction(nameof(Categories));
    }
    
    // edit profile
    [HttpGet]
    public IActionResult EditOwnProfile(string id)
    {
        var user = _db.Users.Find(id);
        return View(user);
    }

    [HttpPost]
    public IActionResult EditOwnProfile(User user)
    {
        var ownUser = _db.Users.Find(user.Id);
        if (ownUser == null)
        {
            return NotFound();
        }
        
        
        ownUser.FullName = user.FullName;
        ownUser.Address = user.Address;
        ownUser.PhoneNum = user.PhoneNum;
        ownUser.UpdatedAt = DateTime.Now;
        
        _db.Users.Update(ownUser);

        _db.SaveChanges();

        return RedirectToAction(nameof(Index));
    }
    

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var user = _db.Users.Find(id);
        var roleTemp = await _userManager.GetRolesAsync(user);
        var role = roleTemp.First();

        return RedirectToAction("EditOwnProfile", new { id });
        
    }
    


}