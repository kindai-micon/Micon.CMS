using System.ComponentModel.DataAnnotations;

namespace Micon.CMS.Models.Form
{
    public class CreateUserViewModel
    {
        [Required]
        [Display(Name = "ユーザー名")]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "{0} は {2} 文字以上、{1} 文字以下である必要があります。", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "パスワード")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "パスワードの確認")]
        [Compare("Password", ErrorMessage = "パスワードと確認用パスワードが一致しません。")]
        public string ConfirmPassword { get; set; }

        public List<string> SelectedRoles { get; set; }
        public List<ApplicationRole> AllRoles { get; set; } = new List<ApplicationRole>();
    }
}
