using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Micon.CMS.Models.Form
{
    public class UserDeleteModel
    {
        [Required]
        public string Password { get; set; }
        [Required]

        public string UserId { get; set; }
    }
}
