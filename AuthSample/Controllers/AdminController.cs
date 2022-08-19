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
        public async Task<IActionResult> EditRoleAsync(EditRoleViewModel model)
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

        #region 角色加入使用者設定
        public async Task<IActionResult> EditUserInRoleAsync(string roleId)
        {
            ViewBag.roleId = roleId;

            IdentityRole role = await _roleManager.FindByIdAsync(roleId);
            if(role == null)
            {
                ViewBag.ErrorMessage = $"角色{roleId}不存在";
                return View("NotFound");
            }
            var model = new List<UserRoleViewModel>();
            var users = await _userManager.Users.ToListAsync();
            foreach (var user in users)
            {
                var userRoleViewModel = new UserRoleViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                };
                // 判斷使用者有無此角色
                // 注意第二個參數是 role.Name
                var isInRole = await _userManager.IsInRoleAsync(user, role.Name);

                if (isInRole)
                {
                    userRoleViewModel.IsSelected = true;
                }
                else
                {
                    userRoleViewModel.IsSelected = false;
                }
                model.Add(userRoleViewModel);
            }
            
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUserInRoleAsync(IList<UserRoleViewModel> model, string roleId)
        {
            IdentityRole role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"角色id={roleId}不存在";
                return View("NotFound");
            }

            for(int i=0; i<model.Count; i++)
            {
                ApplicationUser user = await _userManager.FindByIdAsync(model[i].UserId);

                IdentityResult result;
                // ***重點***  Start
                // 選擇跟查詢結果不一樣，更改資料
                bool isInRole = await _userManager.IsInRoleAsync(user, role.Name);
                if (model[i].IsSelected && !(isInRole))
                {
                    result = await _userManager.AddToRoleAsync(user, role.Name);   // 使用者角色新增 role.Name
                }
                else if (!model[i].IsSelected && (isInRole))
                { 
                    result = await _userManager.RemoveFromRoleAsync(user, role.Name);   // 使用者角色移除 role.Name
                }
                else
                {
                    continue;     //與資料庫一樣不做處理
                }

                if (result.Succeeded)
                {
                    if(i < (model.Count - 1))  //簡單來說，第一次和中間次數跑 continue 執行最後一次會跑 else 內容
                    {
                        continue;
                    }
                    else
                    {
                        return RedirectToAction("EditRole", new { Id = roleId });
                    }
                }
                // ***重點***  End

                foreach(var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

            }

            //return View(model); 這邊要指定一個 roleId
            return RedirectToAction("EditRole", new { Id = roleId });
        }

        #endregion
    }
}
