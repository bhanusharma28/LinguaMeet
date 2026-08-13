using LinguaMeet.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LinguaMeet.Api.Data;

public static class DbInitializer
{
    public static async Task SeedDevelopmentUsersAsync(ApplicationDbContext database)
    {
        var accounts = new[]
        {
            new
            {
                User = new User
                {
                    DisplayName = "Bharat Sharma",
                    Email = "bharat@linguameet.local",
                    PreferredLanguage = "en",
                },
                LegacyEmail = "aarav@linguameet.local",
            },
            new
            {
                User = new User
                {
                    DisplayName = "Aniket Pal",
                    Email = "aniket@linguameet.local",
                    PreferredLanguage = "hi",
                },
                LegacyEmail = "meera@linguameet.local",
            },
        };

        var passwordHasher = new PasswordHasher<User>();

        foreach (var accountDefinition in accounts)
        {
            var account = accountDefinition.User;
            var existingUser = await database.Users.FirstOrDefaultAsync(user =>
                user.Email == account.Email || user.Email == accountDefinition.LegacyEmail
            );
            if (existingUser != null)
            {
                existingUser.DisplayName = account.DisplayName;
                existingUser.Email = account.Email;
                existingUser.PreferredLanguage = account.PreferredLanguage;
                continue;
            }

            account.PasswordHash = passwordHasher.HashPassword(account, "Demo@123");
            database.Users.Add(account);
        }

        await database.SaveChangesAsync();
    }
}
