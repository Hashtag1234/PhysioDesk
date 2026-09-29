using Microsoft.EntityFrameworkCore;
using PhysioDesk.Api.Features.Auth;

namespace PhysioDesk.Api.Common.Persistence;

/// <summary>
/// Applies pending migrations and seeds baseline data. Called once at startup.
/// As of the Auth slice this only seeds users; Patients/Therapists/Appointments/
/// Invoices seeding is added when those features land, per the build order.
/// </summary>
public static class DbInitializer
{
    public static async Task MigrateAndSeedAsync(AppDbContext db)
    {
        await db.Database.MigrateAsync();

        if (!await db.Users.AnyAsync())
        {
            db.Users.AddRange(
                new User
                {
                    Email = "admin@physiodesk.dev",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    Role = UserRole.Admin,
                    DisplayName = "Admin User"
                },
                new User
                {
                    Email = "staff@physiodesk.dev",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Staff123!"),
                    Role = UserRole.Staff,
                    DisplayName = "Staff User"
                });

            await db.SaveChangesAsync();
        }
    }
}
