using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Micon.CMS.Models.Form
{
    public class Users
    {
        [Required]
        public string UserName{ get; set; }
        [Required]
        public string Age { get; set; }
        [Required]
        public string Comment { get; set; }
        [Required]
        public string account { get; set; }
        [Required]
        public string Rabel { get; set; }
    }
}
