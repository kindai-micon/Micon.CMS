using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Micon.CMS.Library.Services;

namespace Micon.CMS.Library.Providers
{
    /// <summary>
    /// IFileProvider のラッパー
    /// Razor ファイル読み込み時に <style> タグを抽出し、CSS を分離
    /// </summary>
    public class CssExtractingFileProvider : IFileProvider
    {
        private readonly IFileProvider _innerProvider;
        private readonly CssExtractorService _cssExtractor;
        private readonly CssService? _cssService;
        private readonly ILogger<CssExtractingFileProvider> _logger;

        public CssExtractingFileProvider(
            IFileProvider innerProvider,
            CssExtractorService cssExtractor,
            CssService? cssService,
            ILogger<CssExtractingFileProvider> logger)
        {
            _innerProvider = innerProvider;
            _cssExtractor = cssExtractor;
            _cssService = cssService;
            _logger = logger;
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            var fileInfo = _innerProvider.GetFileInfo(subpath);

            // .cshtml ファイルのみ処理
            if (subpath.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase) &&
                subpath.Contains("Components", StringComparison.OrdinalIgnoreCase))
            {
                return new CssExtractingFileInfo(
                    fileInfo,
                    _cssExtractor,
                    _cssService,
                    subpath,
                    _logger);
            }

            return fileInfo;
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return _innerProvider.GetDirectoryContents(subpath);
        }

        public IChangeToken Watch(string filter)
        {
            return _innerProvider.Watch(filter);
        }
    }

    /// <summary>
    /// CSS 抽出を行うファイル情報ラッパー
    /// </summary>
    public class CssExtractingFileInfo : IFileInfo
    {
        private readonly IFileInfo _innerFileInfo;
        private readonly CssExtractorService _cssExtractor;
        private readonly CssService _cssService;
        private readonly string _subpath;
        private readonly ILogger _logger;
        private byte[]? _cachedContent;
        private string? _cachedCssClassName;

        public CssExtractingFileInfo(
            IFileInfo innerFileInfo,
            CssExtractorService cssExtractor,
            CssService cssService,
            string subpath,
            ILogger logger)
        {
            _innerFileInfo = innerFileInfo;
            _cssExtractor = cssExtractor;
            _cssService = cssService;
            _subpath = subpath;
            _logger = logger;
        }

        public bool Exists => _innerFileInfo.Exists;
        public long Length => _cachedContent?.Length ?? _innerFileInfo.Length;
        public string PhysicalPath => _innerFileInfo.PhysicalPath;
        public string Name => _innerFileInfo.Name;
        public DateTimeOffset LastModified => _innerFileInfo.LastModified;
        public bool IsDirectory => _innerFileInfo.IsDirectory;

        public Stream CreateReadStream()
        {
            using (var stream = _innerFileInfo.CreateReadStream())
            using (var reader = new StreamReader(stream))
            {
                var originalContent = reader.ReadToEnd();

                // コンポーネント名を抽出（例: Views/Shared/Components/C1/Default.cshtml → C1ViewComponent）
                // ※ ここではViewComponent クラス名が必要
                var componentName = ExtractComponentName(_subpath);

                // PackageId はDLL全体で共通（仮の値）
                var packageId = Guid.Parse("00000000-0000-0000-0000-000000000001");

                // CSS を抽出・スコープ化
                var (cleanedRazor, cssContent) = _cssExtractor.ExtractCss(
                    originalContent,
                    componentName,
                    packageId);

                _logger.LogInformation(
                    $"Razor '{_subpath}' processed with scoped CSS");

                // 修正済み Razor をメモリに保存
                _cachedContent = System.Text.Encoding.UTF8.GetBytes(cleanedRazor);
                return new MemoryStream(_cachedContent);
            }
        }

        /// <summary>
        /// ファイルパスからコンポーネント名を抽出
        /// 例: Views/Shared/Components/C1/Default.cshtml → C1
        /// </summary>
        private string ExtractComponentName(string subpath)
        {
            // Components/[ComponentName]/Default.cshtml の [ComponentName] を抽出
            var parts = subpath.Split('/', '\\');
            var componentsIndex = Array.IndexOf(parts, "Components");

            if (componentsIndex >= 0 && componentsIndex + 1 < parts.Length)
            {
                return parts[componentsIndex + 1];
            }

            return Path.GetFileNameWithoutExtension(subpath);
        }

        /// <summary>
        /// 抽出された CSS クラス名を取得
        /// </summary>
        public string? GetCssClassName()
        {
            return _cachedCssClassName;
        }
    }
}
