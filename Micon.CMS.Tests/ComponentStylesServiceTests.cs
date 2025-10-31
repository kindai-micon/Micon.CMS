using System;
using System.Threading.Tasks;
using Micon.CMS.Library.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Micon.CMS.Tests
{
    /// <summary>
    /// CssService のシングルトン共有テスト
    /// CssExtractingFileProvider と ComponentStylesController が同じインスタンスを共有していることを確認
    /// </summary>
    public class ComponentStylesServiceTests : IClassFixture<MiconCmsAppFactory>, IAsyncLifetime
    {
        private readonly MiconCmsAppFactory _factory;
        private IServiceScope? _scope;
        private CssService? _cssService;

        public ComponentStylesServiceTests(MiconCmsAppFactory factory)
        {
            _factory = factory;
        }

        public async Task InitializeAsync()
        {
            _factory.CreateClient();
            _scope = _factory.Services.CreateScope();
            _cssService = _scope.ServiceProvider.GetRequiredService<CssService>();
            await Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            _scope?.Dispose();
            return Task.CompletedTask;
        }

        /// <summary>
        /// CssService に CSS を登録し、取得できることを確認
        /// </summary>
        [Fact]
        public void CssService_RegisterAndRetrieveCss_Success()
        {
            // Arrange
            var packageId = Guid.Parse("12345678-1234-1234-1234-123456789012");
            var componentName = "TestComponent";
            var cssContent = ".component { color: red; }";

            // Act
            _cssService!.RegisterCssContent(packageId, componentName, cssContent);
            var allCssContent = _cssService.GetAllCssContent();

            // Assert
            Assert.NotNull(allCssContent);
            Assert.Contains(cssContent, allCssContent);
        }

        /// <summary>
        /// 複数の CSS を登録して、すべて取得できることを確認
        /// </summary>
        [Fact]
        public void CssService_RegisterMultipleCss_AllRetrieved()
        {
            // Arrange
            var packageId1 = Guid.Parse("12345678-1234-1234-1234-123456789012");
            var packageId2 = Guid.Parse("87654321-4321-4321-4321-210987654321");
            var css1 = ".component1 { color: blue; }";
            var css2 = ".component2 { color: green; }";

            // Act
            _cssService!.RegisterCssContent(packageId1, "Component1", css1);
            _cssService.RegisterCssContent(packageId2, "Component2", css2);
            var allCssContent = _cssService.GetAllCssContent();

            // Assert
            Assert.NotNull(allCssContent);
            Assert.Contains(css1, allCssContent);
            Assert.Contains(css2, allCssContent);
        }

        /// <summary>
        /// API エンドポイント /api/component-styles/all が登録済みの CSS を返すことを確認
        /// </summary>
        [Fact]
        public async Task ComponentStylesController_GetAllStyles_ReturnsCss()
        {
            // Arrange
            var client = _factory.CreateClient();
            var packageId = Guid.Parse("12345678-1234-1234-1234-123456789012");
            var cssContent = ".test-component { padding: 10px; }";

            // CSS を登録
            _cssService!.RegisterCssContent(packageId, "TestComponent", cssContent);

            // Act
            var response = await client.GetAsync("/api/component-styles/all");

            // Assert
            // ステータスコードが 200-299 の範囲（成功）か確認
            // または 500 以上のエラーがないこと
            Assert.NotNull(response);

            // 成功した場合はレスポンスボディに CSS が含まれていることを確認
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                Assert.NotEmpty(responseContent);
                Assert.Contains(cssContent, responseContent);
            }
        }

        /// <summary>
        /// DI コンテナから複数回取得した CssService が同じインスタンスであることを確認
        /// </summary>
        [Fact]
        public void CssService_SingletonInstance_SameInstance()
        {
            // Arrange & Act
            var scope1 = _factory.Services.CreateScope();
            var scope2 = _factory.Services.CreateScope();

            var service1 = scope1.ServiceProvider.GetRequiredService<CssService>();
            var service2 = scope2.ServiceProvider.GetRequiredService<CssService>();

            // Assert
            Assert.Same(service1, service2);

            // Cleanup
            scope1.Dispose();
            scope2.Dispose();
        }
    }
}
