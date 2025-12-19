using Microsoft.AspNetCore.Identity;
using ChurchAttendance.Models;

namespace ChurchAttendance.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Database migration is handled in Program.cs

            // Create roles
            if (!await roleManager.RoleExistsAsync("Administrator"))
            {
                await roleManager.CreateAsync(new IdentityRole("Administrator"));
            }
            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(new IdentityRole("User"));
            }

            // Create default admin user
            var existingAdmin = await userManager.FindByNameAsync("admin");
            if (existingAdmin == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = "admin",
                    Email = "admin@church.com",
                    FullName = "Administrator",
                    Role = UserRole.Administrator,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Administrator");
                }
            }

            // Create default user account
            var existingUser = await userManager.FindByNameAsync("user");
            if (existingUser == null)
            {
                var regularUser = new ApplicationUser
                {
                    UserName = "user",
                    Email = "user@church.com",
                    FullName = "User",
                    Role = UserRole.User,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(regularUser, "User123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(regularUser, "User");
                }
            }

            // Create default groups if none exist
            if (!context.Groups.Any())
            {
                context.Groups.AddRange(
                    new Group { Name = "Adult - Male", Type = GroupType.Adult, GenderRestriction = Gender.Male },
                    new Group { Name = "Adult - Female", Type = GroupType.Adult, GenderRestriction = Gender.Female },
                    new Group { Name = "Young Adult - Male", Type = GroupType.YoungAdult, GenderRestriction = Gender.Male },
                    new Group { Name = "Young Adult - Female", Type = GroupType.YoungAdult, GenderRestriction = Gender.Female },
                    new Group { Name = "Children - Male", Type = GroupType.Children, GenderRestriction = Gender.Male },
                    new Group { Name = "Children - Female", Type = GroupType.Children, GenderRestriction = Gender.Female }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}

