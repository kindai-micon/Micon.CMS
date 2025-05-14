using Micon.CMS.Models;
using Micon.CMS.Models.Form;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Micon.CMS.Controllers
{
    public class UserDeleteController(UserManager<ApplicationUser> userManager) : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Delete([Bind("Password,UserId")] UserDeleteModel model)
        {
            ApplicationUser Deleter = await userManager.GetUserAsync(User);
            bool Passward_Ok = await userManager.CheckPasswordAsync(Deleter,model.Password);
            string DeletedUserId = model.UserId;
            ApplicationUser DeletedUser = await userManager.FindByIdAsync(DeletedUserId);
            if (Passward_Ok)
            {
                var DeleteResult = await userManager.DeleteAsync(DeletedUser);
                if (DeleteResult.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                return View("Delete/Index");
            }
            return View("Delete/Index");
        }
    }
}
