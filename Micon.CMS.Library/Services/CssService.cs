using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Micon.CMS.Library.Services
{
    /// <summary>
    /// Component の CSS を IMemoryCache で管理するサービス
    /// PackageId + ComponentName をキーとして CSS コンテンツを保持
    /// Singleton で共有され、確実にデータが保持される
    /// </summary>
    public class CssService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<CssService> _logger;

        // 登録されたコンポーネントの一覧を追跡
        private readonly HashSet<string> _registeredComponents = new();

        public CssService(IMemoryCache cache, ILogger<CssService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// CSS スコープ用のクラス名を生成（PackageId + ComponentName から）
        /// </summary>
        public string GenerateScopedClassName(Guid packageId, string componentName)
        {
            return $"{packageId:N}_{componentName}";
        }

        /// <summary>
        /// CSS クラス名をキャッシュに保存
        /// </summary>
        public void RegisterCssClass(Guid packageId, string componentName, string cssClassName)
        {
            var classKey = GetClassKey(packageId, componentName);
            _cache.Set(classKey, cssClassName, TimeSpan.FromHours(24));
            _logger.LogInformation($"Registered CSS class: {componentName} -> {cssClassName}");
        }

        /// <summary>
        /// CSS クラス名をキャッシュから取得
        /// 存在しない場合は自動生成して保存
        /// </summary>
        public string GetCssClass(Guid packageId, string componentName)
        {
            var classKey = GetClassKey(packageId, componentName);

            if (_cache.TryGetValue(classKey, out var cssClass))
            {
                return cssClass?.ToString() ?? string.Empty;
            }

            // キャッシュに無い場合は生成
            var generatedClass = GenerateScopedClassName(packageId, componentName);
            RegisterCssClass(packageId, componentName, generatedClass);
            return generatedClass;
        }

        /// <summary>
        /// CSS コンテンツを保存
        /// </summary>
        public void RegisterCssContent(Guid packageId, string componentName, string cssContent)
        {
            var contentKey = GetContentKey(packageId, componentName);
            _cache.Set(contentKey, cssContent, TimeSpan.FromHours(24));

            // 登録されたコンポーネント一覧に追加
            _registeredComponents.Add(contentKey);

            _logger.LogInformation($"Registered CSS content for {componentName}");
        }

        /// <summary>
        /// すべての CSS コンテンツを取得
        /// </summary>
        public string GetAllCssContent()
        {
            if (_registeredComponents.Count == 0)
            {
                return string.Empty;
            }

            var allCssContent = new System.Text.StringBuilder();

            foreach (var key in _registeredComponents)
            {
                if (_cache.TryGetValue(key, out var cssContent))
                {
                    if (!string.IsNullOrEmpty(cssContent?.ToString()))
                    {
                        allCssContent.AppendLine(cssContent?.ToString());
                        allCssContent.AppendLine();
                    }
                }
            }

            return allCssContent.ToString();
        }

        /// <summary>
        /// CSS コンテンツを取得
        /// </summary>
        public string? GetCssContent(Guid packageId, string componentName)
        {
            var contentKey = GetContentKey(packageId, componentName);

            if (_cache.TryGetValue(contentKey, out var cssContent))
            {
                return cssContent?.ToString();
            }

            return null;
        }

        /// <summary>
        /// キャッシュキーを生成（CSS クラス名用）
        /// </summary>
        private string GetClassKey(Guid packageId, string componentName)
        {
            return $"class_{packageId:N}_{componentName}";
        }

        /// <summary>
        /// キャッシュキーを生成（CSS コンテンツ用）
        /// </summary>
        private string GetContentKey(Guid packageId, string componentName)
        {
            return $"content_{packageId:N}_{componentName}";
        }

        /// <summary>
        /// すべてのキャッシュをクリア
        /// </summary>
        public void ClearCache()
        {
            _registeredComponents.Clear();
            _logger.LogInformation("Cleared CSS service cache");
        }

        /// <summary>
        /// 登録されているコンポーネント数を取得（デバッグ用）
        /// </summary>
        public int GetRegisteredComponentCount()
        {
            return _registeredComponents.Count;
        }
    }
}
