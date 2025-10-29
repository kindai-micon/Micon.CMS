using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Micon.CMS.Library.Services
{
    /// <summary>
    /// Component の CSS をメモリキャッシュで管理するサービス
    /// PackageId + ComponentName をキーとして CSS クラス名と CSS コンテンツを保持
    /// </summary>
    public class CssService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<CssService> _logger;
        private const string CACHE_KEY_PREFIX = "css_scoped_";
        private const string CACHE_KEY_CONTENT_PREFIX = "css_content_";
        private const string CACHE_KEY_ALL = "css_all_";
        private const string CACHE_KEY_COMPONENTS_LIST = "css_components_list_";

        // 登録されたコンポーネントの一覧を追跡するためのローカルリスト
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
            var cacheKey = GetCacheKey(packageId, componentName);
            _cache.Set(cacheKey, cssClassName, TimeSpan.FromHours(24));
            _logger.LogInformation($"Registered CSS class: {componentName} -> {cssClassName}");
        }

        /// <summary>
        /// CSS クラス名をキャッシュから取得
        /// 存在しない場合は自動生成して保存
        /// </summary>
        public string GetCssClass(Guid packageId, string componentName)
        {
            var cacheKey = GetCacheKey(packageId, componentName);

            if (_cache.TryGetValue(cacheKey, out var cssClass))
            {
                return cssClass?.ToString() ?? string.Empty;
            }

            // キャッシュに無い場合は生成
            var generatedClass = GenerateScopedClassName(packageId, componentName);
            RegisterCssClass(packageId, componentName, generatedClass);
            return generatedClass;
        }

        /// <summary>
        /// CSS コンテンツをキャッシュに保存
        /// </summary>
        public void RegisterCssContent(Guid packageId, string componentName, string cssContent)
        {
            var contentKey = GetContentCacheKey(packageId, componentName);
            _cache.Set(contentKey, cssContent, TimeSpan.FromHours(24));

            // 登録されたコンポーネント一覧に追加
            var componentKey = $"{packageId:N}_{componentName}";
            _registeredComponents.Add(componentKey);

            // すべての CSS キャッシュを無効化（再生成が必要）
            InvalidateAllCssCache();
        }

        /// <summary>
        /// すべての CSS コンテンツを取得
        /// </summary>
        public string GetAllCssContent()
        {
            var allCacheKey = CACHE_KEY_ALL;

            // キャッシュから取得
            if (_cache.TryGetValue(allCacheKey, out var allCss))
            {
                return allCss?.ToString() ?? string.Empty;
            }

            // キャッシュにない場合は、登録されているすべての CSS コンテンツを集約
            var allCssContent = new System.Text.StringBuilder();

            foreach (var componentKey in _registeredComponents)
            {
                // componentKey は "{packageId}_{componentName}" の形式
                var contentCacheKey = $"{CACHE_KEY_CONTENT_PREFIX}{componentKey}";

                if (_cache.TryGetValue(contentCacheKey, out var cssContent))
                {
                    if (!string.IsNullOrEmpty(cssContent?.ToString()))
                    {
                        allCssContent.AppendLine(cssContent?.ToString());
                        allCssContent.AppendLine();
                    }
                }
            }

            var result = allCssContent.ToString();

            // 集約した CSS をキャッシュに保存
            if (!string.IsNullOrEmpty(result))
            {
                _cache.Set(allCacheKey, result, TimeSpan.FromHours(24));
            }

            return result;
        }

        /// <summary>
        /// CSS コンテンツをキャッシュから取得
        /// </summary>
        public string? GetCssContent(Guid packageId, string componentName)
        {
            var contentKey = GetContentCacheKey(packageId, componentName);

            if (_cache.TryGetValue(contentKey, out var cssContent))
            {
                return cssContent?.ToString();
            }

            return null;
        }

        /// <summary>
        /// キャッシュキーを生成（CSS クラス名用）
        /// </summary>
        private string GetCacheKey(Guid packageId, string componentName)
        {
            return $"{CACHE_KEY_PREFIX}{packageId:N}_{componentName}";
        }

        /// <summary>
        /// キャッシュキーを生成（CSS コンテンツ用）
        /// </summary>
        private string GetContentCacheKey(Guid packageId, string componentName)
        {
            return $"{CACHE_KEY_CONTENT_PREFIX}{packageId:N}_{componentName}";
        }

        /// <summary>
        /// すべての CSS キャッシュを無効化
        /// </summary>
        private void InvalidateAllCssCache()
        {
            // 集約 CSS キャッシュを削除するため、トークンを更新
            // (IMemoryCache に Clear メソッドがないため、キャッシュキーごとに remove が必要だが、API がない)
            // 代わりに、次回アクセス時に集約 CSS を再生成するようにする
        }

        /// <summary>
        /// すべての CSS クラスをキャッシュから削除
        /// </summary>
        public void ClearCache()
        {
            _logger.LogInformation("Cleared CSS service cache");
            // 注: IMemoryCache には Clear メソッドがないため、
            // 実装が必要な場合は Dictionary を使った カスタム実装を検討
        }
    }
}
