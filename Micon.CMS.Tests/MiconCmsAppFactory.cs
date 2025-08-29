using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Micon.CMS;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Micon.CMS.Tests
{
    public class MiconCmsAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private DistributedApplication? _appHost;
        private string? _postgresConnectionString;

        protected override IHost CreateHost(IHostBuilder builder)
        {
            var solutionDir = FindSolutionDirectory();
            var contentRoot = Path.Combine(solutionDir, "Micon.CMS");
            AppContext.SetData("WEB_CONTENT_ROOT", contentRoot);
            return base.CreateHost(builder);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var dbContextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (dbContextDescriptor != null)
                {
                    services.Remove(dbContextDescriptor);
                }

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseNpgsql(_postgresConnectionString);
                });
            });
        }

        public async Task InitializeAsync()
        {
            var appHostFixture = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Micon_CMS_AppHost>();
            _appHost = await appHostFixture.BuildAsync();
            await _appHost.StartAsync();
            _postgresConnectionString = await _appHost.GetConnectionStringAsync("postgres");
        }

        public new async Task DisposeAsync()
        {
            if (_appHost != null)
            {
                await _appHost.StopAsync();
                await _appHost.DisposeAsync();
            }
            await base.DisposeAsync();
        }
        
        private static string FindSolutionDirectory()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null && !directory.GetFiles("*.sln").Any())
            {
                directory = directory.Parent;
            }
            return directory?.FullName ?? throw new Exception("Solution directory not found.");
        }
    }
}
