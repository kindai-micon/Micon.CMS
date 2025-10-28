using System.Reflection;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Micon.CMS.Library.Services;

/// <summary>
/// Razorビュー内の LoadChildComponentAsync 呼び出しを解析し、スロット情報を抽出するサービス
/// </summary>
public class ComponentSlotAnalyzerService
{
    /// <summary>
    /// スロット情報
    /// </summary>
    public record SlotInfo(string SlotName);

    /// <summary>
    /// Razorビューファイル情報
    /// </summary>
    private record RazorFileInfo(string Path, string Content);

    /// <summary>
    /// 指定されたアセンブリから、すべてのRazorビューファイルを読み込み、
    /// LoadChildComponentAsync の呼び出しからスロット情報を抽出します。
    /// </summary>
    /// <param name="assembly">DLLアセンブリ</param>
    /// <param name="assemblyBasePath">アセンブリの基本パス（ファイルシステムから読み込む場合）</param>
    /// <returns>スロット情報の辞書。キーはビューファイルの相対パス、値はスロット名のリスト</returns>
    public Dictionary<string, List<SlotInfo>> AnalyzeComponentSlots(Assembly assembly, string? assemblyBasePath = null)
    {
        var result = new Dictionary<string, List<SlotInfo>>();

        // アセンブリからRazorビューファイルを検索
        var razorFiles = FindRazorViewFiles(assembly, assemblyBasePath);

        foreach (var razorFile in razorFiles)
        {
            var slots = ExtractSlotsFromRazorView(razorFile.Content);
            if (slots.Count > 0)
            {
                result[razorFile.Path] = slots;
            }
        }

        return result;
    }

    /// <summary>
    /// 指定されたコンポーネント型から、スロット情報を取得します。
    /// 優先順位：
    /// 1. IComponentSlots インターフェース実装（明示的定義）
    /// 2. Razorビュー静的解析（フォールバック）
    /// </summary>
    /// <param name="componentType">コンポーネントの型</param>
    /// <param name="assemblyBasePath">アセンブリの基本パス（ファイルシステムから読み込む場合）</param>
    /// <returns>スロット情報のリスト</returns>
    public List<SlotInfo> AnalyzeComponentSlots(Type componentType, string? assemblyBasePath = null)
    {
        // ステップ1: IComponentSlots インターフェース実装を確認
        var slots = GetSlotsFromInterface(componentType);
        if (slots != null && slots.Count > 0)
        {
            return slots;
        }

        // ステップ2: Razorビュー解析にフォールバック
        return GetSlotsFromRazorView(componentType, assemblyBasePath);
    }

