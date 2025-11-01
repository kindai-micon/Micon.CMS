using Micon.CMS.Library.Services;
using Micon.CMS.Services;
using Micon.CMS.Providers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text;
using Xunit;

namespace Micon.CMS.Tests
{
    /// <summary>
    /// テスト用のモック MiconCmsSettings
    /// </summary>
    public class MiconCmsSettings
    {
        public static Guid PackageId = Guid.Parse("12345678-1234-1234-1234-123456789012");
    }

    public class DebugHtmlClassTest
    {
        [Fact]
        public void HtmlClassModification_PreservesExistingClasses_WithScopeClass()
        {
            // Arrange
            var loggerFactory = new TestLoggerFactory();
            var cssExtractorLogger = loggerFactory.CreateLogger<CssExtractorService>();
            var cssServiceLogger = loggerFactory.CreateLogger<CssService>();
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var cssService = new CssService(memoryCache, cssServiceLogger);
            var cssExtractor = new CssExtractorService(cssExtractorLogger);

            // テスト用の仮想ファイル
            var fileContent = @"<style>.component { color: red; }</style>
<div class=""component"">Test</div>";

            // モックの IFileInfo を作成
            var mockFileInfo = new MockFileInfo(fileContent);

            // CssExtractingFileInfo を作成（これが AddScopedClassToAllElements を呼ぶ）
            var cssExtractingInfo = new CssExtractingFileInfo(
                mockFileInfo,
                cssExtractor,
                cssService,
                "Views/Shared/Components/C1/Default.cshtml",
                cssExtractorLogger);

            // Act
            var resultStream = cssExtractingInfo.CreateReadStream();
            var resultContent = new StreamReader(resultStream).ReadToEnd();

            // Assert
            // 1. Style タグが抽出されているか確認
            Assert.DoesNotContain("<style>", resultContent);
            Assert.DoesNotContain("</style>", resultContent);

            // 2. HTML クラスが正しく追加されているか確認（既存クラス + スコープクラス）
            Assert.Contains(@"class=""component 12345678123412341234123456789012_C1Default""", resultContent);

            // 3. 開き括弧タグとテキストが保持されているか確認
            Assert.Contains(@">Test</div>", resultContent);
        }

        [Fact]
        public void HtmlClassModification_NoExistingClass_CreatesWithScopeClass()
        {
            // Arrange
            var loggerFactory = new TestLoggerFactory();
            var cssExtractorLogger = loggerFactory.CreateLogger<CssExtractorService>();
            var cssServiceLogger = loggerFactory.CreateLogger<CssService>();
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var cssService = new CssService(memoryCache, cssServiceLogger);
            var cssExtractor = new CssExtractorService(cssExtractorLogger);

            // テスト用の仮想ファイル - class属性なし
            var fileContent = @"<style>.component { color: red; }</style>
<div>Test</div>";

            // モックの IFileInfo を作成
            var mockFileInfo = new MockFileInfo(fileContent);

            // CssExtractingFileInfo を作成
            var cssExtractingInfo = new CssExtractingFileInfo(
                mockFileInfo,
                cssExtractor,
                cssService,
                "Views/Shared/Components/C1/Default.cshtml",
                cssExtractorLogger);

            // Act
            var resultStream = cssExtractingInfo.CreateReadStream();
            var resultContent = new StreamReader(resultStream).ReadToEnd();

            // Assert
            // クラスがなかった要素にスコープクラスが追加されているか確認
            Assert.Contains(@"class=""12345678123412341234123456789012_C1Default""", resultContent);
        }
    }

    /// <summary>
    /// IFileInfo のモック実装
    /// </summary>
    public class MockFileInfo : IFileInfo
    {
        private readonly string _content;

        public MockFileInfo(string content)
        {
            _content = content;
        }

        public bool Exists => true;
        public long Length => _content.Length;
        public string PhysicalPath => null!;
        public string Name => "Default.cshtml";
        public DateTimeOffset LastModified => DateTimeOffset.Now;
        public bool IsDirectory => false;

        public Stream CreateReadStream()
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(_content));
        }
    }

    /// <summary>
    /// テスト用のシンプルなロガーファクトリ
    /// </summary>
    public class TestLoggerFactory : ILoggerFactory
    {
        public ILogger CreateLogger(string categoryName) => new TestLogger();
        public ILogger<T> CreateLogger<T>() where T : class => new TestLogger<T>();
        public void AddProvider(ILoggerProvider provider) { }
        public void Dispose() { }
    }

    /// <summary>
    /// テスト用のシンプルなロガー
    /// </summary>
    public class TestLogger : ILogger
    {
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => null!;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
    }

    /// <summary>
    /// テスト用のジェネリックロガー
    /// </summary>
    public class TestLogger<T> : ILogger<T>
    {
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => null!;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
    }
}
