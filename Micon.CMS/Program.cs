using Micon.CMS.Library.Services;
using Micon.CMS.Library.Providers;
using Micon.CMS.Models;
using Micon.CMS.Repositories;
using Micon.CMS.Services;
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
            var options = new WebApplicationOptions
            {
                Args = args,
                ContentRootPath = AppContext.GetData("WEB_CONTENT_ROOT") as string
            };

            var builder = WebApplication.CreateBuilder(options);
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

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading plugin assembly: {dllFile}");
                    Console.WriteLine(ex.Message);
                }
            }
            
            foreach (var assembly in pluginAssemblies)
            {
                mvcBuilder.AddApplicationPart(assembly);
            }

            // CSS 抽出サービスを DI に登録
            builder.Services.AddScoped<CssExtractorService>();

            mvcBuilder.AddRazorRuntimeCompilation(options =>
            {
                // DI コンテナをビルドしてサービスを取得
                var serviceProvider = builder.Services.BuildServiceProvider();
                var cssExtractor = serviceProvider.GetRequiredService<CssExtractorService>();
                var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

                foreach (var assembly in pluginAssemblies)
                {
                    var embeddedProvider = new EmbeddedFileProvider(assembly);

                    // CssExtractingFileProvider でラップ
                    // ※ CssService は現在不要（メモリキャッシュは後で実装予定）
                    var cssExtractingProvider = new CssExtractingFileProvider(
                        embeddedProvider,
                        cssExtractor,
                        null, // CssService は null
                        loggerFactory.CreateLogger<CssExtractingFileProvider>());

                    options.FileProviders.Add(cssExtractingProvider);
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
            builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
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

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddScoped<IPageTemplateRepository,PageTemplateRepository>();
            builder.Services.AddScoped<IPageRepository, PageRepository>();
            builder.Services.AddScoped<IComponentRelationRepository, ComponentRelationRepository>();
            builder.Services.AddScoped<IComponentRepository, ComponentRepository>();
            builder.Services.AddScoped<IPageTemplateWorkspaceRepository, PageTemplateWorkspaceRepository>();
            builder.Services.AddScoped<ITestService, TestService>();
            builder.Services.AddScoped<ComponentSlotAnalyzerService>();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            var cssDir = Path.Combine(builder.Environment.ContentRootPath, "Plugins", "Pages", "Themes");
            app.UseStaticFiles();
            //app.UseStaticFiles(new StaticFileOptions()
            //{
            //    FileProvider = new PhysicalFileProvider(cssDir),
            //    RequestPath = "/Themes"
            //}
            //);

            app.UseRouting();
            if (app.Environment.IsDevelopment())
            {
                app.UseCors("AllowMy");
                app.UseCors();
            }
            app.UseAuthorization();


            app.MapControllerRoute(
                name: "default",
                pattern: "{tenant}/Setting/{controller=Home}/{action=Index}/{id?}");

            app.MapGet("/{tenant}", context =>
            {
                var tenant = context.Request.RouteValues["tenant"];
                context.Response.Redirect($"/{tenant}/Setting/Home/Index");
                return Task.CompletedTask;
            });
            app.MapControllerRoute(
                name: "page",
                pattern: "/{tenant}/{categoryId}/{pageId}",
                defaults: new { controller = "Page", action = "Index" });
            app.MapControllerRoute(
                name: "ctest",
                pattern: "/ComponentTest",
                defaults: new { controller = "Page", action = "Test" });
            app.MapGet("/", context =>
            {
                // Redirect to a default tenant or a tenant selection page.
                // For now, let's assume a default tenant "default"
                context.Response.Redirect("/default");
                return Task.CompletedTask;
            });

            using (var scope = app.Services.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;
                var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
                context.Database.Migrate();

                // デフォルトTenantの作成
                var defaultTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
                if (!context.Tenants.Any(t => t.Id == defaultTenantId))
                {
                    context.Tenants.Add(new Tenant
                    {
                        Id = defaultTenantId,
                        TenantName = "default",
                        IsAdmin = true
                    });
                    context.SaveChanges();
                }

                AssemblyService.AddAssemblies(pluginAssemblies);

                // プラグインからコンポーネントを検出して登録
                foreach (var assembly in pluginAssemblies)
                {
                    try
                    {
                        // MiconCmsSettingsからPackageIdを取得
                        var settingsType = assembly.GetTypes()
                            .FirstOrDefault(t => t.IsPublic && t.IsClass && t.Name == "MiconCmsSettings");

                        if (settingsType != null)
                        {
                            var packageIdField = settingsType.GetField("PackageId");
                            if (packageIdField != null && packageIdField.IsStatic)
                            {
                                var packageId = (Guid?)packageIdField.GetValue(null);
                                if (packageId.HasValue)
                                {
                                    // ViewComponentを探す
                                    var viewComponentTypes = assembly.GetTypes()
                                        .Where(t => t.IsPublic && t.IsClass && t.Name.EndsWith("ViewComponent"))
                                        .ToList();

                                    foreach (var componentType in viewComponentTypes)
                                    {
                                        var componentName = componentType.FullName ?? componentType.Name;

                                        // データベースに既に存在するか確認
                                        var existingComponent = context.Components
                                            .FirstOrDefault(c => c.PackageId == packageId.Value && c.Name == componentName);

                                        if (existingComponent == null)
                                        {
                                            // 新しいコンポーネントを登録
                                            context.Components.Add(new Component
                                            {
                                                PackageId = packageId.Value,
                                                Name = componentName
                                            });
                                            Console.WriteLine($"Registered component: {componentName} (PackageId: {packageId.Value})");
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error registering components from assembly: {assembly.FullName}");
                        Console.WriteLine(ex.Message);
                    }
                }

                context.SaveChanges();
            }

            
            app.Run();
        }
    }
}
