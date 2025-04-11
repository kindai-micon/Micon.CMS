using Micon.CMS.Models;
using Micon.CMS.Models.Form;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public async Task<IActionResult> Delete([Bind("Password,UserId")] UserDeleteModel model)
        {
            var user = await userManager.GetUserAsync(User);
            var passward_ok = await userManager.CheckPasswordAsync(user,model.Password);
            var UserId = model.UserId;
            var DeletedUser = await userManager.FindByIdAsync(UserId);
            if (passward_ok)
            {
                
                _=await userManager.DeleteAsync(DeletedUser);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return View("Index");
            }
        }
    }
}
