using JWTToken.Data;
using JWTToken.Entities;
using JWTToken.Seeds;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JWTToken.DbInitializer
{
    public class DbInitializer: IDbInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbInitializer(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task InitializeAsync()
        {
            await _context.Database.MigrateAsync();
            await SeedAsync();
        }

        private async Task SeedAsync()
        {
            await DefaultRoles.SeedAsync(_roleManager);
            await DefaultUser.SeedAsync(_userManager);
        }

    }
}
