using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SecureTransparentDataExchange.Models;
using SecureTransparentDataExchange.Models.Enums;
using SecureTransparentDataExchange.Models.Location;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SecureTransparentDataExchange.Models.identity;

namespace SecureTransparentDataExchange.Data
{
    public static class SeedData
    {
        private sealed record LocationSeed(
            string Country,
            string City,
            string PostalCode,
            string Street);

        public static async Task InitializeAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var users = scope.ServiceProvider.GetRequiredService<UserManager<Login>>();
            var roles = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            await db.Database.MigrateAsync();

            await SeedRolesAsync(roles, logger);
            await SeedAdminAsync(db, users, roles, logger);
            await SeedAgreementAsync(db, logger);
            await SeedLocationsAsync(db, logger);
        }

        // =====================================================
        // ROLES
        // =====================================================
        private static async Task SeedRolesAsync(
            RoleManager<Role> roles,
            ILogger logger)
        {
            var roleNames = new[] { "Admin", "User", "Employee" };

            foreach (var name in roleNames)
            {
                if (!await roles.RoleExistsAsync(name))
                {
                    await roles.CreateAsync(new Role
                    {
                        Name = name,
                        NormalizedName = name.ToUpperInvariant(),
                        Description = $"Default role {name}"
                    });

                    logger.LogInformation("Role created: {Role}", name);
                }
            }
        }

        // =====================================================
        // ADMIN USER (DEV ONLY)
        // =====================================================
        private static async Task SeedAdminAsync(
            ApplicationDbContext db,
            UserManager<Login> users,
            RoleManager<Role> roles,
            ILogger logger)
        {
            const string adminEmail = "admin@example.com";
            const string adminPassword = "Admin@123"; // ⚠ DEV ONLY

            var admin = await users.FindByEmailAsync(adminEmail);

            if (admin == null)
            {
                var adminRole = await roles.FindByNameAsync("Admin");

                admin = new Login
                {
                    Email = adminEmail,
                    UserName = adminEmail,
                    PhoneNumber = "00000000000",
                    Name = "Admin",
                    LastName = "User",
                    AgreeToTerms = true,
                    UserType = UserType.Individual,
                    EmailConfirmed = true,
                    IsEmailConfirmed = true,
                    RoleId = adminRole!.Id
                };

                await users.CreateAsync(admin, adminPassword);
                await users.AddToRoleAsync(admin, "Admin");

                logger.LogInformation("Admin user seeded");
            }
            else if (!admin.EmailConfirmed)
            {
                admin.EmailConfirmed = true;
                await db.SaveChangesAsync();
            }
        }

        // =====================================================
        // LOGIN AGREEMENT
        // =====================================================
        private static async Task SeedAgreementAsync(
            ApplicationDbContext db,
            ILogger logger)
        {
            if (await db.LoginAgreements.AnyAsync())
                return;

            db.LoginAgreements.Add(new LoginAgreement
            {
                Title = "GDPR + AI Policy Agreement",
                Version = "1.0",
                IsLatest = true,
                CreatedAt = DateTime.UtcNow,
                Content =
                    "Based on GDPR Articles 5,6,7,9,12,14,17,22 with AI usage transparency."
            });

            await db.SaveChangesAsync();
            logger.LogInformation("Login Agreement seeded");
        }

        // =====================================================
        // LOCATIONS (COUNTRY / CITY / POSTAL / ADDRESS)
        // =====================================================
        private static async Task SeedLocationsAsync(
            ApplicationDbContext db,
            ILogger logger)
        {
            var data = new List<LocationSeed>
            {
                new ("USA", "New York", "10001", "5th Avenue"),
                new ("USA", "Los Angeles", "90001", "Sunset Boulevard"),
                new ("Canada", "Toronto", "M5H 2N2", "King Street"),
                new ("Germany", "Berlin", "10115", "Friedrichstraße"),
                new ("Latvia", "Riga", "LV-1010", "Brīvības iela"),
                new ("Lithuania", "Vilnius", "LT-01100", "Gedimino prospektas"),
                new ("France", "Paris", "75001", "Rue de Rivoli"),
                new ("United Kingdom", "London", "SW1A 1AA", "The Mall")
            };

            // -------- COUNTRIES --------
            var existingCountries = await db.Countries
                .Select(c => c.Name)
                .ToListAsync();

            var newCountries = data
                .Select(d => d.Country)
                .Distinct()
                .Where(c => !existingCountries.Contains(c))
                .Select(c => new Country { Name = c });

            db.Countries.AddRange(newCountries);
            await db.SaveChangesAsync();

            var countries = await db.Countries.ToDictionaryAsync(c => c.Name);

            // -------- CITIES --------
            var citiesToAdd = new List<City>();

            foreach (var g in data.GroupBy(x => x.City))
            {
                var first = g.First();
                var countryId = countries[first.Country].Id;

                if (!await db.Cities.AnyAsync(c =>
                        c.Name == first.City && c.CountryId == countryId))
                {
                    citiesToAdd.Add(new City
                    {
                        Name = first.City,
                        CountryId = countryId
                    });
                }
            }

            db.Cities.AddRange(citiesToAdd);
            await db.SaveChangesAsync();

            var cities = await db.Cities.ToDictionaryAsync(c => c.Name);

            // -------- POSTAL CODES --------
            var postalCodesToAdd = new List<PostalCode>();

            foreach (var item in data)
            {
                var cityId = cities[item.City].Id;

                if (!await db.PostalCodes.AnyAsync(p =>
                        p.Code == item.PostalCode && p.CityId == cityId))
                {
                    postalCodesToAdd.Add(new PostalCode
                    {
                        Code = item.PostalCode,
                        CityId = cityId
                    });
                }
            }

            db.PostalCodes.AddRange(postalCodesToAdd);
            await db.SaveChangesAsync();

            var postals = await db.PostalCodes
                .ToDictionaryAsync(p => (p.CityId, p.Code));

            // -------- ADDRESSES --------
            var addressesToAdd = new List<Address>();

            foreach (var item in data)
            {
                var cityId = cities[item.City].Id;
                var postalId = postals[(cityId, item.PostalCode)].Id;
                var countryId = countries[item.Country].Id;

                if (!await db.Addresses.AnyAsync(a =>
                a.Street == item.Street &&
                a.PostalCodeId == postalId))

                    addressesToAdd.Add(new Address
                    {
                        Street = item.Street,
                        PostalCodeId = postalId
                    });

                db.Addresses.AddRange(addressesToAdd);
                await db.SaveChangesAsync();

                logger.LogInformation("Global locations seeded successfully.");
            }
        }
    }
}
