using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace WebProcessManager.Data
{
    public class DataSeeder
    {
        public static void SeedData
        (UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            SeedRoles(roleManager);
            SeedUsers(userManager);
        }

        public static void SeedUsers
            (UserManager<IdentityUser> userManager)
        {
            if (userManager.FindByNameAsync
                    ("admin").Result == null)
            {
                var user = new IdentityUser
                {
                    UserName = "admin",
                    Email = "admin",
                    NormalizedEmail = "admin".ToUpper()
                };

                var result = userManager.CreateAsync
                    (user, "Echo@1927").Result;

                if (result.Succeeded)
                {
                    userManager.AddToRoleAsync(user,
                        "Administrator").Wait();
                }
            }
        }

        public static void SeedRoles
            (RoleManager<IdentityRole> roleManager)
        {
            if (!roleManager.RoleExistsAsync
                ("NormalUser").Result)
            {
                var role = new IdentityRole()
                {
                    Name = "NormalUser",
                    NormalizedName = "NormalUser".ToUpper()
                };
                var roleResult = roleManager.
                    CreateAsync(role).Result;
            }


            if (!roleManager.RoleExistsAsync
                ("Administrator").Result)
            {
                var role = new IdentityRole()
                {
                    Name = "Administrator",
                    NormalizedName = "Administrator".ToUpper()
                };
                var roleResult = roleManager.
                    CreateAsync(role).Result;
            }
        }
    }
}
