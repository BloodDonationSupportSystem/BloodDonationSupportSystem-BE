using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace BusinessObjects.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Get the current directory
            var basePath = Directory.GetCurrentDirectory();
            
            // Find the startup project directory
            var startupProjectPath = Path.GetFullPath(Path.Combine(basePath, "..", "BloodDonationSupportSystem"));
            
            Console.WriteLine($"Looking for appsettings.json in: {startupProjectPath}");
            
            // Build configuration from the startup project appsettings
            var configuration = new ConfigurationBuilder()
                .SetBasePath(startupProjectPath)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            // Get the connection string
            var connectionString = configuration.GetConnectionString("BDSS");
            
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException($"Connection string 'BDSS' not found in appsettings.json at path: {startupProjectPath}");
            }
            
            Console.WriteLine($"Connection string found: {connectionString}");
            
            // Create DbContext options
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}