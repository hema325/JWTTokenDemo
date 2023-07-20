using JWTToken.Constants;
using Microsoft.AspNetCore.Identity;

namespace JWTToken.Seeds
{
    public static class DefaultRoles
    {
        public static async Task SeedAsync(RoleManager<IdentityRole> roleManager)
        {
            var firstRole = new IdentityRole
            {
                Name = Roles.Admin
            };

            var secondRole = new IdentityRole
            {
                Name = Roles.User
            };

            if (!roleManager.Roles.Any())
            {
                await roleManager.CreateAsync(firstRole);
                await roleManager.CreateAsync(secondRole);
            }
        }
    }
}
