using Micon.CMS.Services;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Micon.CMS.Tests
{
    /// <summary>
    /// CssExtractorService がさまざまなセレクタに対応しているかのテスト
    /// </summary>
    public class CssExtractorSelectorTests
    {
        private readonly CssExtractorService _cssExtractor;

        public CssExtractorSelectorTests()
        {
            var loggerFactory = new TestLoggerFactory();
            var logger = loggerFactory.CreateLogger<CssExtractorService>();
            _cssExtractor = new CssExtractorService(logger);
        }

        [Theory]
        [InlineData("p { color: red; }", "p.scope")]
        [InlineData(".component { color: red; }", ".component.scope")]
        [InlineData("#header { color: red; }", "#header.scope")]
        [InlineData("p:first-child { color: red; }", "p:first-child.scope")]
        [InlineData("p:nth-of-type(2) { color: red; }", "p:nth-of-type(2).scope")]
        [InlineData("p[class^=\"ttl\"] { color: red; }", "p[class^=\"ttl\"].scope")]
        [InlineData(".component h1 { color: red; }", ".component.scope h1.scope")]
        [InlineData(".component > .child { color: red; }", ".component.scope > .child.scope")]
        [InlineData(".component + .sibling { color: red; }", ".component.scope + .sibling.scope")]
        [InlineData(".component ~ .sibling { color: red; }", ".component.scope ~ .sibling.scope")]
        [InlineData(".a, .b { color: red; }", ".a.scope, .b.scope")]
        public void ExtractCss_VariousSelectors_ScopedCorrectly(string cssInput, string expectedSelector)
        {
            // Arrange
            var packageId = System.Guid.Parse("12345678-1234-1234-1234-123456789012");
            var scopeClassName = "scope";
            var razorContent = $"<style>{cssInput}</style><div>test</div>";

            // Act
            var (cleanedRazor, scopedCss) = _cssExtractor.ExtractCss(razorContent, scopeClassName, packageId);

            // Assert
            Assert.NotNull(scopedCss);
            Assert.Contains(expectedSelector, scopedCss);
            Assert.DoesNotContain("<style>", cleanedRazor);
        }

        [Fact]
        public void ExtractCss_PseudoElements_HandledCorrectly()
        {
            // Arrange
            var packageId = System.Guid.Parse("12345678-1234-1234-1234-123456789012");
            var scopeClassName = "scope";

            var razorContent = "\"<style>p::before { content: 'x'; }</style>\"";

            // Act
            var (cleanedRazor, scopedCss) = _cssExtractor.ExtractCss(razorContent, scopeClassName, packageId);

            // Assert
            Assert.NotNull(scopedCss);
            Assert.Contains("p::before.scope", scopedCss);
        }

        [Fact]
        public void ExtractCss_ComplexSelector_WithMultiplePseudoClasses()
        {
            // Arrange
            var packageId = System.Guid.Parse("12345678-1234-1234-1234-123456789012");
            var scopeClassName = "scope";
            var razorContent = "\"<style>ul li:first-child { color: red; }</style>\"";

            // Act
            var (cleanedRazor, scopedCss) = _cssExtractor.ExtractCss(razorContent, scopeClassName, packageId);

            // Assert
            Assert.NotNull(scopedCss);
            Assert.Contains("ul.scope li:first-child.scope", scopedCss);
        }
    }
}
