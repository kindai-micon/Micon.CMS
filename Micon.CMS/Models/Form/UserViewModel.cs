namespace Micon.CMS.Models.Form
{
    public class UserViewModel
    {
        public ApplicationUser User { get; set; }
        public IEnumerable<string> Roles { get; set; }
    }
}
