# AGENTS.md

このリポジトリで自動/半自動エージェントや開発者が安全かつ一貫して作業できるようにするためのガイドです。ここに記載の方針は、このファイルが置かれているルート配下のすべてのディレクトリに適用されます。

## プロジェクト概要
- ソリューション: `Micon.CMS.sln`
- 主アプリ: `Micon.CMS` (ASP.NET Core, `net9.0`)
- 付随ライブラリ: `ClassLibrary1`
- Aspire 関連: `Micon.CMS.Aspire/Micon.CMS.Aspire.AppHost`, `Micon.CMS.Aspire/Micon.CMS.Aspire.ServiceDefaults`
- データベース: PostgreSQL (EF Core + Npgsql)
- `Program.cs` では接続文字列キー `micon-cms-db` を使用します（`ConnectionStrings__micon-cms-db` で注入可能）。

注意: `Program.cs` で `ClassLibrary1.dll` を `bin/Debug/net9.0` から直接ロードしています。`Release` 実行では失敗する可能性があるため、基本は Debug 構成での実行/デバッグを推奨します。

## 前提ツール
- .NET SDK 9.0 以上（`dotnet --info` で確認）
- PostgreSQL 14+ 推奨
- PowerShell 7+（Windows 開発時）/ Bash (macOS/Linux)

## OS 別コマンド早見表
以下はすべてリポジトリのルート、または明記したプロジェクト配下で実行してください。

### ビルド / 実行
- 依存関係復元 + ビルド
  - Windows (PowerShell): ``dotnet restore .; dotnet build Micon.CMS.sln``
  - macOS/Linux (bash): ``dotnet restore . && dotnet build Micon.CMS.sln``
- Web アプリ起動（Debug 構成推奨）
  - Windows: ``dotnet run --project Micon.CMS``
  - macOS/Linux: ``dotnet run --project Micon.CMS``
- ホットリロード
  - Windows: ``dotnet watch --project Micon.CMS run``
  - macOS/Linux: ``dotnet watch --project Micon.CMS run``

### Aspire AppHost（必要な場合）
- 起動
  - Windows: ``dotnet run --project "Micon.CMS.Aspire/Micon.CMS.Aspire.AppHost"``
  - macOS/Linux: ``dotnet run --project Micon.CMS.Aspire/Micon.CMS.Aspire.AppHost``

### テスト
- すべてのテスト実行
  - Windows: ``dotnet test Micon.CMS.sln``
  - macOS/Linux: ``dotnet test Micon.CMS.sln``

### データベース（EF Core）
- マイグレーションの適用
  - Windows: ``dotnet ef database update --project Micon.CMS``
  - macOS/Linux: ``dotnet ef database update --project Micon.CMS``
- 新規マイグレーションの追加（例: `Init`）
  - Windows: ``dotnet ef migrations add Init --project Micon.CMS``
  - macOS/Linux: ``dotnet ef migrations add Init --project Micon.CMS``

注: `dotnet-ef` が無い場合は ``dotnet tool install --global dotnet-ef`` を実行してください。

### 環境変数の設定
- アプリ実行時の環境（Development）
  - Windows (PowerShell): ``$env:ASPNETCORE_ENVIRONMENT = "Development"``
  - macOS/Linux (bash): ``export ASPNETCORE_ENVIRONMENT=Development``
- 接続文字列 `micon-cms-db` の注入（PostgreSQL の例）
  - 値の例: `Host=localhost;Port=5432;Database=micon_cms;Username=postgres;Password=postgres;` など
  - Windows (PowerShell): ``$env:ConnectionStrings__micon-cms-db = "Host=localhost;Port=5432;Database=micon_cms;Username=postgres;Password=postgres;"``
  - macOS/Linux (bash): ``export ConnectionStrings__micon-cms-db='Host=localhost;Port=5432;Database=micon_cms;Username=postgres;Password=postgres;'``

## 変更方針と作法
- 影響範囲を最小化し、既存スタイル/設計に合わせる（`Nullable`/`ImplicitUsings` 有効）。
- C# コード規約（要点）
  - 命名: `PascalCase`（型/メソッド）, `camelCase`（ローカル/パラメータ）, `_camelCase`（プライベートフィールド）
  - 非同期 API には `Async` サフィックス、`await` を用いた非同期優先。
  - DI/`IServiceCollection` による依存注入、一時的ロジックは Controller に詰め込まない。
  - 例外は握りつぶさない。必要に応じてロガーを使用。
- EF Core
  - マイグレーション由来の変更はコミットを分ける。DB 変更の説明を残す。
  - 接続文字列キーは `micon-cms-db` を使用。キー変更時は `Program.cs` とドキュメントを同時更新。
- Razor/Views
  - ランタイム再コンパイルを活用（開発時）。部分ビューやコンポーネントは再利用前提で設計。
- プラグイン/静的ファイル
  - `Plugins/Pages` 配下を参照している処理（`PhysicalFileProvider`）に留意。配置/パスを変える場合は `Program.cs` を追随更新。
- デバッグ/ビルド構成
  - `bin/Debug/net9.0` への明示ロードに依存するコードがあるため、基本は Debug 構成での実行を推奨。Release 対応を行う際は読み込みロジックの改修をセットで行うこと。

## レビューと検証
- 変更前後で `dotnet build` と `dotnet test` を実行し、影響を確認。
- DB 変更がある場合はローカルで `database update` を適用して起動確認。
- 実行確認: ``ASPNETCORE_ENVIRONMENT=Development`` で `dotnet run --project Micon.CMS` を起動し、起動ログ/例外を確認。

## エージェント向け運用指針
- 作業の目的・前提・影響範囲を要約し、段階的に進める。
- 破壊的操作（`rm` 等）や設定ファイルの大規模変更は避ける/事前に明記。
- 不要なリファクタや無関係な修正を混在させない。ドキュメントは必要最小限で同期更新。
- 外部ネットワーク依存の操作（パッケージ追加等）は明示の許可を得る。

## トラブルシュート（抜粋）
- 「接続文字列が見つからない」
  - `ConnectionStrings__micon-cms-db` が設定されているか確認。
- 「Release でビューが表示されない/エラー」
  - `ClassLibrary1.dll` の直接ロードが Debug 固定になっていないか確認。
- 「マイグレーション/EF コマンドが動かない」
  - `dotnet-ef` がインストールされているか、ターゲットプロジェクトを `--project Micon.CMS` で明示しているか確認。

---
このファイルに書かれたルールは、上書きするより補助することを目的としています。状況により最適な判断がある場合は、変更理由を明記したうえで柔軟に対応してください。
