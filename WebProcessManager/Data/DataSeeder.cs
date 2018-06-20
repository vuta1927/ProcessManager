using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace WebProcessManager.Data
{
    public class DataSeeder : IDataSeeder
    {
        private UserManager<IdentityUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;
        
        public DataSeeder(UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public void SeedData()
        {
            SeedRoles();
            SeedUsers();
        }

        private void SeedUsers()
        {
            if (_userManager.FindByNameAsync
                    ("admin").Result == null)
            {
                var user = new IdentityUser
                {
                    UserName = "admin@localhost",
                    Email = "admin@localhost",
                    NormalizedEmail = "admin@localhost".ToUpper()
                };

                var result = _userManager.CreateAsync
                    (user, "Echo@1927").Result;

                if (result.Succeeded)
                {
                    _userManager.AddToRoleAsync(user,
                        "Administrator").Wait();
                }
            }
        }

        private void SeedRoles()
        {
            if (!_roleManager.RoleExistsAsync
                ("NormalUser").Result)
            {
                var role = new IdentityRole()
                {
                    Name = "NormalUser",
                    NormalizedName = "NormalUser".ToUpper()
                };
                var roleResult = _roleManager.
                    CreateAsync(role).Result;
            }


            if (!_roleManager.RoleExistsAsync
                ("Administrator").Result)
            {
                var role = new IdentityRole()
                {
                    Name = "Administrator",
                    NormalizedName = "Administrator".ToUpper()
                };
                var roleResult = _roleManager.
                    CreateAsync(role).Result;
            }
        }
    }
}
