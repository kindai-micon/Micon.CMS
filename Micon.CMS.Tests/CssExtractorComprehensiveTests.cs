using Micon.CMS.Services;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Micon.CMS.Tests
{
    /// <summary>
    /// CssExtractorService が幅広いセレクタと複雑なセレクタに対応しているかの包括的テスト
    /// </summary>
    public class CssExtractorComprehensiveTests
    {
        private readonly CssExtractorService _cssExtractor;

        public CssExtractorComprehensiveTests()
        {
            var loggerFactory = new TestLoggerFactory();
            var logger = loggerFactory.CreateLogger<CssExtractorService>();
            _cssExtractor = new CssExtractorService(logger);
        }

        private string CreateRazorContent(string cssRule) => $"<style>{cssRule}</style>";

        private (string cleanedRazor, string? scopedCss) ExtractAndScope(string cssRule)
        {
            var packageId = System.Guid.Parse("12345678-1234-1234-1234-123456789012");
            var scopeClassName = "scope";
            var razorContent = CreateRazorContent(cssRule);
            return _cssExtractor.ExtractCss(razorContent, scopeClassName, packageId);
        }

        #region 基本的なセレクタ

        [Fact]
        public void BasicSelector_ElementSelector()
        {
            var (_, scopedCss) = ExtractAndScope("p { color: red; }");
            Assert.Contains("p.scope", scopedCss);
        }

        [Fact]
        public void BasicSelector_ClassSelector()
        {
            var (_, scopedCss) = ExtractAndScope(".component { color: red; }");
            Assert.Contains(".component.scope", scopedCss);
        }

        [Fact]
        public void BasicSelector_IdSelector()
        {
            var (_, scopedCss) = ExtractAndScope("#header { color: red; }");
            Assert.Contains("#header.scope", scopedCss);
        }

        [Fact]
        public void BasicSelector_UniversalSelector()
        {
            var (_, scopedCss) = ExtractAndScope("* { margin: 0; }");
            Assert.Contains("*.scope", scopedCss);
        }

        #endregion

        #region 属性セレクタ

        [Fact]
        public void AttributeSelector_ExactMatch()
        {
            var (_, scopedCss) = ExtractAndScope("input[type=\"text\"] { border: 1px solid; }");
            Assert.Contains("input[type=\"text\"].scope", scopedCss);
        }

        [Fact]
        public void AttributeSelector_PrefixMatch()
        {
            var (_, scopedCss) = ExtractAndScope("p[class^=\"ttl\"] { font-weight: bold; }");
            Assert.Contains("p[class^=\"ttl\"].scope", scopedCss);
        }

        [Fact]
        public void AttributeSelector_SuffixMatch()
        {
            var (_, scopedCss) = ExtractAndScope("img[src$=\".png\"] { display: block; }");
            Assert.Contains("img[src$=\".png\"].scope", scopedCss);
        }

        [Fact]
        public void AttributeSelector_SubstringMatch()
        {
            var (_, scopedCss) = ExtractAndScope("a[href*=\"example\"] { color: blue; }");
            Assert.Contains("a[href*=\"example\"].scope", scopedCss);
        }

        [Fact]
        public void AttributeSelector_MultipleAttributes()
        {
            var (_, scopedCss) = ExtractAndScope("input[type=\"text\"][required] { border-color: red; }");
            Assert.Contains("input[type=\"text\"][required].scope", scopedCss);
        }

        #endregion

        #region 疑似クラス

        [Fact]
        public void PseudoClass_FirstChild()
        {
            var (_, scopedCss) = ExtractAndScope("li:first-child { font-weight: bold; }");
            Assert.Contains("li:first-child.scope", scopedCss);
        }

        [Fact]
        public void PseudoClass_LastChild()
        {
            var (_, scopedCss) = ExtractAndScope("li:last-child { border-bottom: none; }");
            Assert.Contains("li:last-child.scope", scopedCss);
        }

        [Fact]
        public void PseudoClass_NthChild()
        {
            var (_, scopedCss) = ExtractAndScope("tr:nth-child(odd) { background: #f0f0f0; }");
            Assert.Contains("tr:nth-child(odd).scope", scopedCss);
        }

        [Fact]
        public void PseudoClass_NthOfType()
        {
            var (_, scopedCss) = ExtractAndScope("p:nth-of-type(2) { margin-top: 20px; }");
            Assert.Contains("p:nth-of-type(2).scope", scopedCss);
        }

        [Fact]
        public void PseudoClass_FirstOfType()
        {
            var (_, scopedCss) = ExtractAndScope("h1:first-of-type { color: navy; }");
            Assert.Contains("h1:first-of-type.scope", scopedCss);
        }

        [Fact]
        public void PseudoClass_Hover()
        {
            var (_, scopedCss) = ExtractAndScope("a:hover { text-decoration: underline; }");
            Assert.Contains("a:hover.scope", scopedCss);
        }

        [Fact]
        public void PseudoClass_Focus()
        {
            var (_, scopedCss) = ExtractAndScope("input:focus { outline: 2px solid blue; }");
            Assert.Contains("input:focus.scope", scopedCss);
        }

        [Fact]
        public void PseudoClass_Active()
        {
            var (_, scopedCss) = ExtractAndScope("button:active { transform: scale(0.98); }");
            Assert.Contains("button:active.scope", scopedCss);
        }

        [Fact]
        public void PseudoClass_Visited()
        {
            var (_, scopedCss) = ExtractAndScope("a:visited { color: purple; }");
            Assert.Contains("a:visited.scope", scopedCss);
        }

        [Fact]
        public void PseudoClass_Disabled()
        {
            var (_, scopedCss) = ExtractAndScope("button:disabled { opacity: 0.5; }");
            Assert.Contains("button:disabled.scope", scopedCss);
        }

        [Fact]
        public void PseudoClass_Enabled()
        {
            var (_, scopedCss) = ExtractAndScope("input:enabled { cursor: pointer; }");
            Assert.Contains("input:enabled.scope", scopedCss);
        }

        [Fact]
        public void PseudoClass_Not()
        {
            var (_, scopedCss) = ExtractAndScope("p:not(.exception) { color: black; }");
            Assert.Contains("p:not(.exception).scope", scopedCss);
        }

        #endregion

        #region コンビネータ（結合子）

        [Fact]
        public void Combinator_DescendantCombinator()
        {
            var (_, scopedCss) = ExtractAndScope(".parent span { color: blue; }");
            Assert.Contains(".parent.scope span.scope", scopedCss);
        }

        [Fact]
        public void Combinator_ChildCombinator()
        {
            var (_, scopedCss) = ExtractAndScope(".parent > .child { margin-left: 20px; }");
            Assert.Contains(".parent.scope > .child.scope", scopedCss);
        }

        [Fact]
        public void Combinator_AdjacentSibling()
        {
            var (_, scopedCss) = ExtractAndScope("h1 + p { margin-top: 0; }");
            Assert.Contains("h1.scope + p.scope", scopedCss);
        }

        [Fact]
        public void Combinator_GeneralSibling()
        {
            var (_, scopedCss) = ExtractAndScope("h1 ~ p { color: gray; }");
            Assert.Contains("h1.scope ~ p.scope", scopedCss);
        }

        #endregion

        #region 複合セレクタ

        [Fact]
        public void ComplexSelector_ElementWithClass()
        {
            var (_, scopedCss) = ExtractAndScope("div.container { padding: 10px; }");
            Assert.Contains("div.container.scope", scopedCss);
        }

        [Fact]
        public void ComplexSelector_ElementWithMultipleClasses()
        {
            var (_, scopedCss) = ExtractAndScope("div.container.active { display: block; }");
            Assert.Contains("div.container.active.scope", scopedCss);
        }

        [Fact]
        public void ComplexSelector_ElementWithId()
        {
            var (_, scopedCss) = ExtractAndScope("div#main { width: 100%; }");
            Assert.Contains("div#main.scope", scopedCss);
        }

        [Fact]
        public void ComplexSelector_ElementWithAttribute()
        {
            var (_, scopedCss) = ExtractAndScope("input[type=\"email\"] { width: 200px; }");
            Assert.Contains("input[type=\"email\"].scope", scopedCss);
        }

        [Fact]
        public void ComplexSelector_MultipleSelectorsComma()
        {
            var (_, scopedCss) = ExtractAndScope("h1, h2, h3 { color: navy; }");
            Assert.Contains("h1.scope", scopedCss);
            Assert.Contains("h2.scope", scopedCss);
            Assert.Contains("h3.scope", scopedCss);
        }

        [Fact]
        public void ComplexSelector_ClassAndPseudoClass()
        {
            var (_, scopedCss) = ExtractAndScope(".button:hover { background-color: blue; }");
            Assert.Contains(".button:hover.scope", scopedCss);
        }

        [Fact]
        public void ComplexSelector_DeepNesting()
        {
            var (_, scopedCss) = ExtractAndScope(".container > .row > .col { flex: 1; }");
            Assert.Contains(".container.scope > .row.scope > .col.scope", scopedCss);
        }

        [Fact]
        public void ComplexSelector_MixedCombinators()
        {
            var (_, scopedCss) = ExtractAndScope(".parent div.child > span { color: red; }");
            Assert.Contains(".parent.scope div.child.scope > span.scope", scopedCss);
        }

        [Fact]
        public void ComplexSelector_AttributeWithPseudoClass()
        {
            var (_, scopedCss) = ExtractAndScope("input[type=\"text\"]:focus { border-color: blue; }");
            Assert.Contains("input[type=\"text\"]:focus.scope", scopedCss);
        }

        #endregion

        #region 複雑な実世界のケース

        [Fact]
        public void RealWorld_BootstrapLikeGrid()
        {
            var css = @"
                .grid {
                    display: grid;
                }
                .grid > .col {
                    flex: 1;
                }
                .grid > .col.wide {
                    flex: 2;
                }
            ";
            var (_, scopedCss) = ExtractAndScope(css);
            Assert.Contains(".grid.scope", scopedCss);
            Assert.Contains(".grid.scope > .col.scope", scopedCss);
            Assert.Contains(".grid.scope > .col.wide.scope", scopedCss);
        }

        [Fact]
        public void RealWorld_FormStyling()
        {
            var css = @"
                .form-group { margin-bottom: 15px; }
                .form-group > label { font-weight: bold; }
                .form-group input { width: 100%; }
                .form-group input:focus { border-color: blue; }
            ";
            var (_, scopedCss) = ExtractAndScope(css);
            Assert.Contains(".form-group.scope", scopedCss);
            Assert.Contains(".form-group.scope > label.scope", scopedCss);
            Assert.Contains(".form-group.scope input.scope", scopedCss);
            Assert.Contains(".form-group.scope input:focus.scope", scopedCss);
        }

        [Fact]
        public void RealWorld_NavigationMenu()
        {
            var css = @"
                nav.menu { display: flex; }
                nav.menu > ul { list-style: none; }
                nav.menu li { position: relative; }
                nav.menu a { color: white; }
                nav.menu a:hover { background-color: rgba(0,0,0,0.1); }
                nav.menu li:first-child > a { border-radius: 4px 0 0 4px; }
            ";
            var (_, scopedCss) = ExtractAndScope(css);
            Assert.Contains("nav.menu.scope", scopedCss);
            Assert.Contains("nav.menu.scope > ul.scope", scopedCss);
            Assert.Contains("nav.menu.scope li.scope", scopedCss);
            Assert.Contains("nav.menu.scope a.scope", scopedCss);
            Assert.Contains("nav.menu.scope a:hover.scope", scopedCss);
            Assert.Contains("nav.menu.scope li:first-child.scope > a.scope", scopedCss);
        }

        [Fact]
        public void RealWorld_CardComponent()
        {
            var css = @"
                .card { border: 1px solid #ddd; border-radius: 4px; }
                .card .card-header { background-color: #f5f5f5; padding: 10px; }
                .card .card-body { padding: 15px; }
                .card .card-footer { background-color: #f5f5f5; padding: 10px; }
                .card.highlighted { border-color: gold; }
                .card:hover { box-shadow: 0 2px 8px rgba(0,0,0,0.1); }
            ";
            var (_, scopedCss) = ExtractAndScope(css);
            Assert.Contains(".card.scope", scopedCss);
            Assert.Contains(".card.scope .card-header.scope", scopedCss);
            Assert.Contains(".card.scope .card-body.scope", scopedCss);
            Assert.Contains(".card.highlighted.scope", scopedCss);
            Assert.Contains(".card:hover.scope", scopedCss);
        }

        #endregion

        #region エッジケース

        [Fact]
        public void EdgeCase_MultipleSpacesBetweenSelectors()
        {
            var (_, scopedCss) = ExtractAndScope(".parent   .child { color: blue; }");
            Assert.Contains(".parent.scope", scopedCss);
            Assert.Contains(".child.scope", scopedCss);
        }

        [Fact]
        public void EdgeCase_TrailingSpaces()
        {
            var (_, scopedCss) = ExtractAndScope(".parent .child   { color: blue; }");
            Assert.Contains(".parent.scope", scopedCss);
            Assert.Contains(".child.scope", scopedCss);
        }

        [Fact]
        public void EdgeCase_ChildCombinatorWithSpaces()
        {
            var (_, scopedCss) = ExtractAndScope(".parent > .child { color: blue; }");
            Assert.Contains(".parent.scope >", scopedCss);
            Assert.Contains("> .child.scope", scopedCss);
        }

        [Fact]
        public void EdgeCase_LongChainOfSelectors()
        {
            var (_, scopedCss) = ExtractAndScope("div.wrapper section.content article.post p.text { line-height: 1.6; }");
            Assert.Contains("div.wrapper.scope", scopedCss);
            Assert.Contains("section.content.scope", scopedCss);
            Assert.Contains("article.post.scope", scopedCss);
            Assert.Contains("p.text.scope", scopedCss);
        }

        [Fact]
        public void EdgeCase_SpecialCharactersInAttribute()
        {
            var (_, scopedCss) = ExtractAndScope("input[data-info*=\"test-123\"] { color: green; }");
            Assert.Contains("input[data-info*=\"test-123\"].scope", scopedCss);
        }

        #endregion

        #region 已知の制限

        [Fact]
        public void KnownLimitation_PseudoElementPlacedIncorrectly()
        {
            var (_, scopedCss) = ExtractAndScope("p::before { content: 'x'; }");
            Assert.Contains("p::before.scope", scopedCss);
        }

        [Fact]
        public void KnownLimitation_ComplexPseudoClassParamsWithOperators()
        {
            var (_, scopedCss) = ExtractAndScope("li:nth-child(odd) { background: white; }");
            Assert.Contains("li:nth-child(odd).scope", scopedCss);
        }

        #endregion
    }
}
