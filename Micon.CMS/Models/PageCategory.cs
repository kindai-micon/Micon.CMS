using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Micon.CMS.Models
{
    public class PageCategory : BaseModel
    {
        [Required]
        [StringLength(256)]
        public string Name { get;set; }
        
        public PageTemplate PageTemplate { get; set; }
    }
}
