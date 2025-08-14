using Micon.CMS.Models;
using Micon.CMS.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System.Reflection;
using System.Runtime.Loader;

namespace Micon.CMS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var current = Directory.GetCurrentDirectory();

            // プラグインディレクトリの設定
            var pluginsDir = Path.Combine(current, "Plugins");
            if (!Directory.Exists(pluginsDir))
            {
                Directory.CreateDirectory(pluginsDir);
            }

            var mvcBuilder = builder.Services.AddControllersWithViews();
            
            // プラグインDLLの読み込みと登録
            var pluginAssemblies = new List<Assembly>();
            foreach (var dllFile in Directory.GetFiles(pluginsDir, "*.dll", SearchOption.AllDirectories))
            {
                try
                {
                    var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(dllFile);
                    pluginAssemblies.Add(assembly);
                    mvcBuilder.AddApplicationPart(assembly);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading plugin assembly: {dllFile}");
                    Console.WriteLine(ex.Message);
                }
            }

            mvcBuilder.AddRazorRuntimeCompilation(options =>
            {
                foreach (var assembly in pluginAssemblies)
                {
                    options.FileProviders.Add(new EmbeddedFileProvider(assembly));
                }
            });


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
            Console.WriteLine(builder.Configuration.GetConnectionString("micon-cms-db"));
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(builder.Configuration.GetConnectionString("micon-cms-db"));
            });
            builder.Services.AddAuthentication(o =>
            {
                o.DefaultScheme = IdentityConstants.ApplicationScheme;
                o.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddIdentityCookies(o => { });
            builder.Services.AddIdentityCore<ApplicationUser>(o =>
            {
                o.Stores.MaxLengthForKeys = 128;
                o.User.RequireUniqueEmail = false;
            })
                .AddDefaultTokenProviders()
                .AddRoles<ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            builder.Services.AddScoped<IPageTemplateRepository,PageTemplateRepository>();
            builder.Services.AddScoped<IPageRepository, PageRepository>();
            builder.Services.AddScoped<IComponentRelationRepository, ComponentRelationRepository>();

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
                name: "page",
                pattern: "{categoryId}/{pageId:guid}",
                defaults: new { controller = "Page", action = "Index" });

            app.MapControllerRoute(
                name: "default",
                pattern: "Setting/{controller=Home}/{action=Index}/{id?}");
            
            app.MapGet("/", context =>
            {
                context.Response.Redirect("/Home/Index");
                return Task.CompletedTask;
            });

            using(var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context.Database.Migrate();

            }

            
            app.Run();
        }
    }
}
