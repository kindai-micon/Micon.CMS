using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Micon.CMS.Models
{
    /// <summary>
    /// コンポーネント内で定義されているスロット情報
    /// LoadChildComponentAsync 呼び出しから動的に検出
    /// </summary>
    public class ComponentSlot : BaseModel
    {
        [Required]
        [ForeignKey("Component")]
        public Guid ComponentId { get; set; }

        [Required]
        public string SlotName { get; set; } = string.Empty;

        /// <summary>
        /// スロットが定義されているRazorビューファイルのパス
        /// </summary>
        public string ViewFilePath { get; set; } = string.Empty;

        public Component? Component { get; set; }
    }
}
