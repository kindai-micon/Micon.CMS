namespace Micon.CMS.Library.Services;

/// <summary>
/// ViewComponent が使用するスロットを明示的に定義するインターフェース
/// </summary>
public interface IComponentSlots
{
    /// <summary>
    /// このコンポーネントが使用するスロット名のリストを返す
    /// </summary>
    /// <returns>スロット名のリスト（例：["Main", "ChildSlot"]）</returns>
    static abstract List<string> GetSlots();
}
