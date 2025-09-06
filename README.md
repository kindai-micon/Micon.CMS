# Micon.CMS

モジュール拡張を前提に設計された、ASP.NET Core（.NET 9）ベースの軽量CMSです。Razorのランタイム再コンパイルやプラグイン/テーマ機構、EF Core（PostgreSQL）を活用して、拡張しやすく運用しやすいWebアプリケーションを目指しています。

## 特徴（What it can do）
- モジュール/プラグイン拡張
  - `Plugins/Pages` 配下のページ/テーマ資産を参照できる構成。
  - `ClassLibrary1` のようなクラスライブラリから、埋め込みリソースとして ViewComponent 等を提供可能。
- テーマ/静的ファイル
  - `Plugins/Pages/Themes` を `/Themes` パスにマウントして配信。
- 認証/認可
  - ASP.NET Core Identity（ユーザー/ロール/トークン）に対応。
- データ永続化
  - Entity Framework Core + Npgsql（PostgreSQL）。アプリ起動時に `Database.Migrate()` を実行し、自動で最新スキーマへ更新。
- 開発者体験
  - Razor ランタイム再コンパイル（開発時の変更反映が高速）。
  - `dotnet watch run` によるホットリロード。
- サービスオーケストレーション
  - .NET Aspire の AppHost/ServiceDefaults を同梱（必要に応じて利用）。

想定ユースケース
- プラグイン由来のビュー/コンポーネントでWebページを構成するCMSの基盤。
- テーマ切替や拡張コンポーネントの追加を通じてサイトを段階的に拡張。
- 既存の .NET エコシステム（EF、Identity、Razor）をベースにした学習/実験環境。

## クイックスタート

前提
- .NET SDK 9.0 以上（`dotnet --info`）
- PostgreSQL 14+ 推奨

1) 依存関係の復元とビルド
- Windows (PowerShell): `dotnet restore .; dotnet build Micon.CMS.sln`
- macOS/Linux (bash): `dotnet restore . && dotnet build Micon.CMS.sln`

2) 環境変数の設定（開発）
- Windows: `$env:ASPNETCORE_ENVIRONMENT = "Development"`
- macOS/Linux: `export ASPNETCORE_ENVIRONMENT=Development`

3) 接続文字列の設定（例: ローカルPostgreSQL）
- Windows: `$env:ConnectionStrings__micon-cms-db = "Host=localhost;Port=5432;Database=micon_cms;Username=postgres;Password=postgres;"`
- macOS/Linux: `export ConnectionStrings__micon-cms-db='Host=localhost;Port=5432;Database=micon_cms;Username=postgres;Password=postgres;'`

4) 実行（Debug推奨）
- 共通: `dotnet run --project Micon.CMS`
  - 初回起動時に自動でDBマイグレーションを適用します。

ホットリロード
- 共通: `dotnet watch --project Micon.CMS run`

## Aspire AppHost（任意）
- Windows: `dotnet run --project "Micon.CMS.Aspire/Micon.CMS.Aspire.AppHost"`
- macOS/Linux: `dotnet run --project Micon.CMS.Aspire/Micon.CMS.Aspire.AppHost`

## 構成とアーキテクチャ
- ソリューション: `Micon.CMS.sln`
- プロジェクト
  - `Micon.CMS`: ASP.NET Core MVC アプリ（`net9.0`）。
    - `Program.cs`
      - CORS（開発時）
      - Razor ランタイム再コンパイルと `EmbeddedFileProvider`（`ClassLibrary1` の ViewComponent 使用）
      - `Plugins/Pages` と `/Themes` のファイル提供
      - EF Core（`micon-cms-db` 接続文字列）と Identity 設定
      - 起動時 `Database.Migrate()` を実行
  - `ClassLibrary1`: ビューコンポーネント等を含むクラスライブラリ（埋め込みリソースとして参照）。
  - `Micon.CMS.Aspire.AppHost` / `Micon.CMS.Aspire.ServiceDefaults`: Aspire 関連。

ディレクトリ要点
- `Migrations/`: EF Core マイグレーション
- `Repositories/`: リポジトリ層（例: PageTemplateRepository）
- `Views/`: MVCビュー
- `Plugins/Pages/`: 拡張ページやテーマ資産

## 設定
- 環境: `ASPNETCORE_ENVIRONMENT`（Development/Production）
- 接続文字列キー: `micon-cms-db`
  - 環境変数経由で `ConnectionStrings__micon-cms-db` を設定可能
- `appsettings.json` / `appsettings.Development.json` によるロギング等の設定

## 開発のヒント（できることの詳細）
- プラグイン/テーマの追加
  - `Plugins/Pages` 配下に静的資産やRazorページを配置して拡張。
  - テーマ資産は `/Themes` で配信されるため、CSS/画像等の差し替えが容易。
- コンポーネント拡張
  - `ClassLibrary1` のようにクラスライブラリ側で ViewComponent を実装し、埋め込みリソースとして提供。
  - ランタイム再コンパイルにより、開発時の反映が高速。
- データアクセス
  - EF Core を利用。スキーマ変更は `dotnet ef migrations add <Name>` → `dotnet ef database update` の手順で進行。
- 認証/認可
  - Identity により、ユーザー/ロールを用いたアクセス制御が可能。
- サービス構成
  - Aspire を使う場合は AppHost から一括起動し、構成情報を一元管理可能。

## 既知の注意点（重要）
- Debug 固定のアセンブリ読み込み
  - `Program.cs` で `ClassLibrary1.dll` を `bin/Debug/net9.0` から直接ロードしています。Release 実行時に失敗する可能性があるため、現状は Debug 構成での実行を推奨します。
  - Release 対応を行う場合は、ビルド構成に依存しないアセンブリ解決（`IWebHostEnvironment` や `AppContext.BaseDirectory` を用いた探索等）への改修を検討してください。

## トラブルシュート
- 接続文字列が解決されない
  - `ConnectionStrings__micon-cms-db` が設定されているか確認。
- マイグレーション/EF コマンドが失敗する
  - `dotnet tool install --global dotnet-ef` を実行済みか確認。
  - コマンドに `--project Micon.CMS` を付与して対象を明示。
- Release でビューやコンポーネントが読み込めない
  - 上述の Debug 固定読み込みに該当していないか確認。

## ライセンス
このリポジトリは `LICENSE.txt` の内容に従います。

## コントリビュート
Issue/PR は歓迎です。大きめの変更や外部ネットワークを伴う依存追加は、事前に方針を相談してください。プロジェクト内の規約/コマンドは `AGENTS.md` にまとめています。
