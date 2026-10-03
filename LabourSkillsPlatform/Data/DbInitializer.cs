using LabourSkillsPlatform.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LabourSkillsPlatform.Data
{
    /// <summary>
    /// Seeds RBAC roles, a default Admin account, and baseline Categories/Skills so the
    /// system is immediately usable for demonstration and thesis defense (Chapter 3, 3.2.1 - Deployment/UAT).
    /// </summary>
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var configuration = services.GetRequiredService<IConfiguration>();
            var environment = services.GetRequiredService<IHostEnvironment>();

            await context.Database.MigrateAsync();

            // ---------- Roles ----------
            foreach (var role in new[] { Roles.Admin, Roles.Employer, Roles.Labourer })
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // ---------- Default Admin ----------
            var adminEmail = configuration["SeedAdmin:Email"];
            var adminPassword = configuration["SeedAdmin:Password"];
            if (environment.IsDevelopment())
            {
                adminEmail ??= "admin@lsp.local";
                adminPassword ??= "Admin@12345";
            }

            if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            {
                throw new InvalidOperationException(
                    "Set SeedAdmin:Email and SeedAdmin:Password in production configuration before starting the application.");
            }

            if (await userManager.FindByEmailAsync(adminEmail) is null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    EmailConfirmed = true,
                    IsActive = true
                };

                var result = await userManager.CreateAsync(admin, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, Roles.Admin);
                }
            }

            // ---------- Baseline Categories & Skills ----------
            if (!await context.Categories.AnyAsync())
            {
                var categories = new List<Category>
                {
                    new() { Name = "Electrical", Description = "Wiring, installation and repair of electrical systems" },
                    new() { Name = "Plumbing", Description = "Pipe fitting, water systems and drainage work" },
                    new() { Name = "Masonry & Construction", Description = "Building, bricklaying and general construction" },
                    new() { Name = "Carpentry", Description = "Woodwork, furniture and structural carpentry" },
                    new() { Name = "Painting", Description = "Interior and exterior painting services" },
                    new() { Name = "Cleaning Services", Description = "Residential and commercial cleaning" }
                };
                context.Categories.AddRange(categories);
                await context.SaveChangesAsync();

                var skillsByCategory = new Dictionary<string, string[]>
                {
                    ["Electrical"] = new[] { "Residential Wiring", "Circuit Installation", "Solar Panel Setup" },
                    ["Plumbing"] = new[] { "Pipe Fitting", "Leak Repair", "Water Tank Installation" },
                    ["Masonry & Construction"] = new[] { "Bricklaying", "Concrete Work", "Tiling" },
                    ["Carpentry"] = new[] { "Furniture Making", "Door & Window Fitting", "Roof Framing" },
                    ["Painting"] = new[] { "Wall Painting", "Spray Finishing", "Surface Preparation" },
                    ["Cleaning Services"] = new[] { "Deep Cleaning", "Office Cleaning", "Post-Construction Cleaning" }
                };

                var savedCategories = await context.Categories.ToListAsync();
                foreach (var cat in savedCategories)
                {
                    if (skillsByCategory.TryGetValue(cat.Name, out var skillNames))
                    {
                        foreach (var name in skillNames)
                        {
                            context.Skills.Add(new Skill { Name = name, CategoryId = cat.Id });
                        }
                    }
                }
                await context.SaveChangesAsync();
            }
        }
    }
}
