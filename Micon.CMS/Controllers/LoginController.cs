using Micon.CMS.Models;
using Micon.CMS.Models.Form;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Micon.CMS.Controllers
{
    public class LoginController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager) : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Login([Bind("Organization, UserName, Password")] LoginModel model)
        {
            var user = await userManager.FindByNameAsync(model.UserName);
            if (user != null)
            {
                var result = await signInManager.PasswordSignInAsync(user, model.Password, true, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    // Add TenantId to claims
                    var claims = new List<Claim>
                    {
                        new Claim("TenantId", user.TenantId.ToString())
                    };
                    // Remove old claim if exists and add new one
                    var oldClaim = (await userManager.GetClaimsAsync(user)).FirstOrDefault(c => c.Type == "TenantId");
                    if(oldClaim != null)
                    {
                        await userManager.RemoveClaimAsync(user, oldClaim);
                    }
                    await userManager.AddClaimAsync(user, new Claim("TenantId", user.TenantId.ToString()));


                    return RedirectToAction("Index", "Home");
                }
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
