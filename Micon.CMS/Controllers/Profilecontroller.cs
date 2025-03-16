using Microsoft.AspNetCore.Mvc;
using Micon.CMS.Models;
using Microsoft.AspNetCore.Identity;
using Micon.CMS.Models.Form;
namespace Micon.CMS.Controllers
{
    public class ProfileController(UserManager<ApplicationUser> userManager,SignInManager<ApplicationUser> signInManager) : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public　async Task <IActionResult> Profile([Bind("UserName,Password,NewPassword")] EditProfileModel model)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return View("Login/Index");
            var passwordOk = await userManager.CheckPasswordAsync(user,model.Password);
            if (passwordOk)
            {
                var result =await signInManager.PasswordSignInAsync(user, model.Password, true, false);
            }
            return View("Index");
        }
  
    }
}
