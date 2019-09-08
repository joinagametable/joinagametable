using System;
using JoinAGameTable.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JoinAGameTable
{
    /// <summary>
    /// Application main class.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Entry point.
        /// </summary>
        /// <param name="args">Supplied command-line arguments</param>
        public static void Main(string[] args)
        {
            var webHost = CreateWebHostBuilder(args).Build();

            using (var scope = webHost.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();

                try
                {
                    using (var appDbContext = services.GetRequiredService<AppDbContext>())
                    {
                        appDbContext.Database.Migrate();
                        appDbContext.Database.EnsureCreated();
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Can't migrate database");
                    Environment.Exit(1);
                }
            }

            webHost.Run();
        }

        /// <summary>
        /// Create web host builder.
        /// </summary>
        /// <param name="args">Supplied command-line arguments</param>
        /// <returns>Created web host builder</returns>
        private static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseLibuv()
                .ConfigureLogging((hostingContext, loggingBuilder) =>
                {
                    loggingBuilder.ClearProviders();
                    loggingBuilder.AddLog4Net();
                })
                .UseStartup<Startup>();
    }
}
