using AuthSample.ViewModels.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AuthSample.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManger;
        private readonly SignInManager<IdentityUser> _signInManger;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManger = userManager;
            _signInManger = signInManager;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        
        /// <summary>
        /// 使用者登入
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> LoginAsync(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManger.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    return RedirectToAction("index", "home"); // 登入後導回收頁
                }

                ModelState.AddModelError(string.Empty, "登入失敗，請重試");
            }
            return View(model);
        }

        /// <summary>
        /// 注冊帳號
        /// 1. ModelState.IsValid 驗證輸入資訊
        /// 2. _userManger.CreateAsync(user, model.Password) 
        /// 3. 設定登入
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> RegisterAsync(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                };

                var result = await _userManger.CreateAsync(user, model.Password);  // 第二個參數密碼要放，沒放不會檢核密碼規則

                if (result.Succeeded)
                {
                    // isPersistent: 指出登入 Cookie 是否應該在關閉瀏覽器之後保存       參考: [SignInManager<TUser>.SignInAsync 方法 (Microsoft.AspNetCore.Identity) | Microsoft Docs](https://docs.microsoft.com/zh-tw/dotnet/api/microsoft.aspnetcore.identity.signinmanager-1.signinasync?view=aspnetcore-6.0)
                    await _signInManger.SignInAsync(user, isPersistent: false); 
                    return RedirectToAction("index", "home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }
        /// <summary>
        /// 帳號登出
        /// 1. 登出
        /// 2. 導回 /home/index
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> LogoutAsync()
        {
            await _signInManger.SignOutAsync();
            return RedirectToAction("index","home");
        }
    }
}
