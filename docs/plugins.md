# プラグインの作り方（Micon.CMS）

Micon.CMS は ViewComponent ベースのプラグイン拡張を提供します。プラグイン DLL（埋め込み Razor ビュー含む）を `Micon.CMS/Plugins/` に配置するだけで自動認識されます。

## 前提
- .NET SDK 9.0 以上
- アプリは `dotnet run --project Micon.CMS`（Debug 推奨）

## 1) クラスライブラリ（プラグイン）を作成
1. 新規クラスライブラリを作成（例: `MyPlugin`）
   - `dotnet new classlib -n MyPlugin`
   - `TargetFramework` は `net9.0`
2. Razor ビューを埋め込む設定を `.csproj` に追加

例（抜粋）
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <!-- ViewComponent の Razor ビューを埋め込みリソースに含める -->
    <EmbeddedResource Include="Views\Shared\Components\Hello\Default.cshtml" />
    <!-- 必要に応じて _ViewImports を同梱し、TagHelper 等を有効化 -->
    <Content Include="Views\_ViewImports.cshtml">
      <CopyToPublishDirectory>PreserveNewest</CopyToPublishDirectory>
    </Content>
  </ItemGroup>
</Project>
```

## 2) ViewComponent を実装
規約: `Components/<任意>/<Name>ViewComponent.cs` と `Views/Shared/Components/<Name>/Default.cshtml`

例
```csharp
// MiconCmsSettings.cs（プラグイン識別子）
public class MiconCmsSettings
{
    public static readonly Guid PackageId = new("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
}

// Components/Hello/HelloViewComponent.cs
using Microsoft.AspNetCore.Mvc;
namespace MyPlugin.Components.Hello;
public class HelloViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(string name = "world")
        => View("Default", name);
}
```

```cshtml
@* Views/Shared/Components/Hello/Default.cshtml *@
@model string
<span>Hello, @Model!</span>
```

## 3) CMS 側への組み込み（自動）
`Micon.CMS` は起動時に `Plugins/**.dll` を再帰的にロードし、
- MVC `ApplicationPart` に追加
- Razor ランタイム再コンパイルの `EmbeddedFileProvider` に追加
を行います。`Program.cs` の変更は不要です。

## 4) DB 登録と描画
- `Components` へ以下を登録
  - `PackageId` = プラグイン側 `MiconCmsSettings.PackageId`
  - `Name` = ViewComponent の完全修飾名（例: `MyPlugin.Components.Hello.HelloViewComponent`）
- `ComponentRelations` で階層（ParentId/ChildId/SlotName）を構成
- `PageTemplates.ComponentRelationId` にルートの `ComponentRelation` を割り当て
- ページ単位の設定は `ComponentSettings`（Key/Value）に保存

Razor からの呼び出し（親コンポーネントのビュー）
```cshtml
@model Micon.CMS.Library.Models.Form.PageComponentViewModel
<div class="content">
  @await Html.LoadChildComponentAsync(Component, "Main")
  @await Html.LoadChildComponentAsync(Component, "Sidebar")
</div>
```

## よくあるつまずき
- ビューが見つからない: `.csproj` の `EmbeddedResource` 設定漏れ。
- 型が見つからない: `Components.Name` が FQCN になっていない／`PackageId` が一致していない。
- 依存 DLL が不足: プラグインの依存 DLL も `Plugins/` に配置。

## ビルド／動作確認
- `dotnet build Micon.CMS.sln`
- 環境変数を設定して実行
  - Windows: `$env:ASPNETCORE_ENVIRONMENT = "Development"`
  - 接続文字列: `$env:ConnectionStrings__micon-cms-db = "..."`
- `dotnet run --project Micon.CMS`

プラグインの読み込みログと画面出力で動作を確認してください。
