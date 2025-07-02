using BusinessObjects.Data;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BloodDonationSupportSystem.DataSeed
{
    public static class BloodCompatibilityDataSeed
    {
        public static async Task SeedBloodCompatibilityData(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                // Seed Blood Groups if they don't exist
                if (!await context.BloodGroups.AnyAsync())
                {
                    var bloodGroups = new[]
                    {
                        new BloodGroup { Id = Guid.NewGuid(), GroupName = "O-", Description = "O Negative - Universal Donor" },
                        new BloodGroup { Id = Guid.NewGuid(), GroupName = "O+", Description = "O Positive" },
                        new BloodGroup { Id = Guid.NewGuid(), GroupName = "A-", Description = "A Negative" },
                        new BloodGroup { Id = Guid.NewGuid(), GroupName = "A+", Description = "A Positive" },
                        new BloodGroup { Id = Guid.NewGuid(), GroupName = "B-", Description = "B Negative" },
                        new BloodGroup { Id = Guid.NewGuid(), GroupName = "B+", Description = "B Positive" },
                        new BloodGroup { Id = Guid.NewGuid(), GroupName = "AB-", Description = "AB Negative" },
                        new BloodGroup { Id = Guid.NewGuid(), GroupName = "AB+", Description = "AB Positive - Universal Recipient" }
                    };

                    await context.BloodGroups.AddRangeAsync(bloodGroups);
                    await context.SaveChangesAsync();
                    
                    logger.LogInformation("Blood groups seeded successfully");
                }

                // Seed Component Types if they don't exist
                if (!await context.ComponentTypes.AnyAsync())
                {
                    var componentTypes = new[]
                    {
                        new ComponentType { Id = Guid.NewGuid(), Name = "Whole Blood", ShelfLifeDays = 42 },
                        new ComponentType { Id = Guid.NewGuid(), Name = "Red Blood Cells", ShelfLifeDays = 42 },
                        new ComponentType { Id = Guid.NewGuid(), Name = "Plasma", ShelfLifeDays = 365 },
                        new ComponentType { Id = Guid.NewGuid(), Name = "Platelets", ShelfLifeDays = 5 },
                        new ComponentType { Id = Guid.NewGuid(), Name = "Cryoprecipitate", ShelfLifeDays = 365 },
                        new ComponentType { Id = Guid.NewGuid(), Name = "Fresh Frozen Plasma", ShelfLifeDays = 365 }
                    };

                    await context.ComponentTypes.AddRangeAsync(componentTypes);
                    await context.SaveChangesAsync();
                    
                    logger.LogInformation("Component types seeded successfully");
                }

                logger.LogInformation("Blood compatibility data seeding completed");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding blood compatibility data");
                throw;
            }
        }
    }
}