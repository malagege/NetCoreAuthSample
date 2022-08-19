using AuthSample.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace AuthSample.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        #region 建立角色
        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoleAsync(CreateRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                //角色 Name 不能重覆
                var identityRole = new IdentityRole
                {
                    Name = model.RoleName,   
                };

                IdentityResult result = await _roleManager.CreateAsync(identityRole);

                if (result.Succeeded)
                {
                    return RedirectToAction("rolelist", "admin");
                }

                foreach(IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }
        #endregion

        #region 角色清單
        public IActionResult RoleList()
        {
            IQueryable<IdentityRole> roles = _roleManager.Roles;
            return View(roles);
        }
        #endregion
    }
}
