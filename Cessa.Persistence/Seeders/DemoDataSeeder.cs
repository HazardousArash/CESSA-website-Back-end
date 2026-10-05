namespace Cessa.Persistence.Seeders
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Cessa.Domain.Entities.Auth;
    using Cessa.Persistence.Contexts;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;

    public static class DemoDataSeeder
    {
        public static async Task SeedAsync(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager, 
            RoleManager<ApplicationRole> roleManager,
            IConfiguration configuration)
        {
            // ۱. اطمینان از وجود نقش‌ها
            var roles = new[] { ApplicationRole.Admin, ApplicationRole.ContentCreator, ApplicationRole.Student };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new ApplicationRole { Name = role });
                }
            }

            // ۲. خواندن مشخصات از appsettings.json
            var adminEmail = configuration["AdminUser:Email"] ?? "admin@cessa.com";
            var adminPassword = configuration["AdminUser:Password"] ?? "Admin19e8y~!";
            var firstName = configuration["AdminUser:FirstName"] ?? "مدیر";
            var lastName = configuration["AdminUser:LastName"] ?? "سیستم";

            // ۳. بررسی ادمین در دیتابیس
            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
            if (existingAdmin == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = firstName,
                    LastName = lastName,
                    EmailConfirmed = true,
                    IsDeleted = false
                };

                var result = await userManager.CreateAsync(admin, adminPassword);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new Exception($"Failed to seed Admin user: {errors}");
                }

                await userManager.AddToRoleAsync(admin, ApplicationRole.Admin);
            }
            else
            {
                // اگر ادمین از قبل در دیتابیس بود، رمز آن را دقیقاً به رمز جدید ریست کن!
                var resetToken = await userManager.GeneratePasswordResetTokenAsync(existingAdmin);
                await userManager.ResetPasswordAsync(existingAdmin, resetToken, adminPassword);

                existingAdmin.FirstName = firstName;
                existingAdmin.LastName = lastName;
                existingAdmin.EmailConfirmed = true;
                existingAdmin.IsDeleted = false;
                existingAdmin.LockoutEnd = null;

                await userManager.UpdateAsync(existingAdmin);

                if (!await userManager.IsInRoleAsync(existingAdmin, ApplicationRole.Admin))
                {
                    await userManager.AddToRoleAsync(existingAdmin, ApplicationRole.Admin);
                }
            }
        }
    }
}
