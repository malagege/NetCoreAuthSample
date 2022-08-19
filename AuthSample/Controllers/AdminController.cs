using AuthSample.Models;
using AuthSample.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthSample.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(RoleManager<IdentityRole> roleManager,UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
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

        # region 編輯角色
        [HttpGet]
        public async Task<IActionResult> EditRoleAsync(string id)
        {
            // 通過參數 id 帶入 roleManager 查詢
            IdentityRole role = await _roleManager.FindByIdAsync(id);
            if( role == null)
            {
                ViewBag.ErrorMessage = $"角色ID:{id}不存在";
                return View("NotFound");
            }

            var model = new EditRoleViewModel
            {
                Id = role.Id,
                RoleName = role.Name,
            };
            IList<ApplicationUser> users = await _userManager.Users.ToListAsync();

            foreach (var user in users)
            {
                // 每個使用者的 role 判斷是否有權限
                if(await _userManager.IsInRoleAsync(user, role.Name))
                {
                    model.Users.Add(user.UserName);
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditRole(EditRoleViewModel model)
        {
            IdentityRole role =  await _roleManager.FindByIdAsync(model.Id);

            if(role == null)
            {
                ViewBag.ErrorMessage = $"角色Id={model.Id}不存在";
                return View("NotFound");
            }
            else
            {
                role.Name = model.RoleName;

                var result = await _roleManager.UpdateAsync(role);
                if (result.Succeeded)
                {
                    return RedirectToAction("RoleList");
                }
                
                foreach(var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }

        }
        #endregion
    }
}
