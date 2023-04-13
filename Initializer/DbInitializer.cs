using BookShopping.Data;
using BookShopping.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookShopping.Initializer;

public class DbInitializer : IDbInitializer
{
        private readonly ApplicationDbContext _db;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;

        public DbInitializer(ApplicationDbContext db, UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }


        public void Initialize()
        {
            // checking database, if not migration then migrate
            try
            {
                if (_db.Database.GetPendingMigrations().Any()) 
                {
                    _db.Database.Migrate();
                    Console.WriteLine("Migrations applied successfully.");
                }
                else
                {
                    Console.WriteLine("No pending migrations.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error applying migrations: " + e.Message);
                throw;
            }


            // checking in table Role, if yes then return, if not deploy the codes after these conditions
            if (_db.Roles.Any(r => r.Name == Constants.Roles.AdminRole)) return;
            if (_db.Roles.Any(r => r.Name == Constants.Roles.StoreOwnerRole)) return;
            if (_db.Roles.Any(r => r.Name == Constants.Roles.CustomerRole)) return;

            // this will deploy if there no have any role yet
            _roleManager.CreateAsync(new IdentityRole(Constants.Roles.AdminRole)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(Constants.Roles.StoreOwnerRole)).GetAwaiter().GetResult();
            _roleManager.CreateAsync(new IdentityRole(Constants.Roles.CustomerRole)).GetAwaiter().GetResult();

            // create user admin
            _userManager.CreateAsync(new User()
            {
                UserName = "admin@gmail.com",
                Email = "admin@gmail.com",
                FullName = "Admin",
                PhoneNum = "0869502968",
                Address = "Admin123",
                EmailConfirmed = true,
            }, "Admin123@").GetAwaiter().GetResult();


            // finding the user which is just have created
            var admin = _db.Users.FirstOrDefault(a => a.Email == "admin@gmail.com");

            // add that user (admin) to admin role
            _userManager.AddToRoleAsync(admin, Constants.Roles.AdminRole).GetAwaiter().GetResult();
        }
}