    /// <summary>
    /// IComponentSlots インターフェースが実装されている場合、スロット情報を取得します。
    /// </summary>
    private List<SlotInfo>? GetSlotsFromInterface(Type componentType)
    {
        // IComponentSlots インターフェースを実装しているか確認
        var interfaceType = componentType.GetInterfaces()
            .FirstOrDefault(i => i.Name == nameof(IComponentSlots));

        if (interfaceType != null)
        {
            try
            {
                // 静的メソッド GetSlots() を呼び出す
                var method = componentType.GetMethod(
                    "GetSlots",
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

                if (method != null && method.ReturnType == typeof(List<string>))
                {
                    var result = method.Invoke(null, null) as List<string>;
                    if (result != null)
                    {
                        return result
                            .Select(slotName => new SlotInfo(slotName))
                            .ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error invoking GetSlots method: {ex.Message}");
            }
        }

        return null;
    }

    /// <summary>
    /// Razorビューファイルから静的解析でスロット情報を取得します。
    /// </summary>
    private List<SlotInfo> GetSlotsFromRazorView(Type componentType, string? assemblyBasePath)
    {
        var assembly = componentType.Assembly;
        var componentName = componentType.Name;

        // 命名規則に従う短い名前を抽出
        // 例：ClassLibrary1.Components.C1ViewComponent → C1ViewComponent → C1
        var shortComponentName = ExtractComponentShortName(componentName);

        System.Diagnostics.Debug.WriteLine($"[DEBUG] GetSlotsFromRazorView for: {componentName}");
        System.Diagnostics.Debug.WriteLine($"[DEBUG] Short name: {shortComponentName}");
        System.Diagnostics.Debug.WriteLine($"[DEBUG] Assembly path: {assembly.Location}");
        System.Diagnostics.Debug.WriteLine($"[DEBUG] assemblyBasePath: {assemblyBasePath ?? "null"}");

        // コンポーネント型から対応するRazorビューを検索
        var razorFiles = FindRazorViewFiles(assembly, assemblyBasePath);
        System.Diagnostics.Debug.WriteLine($"[DEBUG] Found {razorFiles.Count} razor files");
        foreach (var file in razorFiles)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG]   - {file.Path}");
        }

        // 命名規則に従うパターン: Views/Shared/Components/[ShortName]/ 配下のすべての .cshtml ファイル
        System.Diagnostics.Debug.WriteLine("[DEBUG] Expected patterns:");
        System.Diagnostics.Debug.WriteLine($"[DEBUG]   Slash: Components/{shortComponentName}/");
        System.Diagnostics.Debug.WriteLine($"[DEBUG]   Dots : Components.{shortComponentName}.");

        var matchingFiles = razorFiles.Where(f =>
        {
            var pathContains = IsComponentViewPath(f.Path, shortComponentName);
            var isCshtml = f.Path.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase);
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Checking {f.Path}: Contains={pathContains}, IsCshtml={isCshtml}");
            return pathContains && isCshtml;
        }).ToList();

        if (matchingFiles.Count > 0)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Found {matchingFiles.Count} matching files");
            var slots = new List<SlotInfo>();

            // すべてのマッチングファイルからスロット情報を抽出
            foreach (var file in matchingFiles)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Extracting slots from: {file.Path}");
                var fileSlots = ExtractSlotsFromRazorView(file.Content);
                foreach (var slot in fileSlots)
                {
                    // 重複を避ける
                    if (!slots.Any(s => s.SlotName == slot.SlotName))
                    {
                        slots.Add(slot);
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine($"[DEBUG] Total slots extracted: {slots.Count}");
            return slots;
        }

        System.Diagnostics.Debug.WriteLine($"[DEBUG] No matching files found for component: {componentName}");
        return new List<SlotInfo>();
    }

    /// <summary>
    /// ViewComponent の完全修飾名から短い名前を抽出します
    /// 例：ClassLibrary1.Components.C1ViewComponent → C1
    /// </summary>
    private string ExtractComponentShortName(string fullName)
    {
        // 最後のドット以降を取得（例：C1ViewComponent）
        var shortComponentName = fullName.Contains('.')
            ? fullName.Substring(fullName.LastIndexOf('.') + 1)
            : fullName;

        // ViewComponent サフィックスを削除（例：C1ViewComponent → C1）
        if (shortComponentName.EndsWith("ViewComponent", StringComparison.OrdinalIgnoreCase))
        {
            shortComponentName = shortComponentName.Substring(0, shortComponentName.Length - "ViewComponent".Length);
        }

        return shortComponentName;
    }

    /// <summary>
    /// ファイルパスが指定コンポーネントの Razor ビューかを判定します。
    /// 埋め込みリソース (Components.C1.Default.cshtml) とファイルシステム (Components/C1/Default.cshtml) の両方に対応します。
    /// </summary>
    private bool IsComponentViewPath(string path, string shortComponentName)
    {
        if (string.IsNullOrEmpty(path))
        {
            return false;
        }

        // パス区切りをスラッシュに統一して判定
        var normalizedWithSlash = path
            .Replace('\\', '/');
        if (normalizedWithSlash.Contains($"Components/{shortComponentName}/", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // 埋め込みリソースはドット区切りになるため、そのパターンでも判定
        var normalizedWithDot = path
            .Replace('\\', '.')
            .Replace('/', '.');
        if (normalizedWithDot.Contains($"Components.{shortComponentName}.", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Razorビューのコンテンツからスロット情報を抽出します。
    /// Razorファイルをコンパイルして生成されたC#コードをRoslynで解析し、
    /// LoadChildComponentAsync メソッド呼び出しを検出します。
    /// </summary>
    private List<SlotInfo> ExtractSlotsFromRazorView(string razorContent)
    {
        var slots = new List<SlotInfo>();

        try
        {
            // Razorコンテンツをコンパイルして生成C#コードを取得
            var generatedCSharpCode = CompileRazorToCSharp(razorContent);

            if (string.IsNullOrEmpty(generatedCSharpCode))
            {
                return slots;
            }

            // 生成されたC#コードをRoslynで解析
            var syntaxTree = CSharpSyntaxTree.ParseText(generatedCSharpCode);
            var root = syntaxTree.GetRoot() as CompilationUnitSyntax;

            if (root == null)
            {
                return slots;
            }

            // メソッド呼び出しを検出
            var invocations = root.DescendantNodes().OfType<InvocationExpressionSyntax>();

            foreach (var invocation in invocations)
            {
                // メソッド呼び出しの詳細を抽出
                var slotInfo = ExtractSlotFromInvocation(invocation);
                if (slotInfo != null && !slots.Any(s => s.SlotName == slotInfo.SlotName))
                {
                    slots.Add(slotInfo);
                }
            }
        }
        catch (Exception ex)
        {
            // Razorコンパイルエラーの場合はスキップ
            System.Diagnostics.Debug.WriteLine($"Error analyzing Razor view: {ex.Message}");
        }

        return slots;
    }

    /// <summary>
    /// インvocation式からスロット情報を抽出します。
    /// LoadChildComponentAsync(Component, "SlotName") パターンに対応します。
    /// </summary>
    private SlotInfo? ExtractSlotFromInvocation(InvocationExpressionSyntax invocation)
    {
        // メソッド名を確認（LoadChildComponentAsync）
        var methodName = GetMethodName(invocation.Expression);
        if (methodName != "LoadChildComponentAsync")
        {
            return null;
        }

        // 引数を確認
        var arguments = invocation.ArgumentList.Arguments;

        // 最低2つの引数が必要（Component, "SlotName"）
        if (arguments.Count < 2)
        {
            return null;
        }

        // 2番目の引数がスロット名（リテラル文字列）
        var secondArg = arguments[1].Expression;
        if (secondArg is LiteralExpressionSyntax literalExpr &&
            literalExpr.Kind() == Microsoft.CodeAnalysis.CSharp.SyntaxKind.StringLiteralExpression)
        {
            var slotNameWithQuotes = literalExpr.Token.ValueText;
            return new SlotInfo(slotNameWithQuotes);
        }

        return null;
    }

    /// <summary>
    /// メソッド呼び出し式からメソッド名を抽出します。
    /// </summary>
    private string? GetMethodName(ExpressionSyntax expression)
    {
        return expression switch
        {
            IdentifierNameSyntax id => id.Identifier.ValueText,
            MemberAccessExpressionSyntax ma => ma.Name.Identifier.ValueText,
            _ => null
        };
    }

    /// <summary>
    /// Razorコンテンツをコンパイルして、生成されたC#コードを返します。
    /// </summary>
    private string CompileRazorToCSharp(string razorContent)
    {
        try
        {
            // RazorProjectFileSystemを作成
            var fileSystem = new VirtualRazorProjectFileSystem(razorContent);
            var projectItem = fileSystem.GetItem("", "_view.cshtml");

            // RazorProjectEngineを作成
            var engine = RazorProjectEngine.Create(
                RazorConfiguration.Default,
                fileSystem);

            // Razorドキュメントを処理
            var document = engine.Process(projectItem);
            var csharpDocument = document.GetCSharpDocument();

            return csharpDocument.GeneratedCode;
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// 仮想ファイルシステムの実装
    /// </summary>
    private class VirtualRazorProjectFileSystem : RazorProjectFileSystem
    {
        private readonly string _razorContent;

        public VirtualRazorProjectFileSystem(string content)
        {
            _razorContent = content;
        }

        public override RazorProjectItem GetItem(string path)
        {
            return new VirtualRazorProjectItem(path, _razorContent);
        }

        public override RazorProjectItem GetItem(string basePath, string path = "")
        {
            var filePath = string.IsNullOrEmpty(path) ? basePath : Path.Combine(basePath, path);
            return new VirtualRazorProjectItem(filePath, _razorContent);
        }

        public override IEnumerable<RazorProjectItem> EnumerateItems(string basePath)
        {
            yield return new VirtualRazorProjectItem(basePath, _razorContent);
        }

        public override IEnumerable<RazorProjectItem> FindHierarchicalItems(
            string basePath,
            string path,
            string fileName) =>
            Enumerable.Empty<RazorProjectItem>();
    }

    /// <summary>
    /// 仮想RazorProjectItemの実装
    /// </summary>
    private class VirtualRazorProjectItem : RazorProjectItem
    {
        private readonly string _content;

        public VirtualRazorProjectItem(string path, string content)
        {
            FilePath = path;
            _content = content;
        }

        public override string BasePath => Path.GetDirectoryName(FilePath) ?? "";

        public override string FilePath { get; }

        public override string FileKind => FileKinds.Legacy;

        public override bool Exists => true;

        public override string PhysicalPath => FilePath;

        public override Stream Read()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(_content);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }

    /// <summary>
    /// アセンブリからRazorビューファイルを検索します。
    /// 埋め込みリソースとファイルシステムの両方に対応しています。
    /// </summary>
    private List<RazorFileInfo> FindRazorViewFiles(Assembly assembly, string? assemblyBasePath)
    {
        var result = new List<RazorFileInfo>();

        // パターン1: 埋め込みリソースからRazorビューを読む
        try
        {
            var resourceNames = assembly.GetManifestResourceNames();
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Assembly: {assembly.GetName().Name}");
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Total resources: {resourceNames.Length}");

            // すべてのリソース名をログ出力
            System.Diagnostics.Debug.WriteLine($"[DEBUG] All resource names:");
            foreach (var rn in resourceNames)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG]   - {rn}");
            }

            var razorResourceNames = resourceNames.Where(r =>
                r.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase) ||
                r.EndsWith(".razor", StringComparison.OrdinalIgnoreCase)).ToList();

            System.Diagnostics.Debug.WriteLine($"[DEBUG] Razor resources found: {razorResourceNames.Count}");
            foreach (var rr in razorResourceNames)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG]   - {rr}");
            }

            var assemblyName = assembly.GetName().Name;
            foreach (var resourceName in razorResourceNames)
            {
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            var content = reader.ReadToEnd();
                            var relativePath = assemblyName != null
                                ? resourceName.Replace(assemblyName + ".", "")
                                : resourceName;
                            result.Add(new RazorFileInfo(relativePath, content));
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // 埋め込みリソースの読み込みに失敗した場合は続行
            System.Diagnostics.Debug.WriteLine($"[ERROR] Failed to read embedded resources: {ex.Message}");
        }

        // パターン2: ファイルシステムからRazorビューを読む
        if (assemblyBasePath != null && Directory.Exists(assemblyBasePath))
        {
            try
            {
                var cstmlFiles = Directory.GetFiles(assemblyBasePath, "*.cshtml", SearchOption.AllDirectories);
                var razorFiles = Directory.GetFiles(assemblyBasePath, "*.razor", SearchOption.AllDirectories);

                foreach (var filePath in cstmlFiles.Concat(razorFiles))
                {
                    var content = File.ReadAllText(filePath);
                    var relativePath = Path.GetRelativePath(assemblyBasePath, filePath);
                    result.Add(new RazorFileInfo(relativePath, content));
                }
            }
            catch
            {
                // ファイル読み込みに失敗した場合は続行
            }
        }

        return result;
    }
}
