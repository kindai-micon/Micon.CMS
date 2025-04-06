using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Micon.CMS.Models.Form
{
    public class LoginModel
    {
        [Required]
        public string Organization { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
