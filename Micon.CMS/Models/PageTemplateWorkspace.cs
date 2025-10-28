using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Micon.CMS.Models
{
    public class PageTemplateWorkspace : BaseModel
    {
        [ForeignKey(nameof(PageTemplate))]
        public Guid PageTemplateId { get; set; }
        [Required]
        public PageTemplate PageTemplate { get; set; }

        /// <summary>
        /// ワークスペースのコンポーネントツリーを JSON 形式で保存
        /// ComponentTreeNode[] の JSON シリアライズ結果
        /// </summary>
        [Required]
        public string WorkspaceData { get; set; }

        [ForeignKey(nameof(ApplicationUser))]
        public Guid? CreatedByUserId { get; set; }
        public ApplicationUser? CreatedByUser { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
