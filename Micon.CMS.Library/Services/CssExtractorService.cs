using System.Text.RegularExpressions;

namespace Micon.CMS.Library.Services
{
    /// <summary>
    /// Razor テンプレートから <style> タグを抽出し、CSS を別ファイルとして分離するサービス
    /// CSS セレクタにスコーププレフィックスを自動追加
    /// </summary>
    public class CssExtractorService
    {
        private readonly ILogger<CssExtractorService> _logger;

        public CssExtractorService(ILogger<CssExtractorService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Razor コンテンツから <style> タグを抽出し、CSS をスコープ化
        /// </summary>
        /// <param name="razorContent">元の Razor テンプレートコンテンツ</param>
        /// <param name="componentName">ViewComponent クラス名（例: C1ViewComponent）</param>
        /// <param name="packageId">パッケージ ID</param>
        /// <returns>修正済み Razor コンテンツと抽出・スコープ化された CSS コンテンツ</returns>
        public (string cleanedRazor, string? cssContent) ExtractCss(
            string razorContent,
            string componentName,
            Guid packageId)
        {
            try
            {
                // <style>...</style> タグを正規表現で検出
                var stylePattern = @"<style\b[^>]*>([\s\S]*?)</style>";
                var match = Regex.Match(razorContent, stylePattern, RegexOptions.IgnoreCase);

                if (!match.Success)
                {
                    _logger.LogInformation($"No <style> tag found in component: {componentName}");
                    return (razorContent, null);
                }

                // CSS コンテンツを抽出
                var originalCss = match.Groups[1].Value.Trim();

                // CSS をスコープ化
                var scopedCss = ScopeCSS(originalCss, packageId, componentName);

                // Razor から <style> タグを削除
                var cleanedRazor = Regex.Replace(
                    razorContent,
                    stylePattern,
                    "",
                    RegexOptions.IgnoreCase);

                _logger.LogInformation($"Extracted CSS from component: {componentName}");

                return (cleanedRazor, scopedCss);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error extracting CSS from {componentName}: {ex.Message}");
                return (razorContent, null);
            }
        }

        /// <summary>
        /// CSS セレクタにプレフィックスを付けてスコープ化
        /// 例: .component { } → .{packageId}_{ComponentName}_component .component { }
        /// </summary>
        private string ScopeCSS(string css, Guid packageId, string componentName)
        {
            // スコーププレフィックスを生成
            // 形式: {packageId}_{ComponentName}
            var packageIdStr = packageId.ToString("N");
            var scopePrefix = $"{packageIdStr}_{componentName}";

            var lines = css.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var scopedLines = new List<string>();

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                // 開き括弧を含む行（セレクタ行）
                if (trimmedLine.Contains("{"))
                {
                    var selectorPart = trimmedLine.Substring(0, trimmedLine.IndexOf("{")).Trim();
                    var rest = trimmedLine.Substring(trimmedLine.IndexOf("{"));

                    // セレクタにプレフィックスを付ける
                    var scopedSelector = ScopifySelector(selectorPart, scopePrefix);
                    scopedLines.Add($"{scopedSelector} {rest}");
                }
                else
                {
                    scopedLines.Add(line);
                }
            }

            return string.Join("\n", scopedLines);
        }

        /// <summary>
        /// 単一のセレクタをスコープ化
        /// 例: .component → .{packageId}_{ComponentName}_component .component
        /// </summary>
        private string ScopifySelector(string selector, string scopePrefix)
        {
            // カンマで区切られたセレクタのリストに対応
            var selectors = selector.Split(',');
            var scopedSelectors = selectors.Select(s =>
            {
                var trimmed = s.Trim();

                // セレクタの前にスコーププレフィックスを付ける
                if (!trimmed.StartsWith($".{scopePrefix}"))
                {
                    return $".{scopePrefix} {trimmed}";
                }
                return trimmed;
            });

            return string.Join(", ", scopedSelectors);
        }
    }
}
