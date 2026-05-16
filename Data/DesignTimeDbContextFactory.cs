// Data/DesignTimeDbContextFactory.cs
using System;
using System.IO;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SecureTransparentDataExchange.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Define the environment (Development/Staging/Production)
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

            // Build the configuration: environment variables → appsettings.{ENV}.json → appsettings.json
            var configBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddEnvironmentVariables(prefix: null);

            // Pick up appsettings.{ENV}.json, if present
            var envSettings = $"appsettings.{environment}.json";
            if (File.Exists(envSettings))
                configBuilder.AddJsonFile(envSettings, optional: true, reloadOnChange: false);

            // And base appsettings.json (if any)
            if (File.Exists("appsettings.json"))
                configBuilder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);

            var config = configBuilder.Build();

            // 1) first try the environment variable ConnectionStrings__DefaultConnection
            var cs = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            optionsBuilder.UseSqlServer(cs, sql =>
            {
                sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                sql.CommandTimeout(60);
                // If the migrations are in a different project, uncomment them and specify the assembly:
                // sql.MigrationsAssembly("SecureTransparentDataExchange");
            });

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}