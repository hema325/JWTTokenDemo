using JWTToken.Constants;
using JWTToken.Entities;
using Microsoft.AspNetCore.Identity;

namespace JWTToken.Seeds
{
    public static class DefaultUser
    {
        public static async Task SeedAsync(UserManager<ApplicationUser> userManager)
        {
            var user = new ApplicationUser
            {
                FirstName = "Ibrahim",
                LastName = "Moawad",
                UserName = "admin@gmail.com",
                Email = "admin@gmail.com",
                PhoneNumber = "+20222222222",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };


            if (!userManager.Users.Any())
            {
                var result = await userManager.CreateAsync(user, "Hema123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, Roles.Admin);
                    await userManager.AddToRoleAsync(user, Roles.User);
                }
            }
        }
    }
}
