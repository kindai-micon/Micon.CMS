using System.ComponentModel.DataAnnotations.Schema;

namespace Micon.CMS.Models
{
    public class ContentCategory:BaseModel
    {
        [Column(TypeName = "varchar(512)")]
        public string CategoryName { get; set; }
    }
}
