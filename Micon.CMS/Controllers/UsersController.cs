
using Micon.CMS.Models;
using Micon.CMS.Models.Form;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Micon.CMS.Controllers
{
    public class UsersController(UserManager<ApplicationUser> userManager):Controller
    {
        public async Task<IActionResult>Index()
        {
            var userlist = await userManager.Users.ToListAsync();
            return View(userlist);
        }
    }
}
