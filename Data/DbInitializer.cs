using Microsoft.AspNetCore.Identity;
using ChurchAttendance.Models;

namespace ChurchAttendance.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            context.Database.EnsureCreated();

            // Create roles
            if (!await roleManager.RoleExistsAsync("Administrator"))
            {
                await roleManager.CreateAsync(new IdentityRole("Administrator"));
            }
            if (!await roleManager.RoleExistsAsync("TeamLeader"))
            {
                await roleManager.CreateAsync(new IdentityRole("TeamLeader"));
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

            // Create default team leader user
            var existingTeamLeader = await userManager.FindByNameAsync("teamleader");
            if (existingTeamLeader == null)
            {
                var teamLeaderUser = new ApplicationUser
                {
                    UserName = "teamleader",
                    Email = "teamleader@church.com",
                    FullName = "Team Leader",
                    Role = UserRole.TeamLeader,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(teamLeaderUser, "Leader123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(teamLeaderUser, "TeamLeader");
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

