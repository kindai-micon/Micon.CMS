using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Micon.CMS.Library.Services
{
    /// <summary>
    /// Component の CSS をメモリキャッシュで管理するサービス
    /// PackageId + ComponentName をキーとして CSS クラス名を保持
    /// </summary>
    public class CssService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<CssService> _logger;
        private const string CACHE_KEY_PREFIX = "css_scoped_";

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
            return $"pkg-{packageId:N}-cmp-{GetComponentNameHash(componentName)}";
        }

        /// <summary>
        /// ComponentName をハッシュ化（長さ制限のため）
        /// </summary>
        private string GetComponentNameHash(string componentName)
        {
            // シンプルなハッシュ値を生成（より短くするため）
            return componentName.GetHashCode().ToString("X").ToLower();
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
        /// キャッシュキーを生成
        /// </summary>
        private string GetCacheKey(Guid packageId, string componentName)
        {
            return $"{CACHE_KEY_PREFIX}{packageId:N}_{componentName}";
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
