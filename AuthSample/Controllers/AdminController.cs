using AuthSample.Models;
using AuthSample.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AuthSample.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AdminController> _logger;

        public AdminController(RoleManager<IdentityRole> roleManager,UserManager<ApplicationUser> userManager,ILogger<AdminController> logger)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _logger = logger;
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

        #region 刪除角色
        [HttpPost]
        public async Task<IActionResult> DeleteRoleAsync(string id)
        {
            IdentityRole role = await _roleManager.FindByIdAsync(id);
            if(role == null)
            {
                ViewBag.ErrorMessage = $"無法找到ID為{id}的角色信息";
            }
            else
            {
                try
                {
                    IdentityResult result = await _roleManager.DeleteAsync(role);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("RoleList");
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError($"發生異常: {ex}");
                    ViewBag.ErrorTitle = $"角色{role.Name} 正在被使用中...";
                    ViewBag.ErrorMessage = $"無法刪除{role.Name}角色，因此角色中己存在帳號，如果想刪除此角色，請移除該角色帳號資料。";
                    return View("Error");
                }

            }
            return RedirectToAction("RoleList");

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

        [Authorize(Roles = "admin")]
        public string TestCheckAdminRole()
        {
            return "Hello, Admin";
        }

        #region 使用者管理
        public async Task<IActionResult> UserListAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> EditUserAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if(user == null)
            {
                ViewBag.ErrorMessage = $"無法找到ID為{id}的使用者";
                return View("NotFound");
            }
            // 取得使用者聲明(claims)
            IList<Claim> userClaims = await _userManager.GetClaimsAsync(user);
            IList<string> userRole = await _userManager.GetRolesAsync(user);

            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                City = user.City,
                Claims = userClaims,
                Roles = userRole,
            };

            return View(model);

        }

        public async Task<IActionResult> EditUserAsync(EditUserViewModel model) 
        {
            ApplicationUser user = await _userManager.FindByIdAsync(model.Id);

            if(user == null)
            {
                ViewBag.ErrorMessage = $"無法找到ID為{model.Id}的使用者";
                return View("NotFound");
            }
            else
            {
                user.Email = model.Email;
                user.UserName = model.UserName;
                user.City = model.City;

                IdentityResult result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("userlist");
                }

                foreach(IdentityError error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUserAsync(string id)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(id);

            if( user == null)
            {
                ViewBag.ErrorMessage = $"無法找到ID為{id}的使用者";
                return View("NotFound");
            }
            else
            {
                IdentityResult result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("UserList");
                }
                foreach(IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return RedirectToAction("UserList");
            }
        }
        #endregion

        #region 使用者設定角色
        [HttpGet]
        public async Task<IActionResult> ManagerUserRolesAsync(string userId)
        {
            ViewBag.userId = userId;
            ApplicationUser user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"無法找到 ID 為 {userId} 的使用者";
                return View("NotFound");
            }
            var model = new List<RolesInUserViewModel>();

            foreach(IdentityRole role in _roleManager.Roles)
            {
                var roleInUserViewModel = new RolesInUserViewModel()
                {
                    RoleId = role.Id,
                    RoleName = role.Name,
                };

                if(await _userManager.IsInRoleAsync(user, role.Name))
                {
                    roleInUserViewModel.IsSelected = true;
                }
                else
                {
                    roleInUserViewModel.IsSelected = false;
                }

                model.Add(roleInUserViewModel);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ManagerUserRolesAsync(IList<RolesInUserViewModel> model, string userId)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(userId);

            if(user == null)
            {
                ViewBag.ErrorMessage = $"無法找到 ID 為 {userId} 的使用者";
                return View("NotFound");
            }
            IList<string> roles = await _userManager.GetRolesAsync(user);

            // 移除當下 User 所有角色
            IdentityResult result = await _userManager.RemoveFromRolesAsync(user, roles);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "無法刪除使用者角色資料");
                return View(model);
            }
            // 查詢模型列表中被選中的 RoleName 并添加到使用者中
            result = await _userManager.AddToRolesAsync(user, model.Where( m => m.IsSelected).Select( m => m.RoleName));
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "無法向使用者添加選定的角色");
                return View(model);
            }

            return RedirectToAction("EditUser", new { Id = userId });
        }
        #endregion
        #region 角色管理聲明
        [HttpGet]
        public async Task<IActionResult> ManagerUserClaimsAsync(string userId)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(userId);
            if(user == null)
            {
                ViewBag.ErrorMessage = $"無法找到 ID 為{userId} 的使用者";
                return View("NotFound");
            }

            // UserManager 服務中的 GetClaimAsync() 方法取得使用者當前所有聲明
            IList<Claim> existingUserClaims = await _userManager.GetClaimsAsync(user);
            var model = new UserClaimsViewModel
            {
                UserId = userId
            };
            foreach(Claim claim in ClaimsStore.AllClaims)
            {
                UserClaim userClaim = new()
                {
                    ClaimType = claim.Type,
                };

                // 如果使用者選中了聲明屬性，則設置 IsSelected 屬性為 true
                // 這邊反思是否要做判斷 ClaimValue
                if(existingUserClaims.Any(c => c.Type == claim.Type && c.Value == "True"))
                {
                    userClaim.IsSelected = true;
                }

                model.Claims.Add(userClaim);
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ManagerUserClaimsAsync(UserClaimsViewModel model)
        {
            ApplicationUser user = await _userManager.FindByIdAsync(model.UserId);

            if(user == null)
            {
                ViewBag.ErrorMessage = $"無法找到 ID 為 {model.UserId} 的使用者";
                return View("NotFound");
            }

            // 獲取所有使用者現有的聲明并刪除它們
            IList<Claim> claims = await _userManager.GetClaimsAsync(user);
            IdentityResult result = await _userManager.RemoveClaimsAsync(user, claims);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "無法刪除當前用戶的聲明");
                return View(model);
            }

            // 添加頁面上選中的所有聲明資訊
            result = await _userManager.AddClaimsAsync(user, 
                model.Claims
                    .Select( c => new Claim(c.ClaimType, c.IsSelected.ToString() , ClaimValueTypes.Boolean))
                  );
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "無法向使用者添加選定的聲明");
                return View(model);
            }

            return RedirectToAction("EditUser", new { Id = model.UserId });
        }
        #endregion
    }
}
