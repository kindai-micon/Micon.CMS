using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace Micon.CMS.Library.Services
{
    /// <summary>
    /// Razor テンプレートから <style> タグを抽出し、CSS を別ファイルとして分離するサービス
    /// 各セレクタユニットの末尾にスコープクラスを自動追加
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
        /// <param name="scopeClassName">スコープクラス名（既に完全形：{packageIdHex}_{folderName}{fileName}）</param>
        /// <param name="packageId">パッケージ ID（互換性のため保持）</param>
        /// <returns>修正済み Razor コンテンツと抽出・スコープ化された CSS コンテンツ</returns>
        public (string cleanedRazor, string? cssContent) ExtractCss(
            string razorContent,
            string scopeClassName,
            Guid packageId)
        {
            try
            {
                // <style>...</style> タグを正規表現で検出
                var stylePattern = @"<style\b[^>]*>([\s\S]*?)</style>";
                var match = Regex.Match(razorContent, stylePattern, RegexOptions.IgnoreCase);

                if (!match.Success)
                {
                    _logger.LogInformation($"No <style> tag found in component: {scopeClassName}");
                    return (razorContent, null);
                }

                // CSS コンテンツを抽出
                var originalCss = match.Groups[1].Value.Trim();

                // CSS をスコープ化（scopeClassName は既に完全形）
                var scopedCss = ScopeCSS(originalCss, scopeClassName);

                // Razor から <style> タグを削除
                var cleanedRazor = Regex.Replace(
                    razorContent,
                    stylePattern,
                    "",
                    RegexOptions.IgnoreCase);

                _logger.LogInformation($"Extracted CSS from component: {scopeClassName}");

                return (cleanedRazor, scopedCss);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error extracting CSS from {scopeClassName}: {ex.Message}");
                return (razorContent, null);
            }
        }

        /// <summary>
        /// CSS のセレクタを スコープ化
        /// 各セレクタユニットの末尾に .{scopeClassName} を追加
        ///
        /// 例:
        /// .component { } → .component.{scopeClass} { }
        /// .component h1 { } → .component.{scopeClass} h1.{scopeClass} { }
        /// h1 { } → h1.{scopeClass} { }
        /// .component.active { } → .component.active.{scopeClass} { }
        /// </summary>
        private string ScopeCSS(string css, string scopeClassName)
        {
            var scopeClassSelector = $".{scopeClassName}";

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

                    // セレクタをスコープ化
                    var scopedSelector = ScopifySelector(selectorPart, scopeClassSelector);
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
        /// セレクタをスコープ化
        /// カンマで区切られた複数セレクタに対応
        /// コンビネータ（スペース、>、+、~）で分割し、各ユニットの末尾にスコープクラスを追加
        ///
        /// 例:
        /// .component → .component.{scopeClass}
        /// .component.active → .component.active.{scopeClass}
        /// .component h1 → .component.{scopeClass} h1.{scopeClass}
        /// .component > .child → .component.{scopeClass} > .child.{scopeClass}
        /// h1 → h1.{scopeClass}
        /// .title[red-color] → .title[red-color].{scopeClass}
        /// </summary>
        private string ScopifySelector(string selector, string scopeClassSelector)
        {
            // カンマで区切られたセレクタのリストに対応
            var selectors = selector.Split(',');
            var scopedSelectors = selectors.Select(s =>
            {
                var trimmed = s.Trim();
                return ScopifySingleSelector(trimmed, scopeClassSelector);
            });

            return string.Join(", ", scopedSelectors);
        }

        /// <summary>
        /// 単一のセレクタをスコープ化
        /// コンビネータ（スペース、>、+、~）で分割し、各ユニットの末尾にスコープクラスを追加
        /// </summary>
        private string ScopifySingleSelector(string selector, string scopeClassSelector)
        {
            // コンビネータ（スペース、>、+、~）で分割（コンビネータも保持）
            // \s+ は複数スペースに対応
            var parts = Regex.Split(selector, @"(\s+|[>+~])");

            var result = new List<string>();

            foreach (var part in parts)
            {
                // 完全に空の部分はスキップ
                if (part.Length == 0)
                {
                    continue;
                }

                // コンビネータ（スペース、>、+、~）かどうか判定
                if (Regex.IsMatch(part, @"^(\s+|[>+~]+)$"))
                {
                    // コンビネータはそのまま追加（スペースを保持）
                    result.Add(part);
                }
                else
                {
                    // セレクタユニット - 末尾にスコープクラスを追加
                    var trimmedPart = part.Trim();
                    result.Add(trimmedPart + scopeClassSelector);
                }
            }

            return string.Join("", result);
        }
    }
}
