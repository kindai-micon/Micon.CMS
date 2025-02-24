using System.ComponentModel.DataAnnotations.Schema;

namespace Micon.CMS.Models
{
    public class PageTemplate : BaseModel
    {
        [Column(TypeName = "varchar(256)")]
        public string Name { get; set; }

    }
}
