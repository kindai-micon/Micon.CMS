using Micon.CMS.Models;
using Micon.CMS.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Micon.CMS.Services
{
    /// <summary>
    /// コンポーネント情報をキャッシュするサービス
    /// プラグインから検出されたコンポーネントを保持
    /// </summary>
    public interface IComponentCacheService
    {
        /// <summary>
        /// キャッシュされたコンポーネント一覧を取得
        /// </summary>
        Task<List<Component>> GetCachedComponentsAsync();

        /// <summary>
        /// プラグインアセンブリからコンポーネント情報を読み込んでキャッシュに保存
        /// </summary>
        Task LoadComponentsFromAssembliesAsync(List<Assembly> assemblies);

        /// <summary>
        /// キャッシュを更新（プラグインDLLをスキャン）
        /// </summary>
        Task RefreshCacheAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken);

        /// <summary>
        /// キャッシュをクリア
        /// </summary>
        void ClearCache();
    }

    public class ComponentCacheService : IComponentCacheService
    {
        private List<Component>? _cachedComponents;
        private DateTime _lastRefreshTime;
        private readonly object _lockObject = new object();

        public ComponentCacheService()
        {
            _lastRefreshTime = DateTime.MinValue;
        }

        public async Task<List<Component>> GetCachedComponentsAsync()
        {
            lock (_lockObject)
            {
                // キャッシュが存在する場合はそれを返す
                if (_cachedComponents != null)
                {
                    Console.WriteLine($"[ComponentCache] Returning {_cachedComponents.Count} components from cache");
                    return _cachedComponents;
                }

                // キャッシュがない場合は空リストを返す（RefreshCacheAsyncで初期化されることを想定）
                Console.WriteLine("[ComponentCache] Cache is empty");
                return new List<Component>();
            }
        }

        public async Task RefreshCacheAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            Console.WriteLine("[ComponentCache] Starting cache refresh...");
            using (var scope = serviceProvider.CreateScope())
            {
                var componentRepository = scope.ServiceProvider.GetRequiredService<IComponentRepository>();

                lock (_lockObject)
                {
                    _cachedComponents = null;
                    _lastRefreshTime = DateTime.UtcNow;
                }

                // キャッシュを再読み込み
                var components = await componentRepository.GetAllAsync(cancellationToken);
                Console.WriteLine($"[ComponentCache] Retrieved {components.Count} components from database");

                lock (_lockObject)
                {
                    _cachedComponents = components;
                }

                Console.WriteLine($"[ComponentCache] Saved {_cachedComponents.Count} components to cache");
            }
        }

        public async Task LoadComponentsFromAssembliesAsync(List<Assembly> assemblies)
        {
            Console.WriteLine("[ComponentCache] Loading components from assemblies...");
            var componentsFromDlls = new List<Component>();

            foreach (var assembly in assemblies)
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
                                    var component = new Component
                                    {
                                        Id = Guid.NewGuid(),
                                        PackageId = packageId.Value,
                                        Name = componentName
                                    };
                                    componentsFromDlls.Add(component);
                                    Console.WriteLine($"[ComponentCache] Found component: {componentName}");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ComponentCache] Error scanning assembly: {assembly.FullName}");
                    Console.WriteLine($"[ComponentCache] {ex.Message}");
                }
            }

            Console.WriteLine($"[ComponentCache] Setting {componentsFromDlls.Count} components to cache");
            lock (_lockObject)
            {
                _cachedComponents = componentsFromDlls;
            }
        }

        public void ClearCache()
        {
            lock (_lockObject)
            {
                _cachedComponents = null;
                _lastRefreshTime = DateTime.MinValue;
            }
        }
    }
}
