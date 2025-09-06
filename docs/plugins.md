# プラグインの作り方（Micon.CMS）

Micon.CMS は 2 つの拡張パスを提供します。

- ファイルベース拡張（Themes/静的資産）: `Plugins/Pages/Themes` 配下を `/Themes` で配信
- アセンブリベース拡張（ViewComponent）: クラスライブラリから埋め込み Razor ビューを提供

以下では後者（ViewComponent ベースのプラグイン）を主に解説し、あわせてファイルベース拡張のポイントも記載します。

## 前提
- .NET SDK 9.0 以上
- アプリは Debug 構成で実行（現状 `bin/Debug/net9.0` 直読み込みのため）
- 実行: `dotnet run --project Micon.CMS`

## 1) クラスライブラリ（プラグイン）を作成
1. ルートでクラスライブラリを作成（例: `MyPlugin`）
   - 例: `dotnet new classlib -n MyPlugin`
   - `TargetFramework` は `net9.0` を推奨
2. 必要パッケージ（ViewComponent 用）
   - `Microsoft.AspNetCore.Mvc.ViewFeatures`
3. Razor ビューを埋め込む設定を `.csproj` に追加

例（抜粋）:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Mvc.ViewFeatures" Version="2.3.0" />
  </ItemGroup>
  <ItemGroup>
    <!-- ViewComponent の Razor ビューを埋め込みリソースとして組み込む -->
    <EmbeddedResource Include="Views\Shared\Components\Hello\Default.cshtml" />
    <!-- 任意: _ViewImports を配布したい場合は Content などで同梱 -->
    <Content Include="Views\_ViewImports.cshtml">
      <CopyToPublishDirectory>PreserveNewest</CopyToPublishDirectory>
    </Content>
  </ItemGroup>
</Project>
```

## 2) ViewComponent を実装
規約: `Components/<任意>/<Name>ViewComponent.cs` とし、`Views/Shared/Components/<Name>/<ViewName>.cshtml` を用意します。

例: `Components/Hello/HelloViewComponent.cs`
```csharp
using Microsoft.AspNetCore.Mvc;

namespace MyPlugin.Components.Hello;

public class HelloViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(string name = "world")
        => View("Default", name);
}
```

例: `Views/Shared/Components/Hello/Default.cshtml`
```cshtml
@model string
<span>Hello, @Model!</span>
```

ポイント:
- ビューは必ず `EmbeddedResource` に含める（`.csproj`）。
- 必要なら `Views/_ViewImports.cshtml` を同梱し、TagHelper などを有効化。

## 3) CMS 側への組み込み
Micon.CMS は起動時にプラグインのアセンブリを読み込み、Razor Runtime Compilation に埋め込みビューの `FileProvider` を追加する必要があります。

最小例（`Program.cs` 内のイメージ）:
```csharp
using Microsoft.Extensions.FileProviders;
// 既存: typeof(TestViewComponent).Assembly を追加している箇所に追記
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation(options =>
{
    // 既存のプラグイン
    options.FileProviders.Add(new EmbeddedFileProvider(typeof(ClassLibrary1.Components.Test.TestViewComponent).Assembly));

    // 新規プラグイン（型を通じてアセンブリを参照）
    options.FileProviders.Add(new EmbeddedFileProvider(typeof(MyPlugin.Components.Hello.HelloViewComponent).Assembly));
});
```

または、ビルド成果物（`bin/Debug/net9.0/MyPlugin.dll`）のパスから読み込む場合は、`AssemblyLoadContext` でロードし、その `Assembly` を `EmbeddedFileProvider` に渡します。

注意:
- 現状は Debug パス固定の読み込み実装が含まれています。Release 対応する場合は `AppContext.BaseDirectory` などで実行ディレクトリから解決する実装へ改修してください。

## 4) ViewComponent の呼び出し（CMS 側 Razor）
Razor からの呼び出し例:
```cshtml
@await Component.InvokeAsync("Hello", new { name = "Micon" })
```
またはジェネリック版:
```cshtml
@using MyPlugin.Components.Hello
@(await Component.InvokeAsync<HelloViewComponent>(new { name = "Micon" }))
```

## 5) ファイルベース拡張（Themes/静的資産）
- `Plugins/Pages/Themes` 配下の静的ファイルは、`/Themes` で配信されます。
- 例: `Plugins/Pages/Themes/MyTheme/css/site.css` → `/Themes/MyTheme/css/site.css`
- レイアウトやビューから通常の `<link>`/`<script>` 参照で利用できます。

（補足）`Plugins/Pages` 直下の Razor 参照について
- 実行時コンパイルで物理パスを追加する実装が必要です。現在の `Program.cs` では埋め込みビューを優先しており、物理パスを追加していません。
- 物理パスからも拾いたい場合は `AddRazorRuntimeCompilation` の `options.FileProviders.Add(new PhysicalFileProvider(<Plugins/Pages>))` を追加してください。

## よくあるつまずき
- ビューが見つからない: `.csproj` に `EmbeddedResource` が漏れている可能性。
- 文字化け/TagHelper が効かない: `Views/_ViewImports.cshtml` 未同梱の可能性。
- Release で動かない: Debug パス固定読み込みの影響。読み込みロジックの改修が必要。

## ビルド・動作確認
- ルートで `dotnet build Micon.CMS.sln`
- 環境変数を設定して実行
  - Windows: `$env:ASPNETCORE_ENVIRONMENT = "Development"`
  - 接続文字列: `$env:ConnectionStrings__micon-cms-db = "..."`
- 起動: `dotnet run --project Micon.CMS`

プラグインの読み込みログや ViewComponent の出力を画面・コンソールで確認してください。

