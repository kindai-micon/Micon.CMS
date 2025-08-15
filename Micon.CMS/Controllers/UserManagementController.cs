using Micon.CMS.Models;
using Micon.CMS.Models.Form;
using Micon.CMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Micon.CMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserManagementController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<ApplicationRole> roleManager,
        ApplicationDbContext applicationDbContext,
        IAuthorityScanService authorityScanService
        ) : Controller
    {
        [HttpGet(nameof(MyInfo))]
        public async Task<IActionResult> MyInfo()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // TODO: Roles
            //var roleStrList = await userManager.GetRolesAsync(user);
            //var roles = await roleManager.Roles
            //    .Include(x => x.Authorities)
            //    .Where(x => roleStrList.Contains(x.Name))
            //    .Select(x => new
            //    {
            //        x.Name,
            //        Authorities = x.Authorities.Select(a => a.Name).ToList()
            //    })
            //    .ToListAsync();

            return View(user);
        }

        [Authorize("UserView")]
        [HttpGet(nameof(UserInfo))]
        public async Task<IActionResult> UserInfo([FromQuery] string userName)
        {
            var user = await userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return NotFound();
            }

            //var roleStrList = await userManager.GetRolesAsync(user);
            //var roles = await roleManager.Roles
            //    .Include(x => x.Authorities)
            //    .Where(x => roleStrList.Contains(x.Name))
            //    .Select(x => new
            //    {
            //        x.Name,
            //        Authorities = x.Authorities.Select(a => a.Name).ToList()
            //    })
            //    .ToListAsync();

            return View(user);
        }

        //[Authorize(Policy = "UserView")]
        [HttpGet()]
        public async Task<IActionResult> Index()
        {
            var users = await userManager.Users
                //.Select(u => new
                //{
                //    u.Id,
                //    u.UserName,
                //    u.Email
                //})
                .ToListAsync();
            if (users == null || !users.Any())
            {
                users = new() {
                    new ApplicationUser { UserName = "aaa" }
                };
            }
            return View(users);
        }

        [HttpPost(nameof(LoginByUserName))]
        public async Task<IActionResult> LoginByUserName([FromBody] LoginModel loginModel)
        {
            var user = await userManager.FindByNameAsync(loginModel.UserName);
            if (user == null)
            {
                return NotFound();
            }

            var result = await signInManager.PasswordSignInAsync(user, loginModel.Password, true, false);
            if (!result.Succeeded)
            {
                return BadRequest();
            }
            
            //var roleStrList = await userManager.GetRolesAsync(user);
            //var roles = await roleManager.Roles
            //    .Include(x => x.Authorities)
            //    .Where(x => roleStrList.Contains(x.Name))
            //    .Select(x => new
            //    {
            //        x.Name,
            //        Authorities = x.Authorities.Select(a => a.Name).ToList()
            //    })
            //    .ToListAsync();

            return View(user);
        }

        [Authorize]
        [HttpPost(nameof(Logout))]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return View();
        }

        [Authorize(Policy="UserView")]
        [Authorize(Policy="UserManagement")]
        [HttpPost(nameof(Register))]
        public async Task<IActionResult> Register([FromBody] RegisterModel registerModel)
        {
            var existing = await userManager.FindByNameAsync(registerModel.UserName);
            if (existing != null)
            {
                return BadRequest(new IdentityError[]
                {
                    new() { Code = "Exists", Description = "ユーザーはすでに存在します" }
                });
            }

            var newUser = new ApplicationUser(registerModel.UserName);
            var result = await userManager.CreateAsync(newUser, registerModel.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return View();

            //var user = await userManager.FindByNameAsync(registerModel.UserName);
            //if (user == null)
            //{
            //    ApplicationUser applicationUser = new ApplicationUser(registerModel.UserName);
            //    var result = await userManager.CreateAsync(applicationUser, registerModel.Password);
            //    if (!result.Succeeded)
            //    {
            //        return BadRequest(result.Errors);
            //    }
            //    if (!result.Succeeded)
            //    {
            //        return BadRequest(result.Errors);
            //    }
            //}
            //else
            //{
            //    return BadRequest(new IdentityError[] { new() { Code = "Exists", Description = "ユーザーはすでに存在します" } });
            //}
            //return Ok(new SendUser(user));
        }

        //[HttpPost(nameof(InitialRegister))]
        //public async Task<IActionResult> InitialRegister(InitialUser initialUser)
        //{
        //    if (await passcodeService.CheckPasscodeAsync(initialUser.Passcode))
        //    {
        //        if (initialUser.Password != initialUser.ConfirmPassword)
        //        {
        //            return BadRequest(new IdentityError[] { new IdentityError() { Code = "Passcode", Description = "Passcodeが異なります" } });

        //        }
        //    }

        //    if (initialUser.Password != initialUser.ConfirmPassword)
        //    {
        //        return BadRequest(new IdentityError[]
        //        {
        //            new() { Code = "PasswordMismatch", Description = "確認用パスワードが一致しません" }
        //        });
        //    }

        //    var applicationUser = new ApplicationUser
        //    {
                //UserName = initialUser.UserName,
                //Email = initialUser.Email
            //};

            //var createUserResult = await userManager.CreateAsync(applicationUser, initialUser.Password);
            //if (!createUserResult.Succeeded) return BadRequest(createUserResult.Errors.ToArray());

            // Adminロール作成（存在しなければ）
        //    if (!await roleManager.RoleExistsAsync("Admin"))
        //    {
        //        var applicationRole = new ApplicationRole("Admin");

        //        // 権限スキャン結果を登録（テーブルに存在しなければ追加）
        //        var authorities = new List<Authority>();
        //        foreach (var name in authorityScanService.Authority)
        //        {
        //            var entity = (await applicationDbContext.Authorities.FirstOrDefaultAsync(a => a.Name == name))
        //                         ?? applicationDbContext.Authorities.Add(new Authority { Name = name }).Entity;
        //            authorities.Add(entity);
        //        }

        //        applicationRole.Authorities.AddRange(authorities);

        //        var roleCreateResult = await roleManager.CreateAsync(applicationRole);
        //        if (!roleCreateResult.Succeeded) return BadRequest(roleCreateResult.Errors.ToArray());

        //        await applicationDbContext.SaveChangesAsync();
        //    }

        //    var addRoleResult = await userManager.AddToRoleAsync(applicationUser, "Admin");
        //    if (!addRoleResult.Succeeded) return BadRequest(addRoleResult.Errors.ToArray());

        //    return Ok(new
        //    {
        //        applicationUser.Id,
        //        applicationUser.UserName,
        //        applicationUser.Email,
        //        Roles = new[] { "Admin" }
        //    });
        //}


        [Authorize("UserManagement")]
        [HttpPost(nameof(DeleteUser))]
        public async Task<IActionResult> DeleteUser([FromBody] string userName)
        {
            var my = await userManager.GetUserAsync(User);
            var user = await userManager.FindByNameAsync(userName);
            //var adminUsers = await userManager,GetUsersInRoleAsync("Admin");
            //if (adminUsers.Any(x => x.Id == user.Id) && adminUsers.Count == 1)
            //{
            //    return Conflict(new IdentityError[] { new() { Code = "adminerror", Description = "AdminUserが一人以上存在する必要があります" } });
            //}

            var result = await userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            if (my.Id == user.Id)
            {
                await signInManager.SignOutAsync();
            }
            return View();
        }

        


        //[HttpGet]
        //public IActionResult User([FromRoute]string id)
        //{
        //    return View();
        //}
    }
}
