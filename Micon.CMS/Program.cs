using ClassLibrary1.Components.Test;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System.Runtime.Loader;
namespace Micon.CMS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var current = Directory.GetCurrentDirectory();
            var cshtmlDir = Path.Combine(current, "Plugins", "Pages");
            PhysicalFileProvider physicalFileProvider = new PhysicalFileProvider(cshtmlDir);
            //var assembly = AssemblyLoadContext.GetAssemblyName()
            //EmbeddedFileProvider embeddedFileProvider = new EmbeddedFileProvider(,)
            // Add services to the container.
            var currentdir=Directory.GetCurrentDirectory();
            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(currentdir,"bin","Debug","net9.0","ClassLibrary1.dll"));
            
            builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation(options =>
            {
                options.FileProviders.Add(new EmbeddedFileProvider(typeof(TestViewComponent).Assembly));

            });
            //builder.Services.Configure<RazorViewEngineOptions>(options =>
            //{
            //    //options.ViewLocationExpanders.Add(new CustomViewLocationExpander());
            //});
            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddCors(options =>
                {
                    options.AddPolicy(
                        "AllowMy",
                        policy =>
                        {
                            policy.WithOrigins("http://localhost:5258", "https://localhost:7213", "http://localhost:65425");
                        });
                });
            }
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            var cssDir = Path.Combine(current, "Plugins", "Pages", "Themes");
            app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(cssDir),
                RequestPath = "/Themes"
            }
            );

            app.UseRouting();
            if (app.Environment.IsDevelopment())
            {
                app.UseCors("AllowMy");
                app.UseCors();
            }
            app.UseAuthorization();
            
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
