using Micon.CMS.Models;
using Micon.CMS.Models.Form;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Micon.CMS.Controllers
{
    public class UserDeleteController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager) : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Delete([Bind("Password")] LoginModel model)
        {
            var user = await userManager.GetUserAsync(User);
            var passward_ok = await userManager.CheckPasswordAsync(user,model.Password);
            if (passward_ok)
            {

                return RedirectToAction("Index", "Home");
            }
            else
            {
                return View("Index");
            }
        }
    }
}
