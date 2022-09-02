using AuthSample.Models;
using AuthSample.ViewModels.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AuthSample.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        public IActionResult AccessDenied()
        {
            return View();
        }


        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> LoginAsync(string returnUrl)
        {
            LoginViewModel model = new()
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult ExternalLogin(string provider, string returnUri)
        {
            // 登入後反回 Callback Url 有些地方叫 RedirectUrl
            string redirectUrl = Url.Action("ExternalLoginCallback", "Account",
                new { ReturnUri= returnUri });
            // _signInManger.ConfigureExternalAuthenticationProperties 放 provider 和redirectUrl 參數
            AuthenticationProperties properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            // ChallengeResult(string authenticationScheme, AuthenticationProperties properties); 執行程式
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult>
            ExternalLoginCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            returnUrl ??= Url.Content("~/");

            LoginViewModel loginViewModel = new()
            {
                ReturnUrl = returnUrl,
                ExternalLogins =
                        (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };

            if (remoteError != null)
            {
                ModelState
                    .AddModelError(string.Empty, $"外部提供程式錯誤: {remoteError}");

                return View("Login", loginViewModel);
            }

            // 從外部登錄提供者,即微軟賬戶體系中，獲取關於帳號的登錄信息。
            ExternalLoginInfo info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ModelState
                    .AddModelError(string.Empty, "加載外部登錄資訊出錯。");

                return View("Login", loginViewModel);
            }

            //如果用戶之前已經登錄過了，會在AspNetUserLogins表有對應的記錄，這個時候無需創建新的記錄，直接使用當前記錄登錄系統即可。
            Microsoft.AspNetCore.Identity.SignInResult signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider,
                info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (signInResult.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }

            //如果AspNetUserLogins表中沒有記錄，則代表用戶沒有一個本地帳戶，這個時候我們就需要創建一個記錄了。       
            else
            {
                // 獲得使用者 Email 資訊
                Claim email = info.Principal.FindFirst(ClaimTypes.Email);


                if (email != null)
                {
                    // 透過 Email 來看使用者是否存在
                    ApplicationUser user = await _userManager.FindByEmailAsync(email.Value);

                    if (user == null)
                    {
                        user = new ApplicationUser
                        {
                            UserName = info.Principal.FindFirstValue(ClaimTypes.Email),
                            Email = info.Principal.FindFirstValue(ClaimTypes.Email)
                        };
                        //如果不存在，則創建一個用戶，但是這個用戶沒有密碼。
                        await _userManager.CreateAsync(user);
                    }

                    // 在AspNetUserLogins表中,添加一行用戶數據，然後將當前用戶登錄到系統中
                    await _userManager.AddLoginAsync(user, info);
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    return LocalRedirect(returnUrl);
                }

                // 如果我們獲取不到電子郵件地址，我們需要將請求重定向到錯誤視圖中。
                ViewBag.ErrorTitle = $"我們無法從提供商:{info.LoginProvider}中解析到您的郵件地址 ";
                ViewBag.ErrorMessage = "請通過聯系尋求技術支持。";

                return View("Error");
            }
        }

        /// <summary>
        /// 使用者登入
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> LoginAsync(LoginViewModel model, string returnUrl)
        {
            // 補上登入頁後的登入資訊
            model.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                ApplicationUser user = await _userManager.FindByEmailAsync(model.Email);

                // 沒有這一個判斷會走到「登入失敗，請重試」，注意這邊要做這個判斷，使用者才知道是什麼錯誤。
                if (user != null & !user.EmailConfirmed &&
                    await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    ModelState.AddModelError(string.Empty, "你的信箱尚為進行驗證");
                    return View(model);
                }

                Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(returnUrl))
                    {
                        if (Url.IsLocalUrl(returnUrl))      //安全性驗證 returnUrl
                        {
                            return Redirect(returnUrl);
                        }
                    }
                    else
                    {
                        return RedirectToAction("index", "home"); // 登入後導回收頁
                    }
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
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    City = model.City,
                };

                var result = await _userManager.CreateAsync(user, model.Password);  // 第二個參數密碼要放，沒放不會檢核密碼規則

                if (result.Succeeded)
                {
                    //當前 admin 新增帳號，導回使用者清單
                    if(_signInManager.IsSignedIn(User) && User.IsInRole("Admin"))
                    {
                        return RedirectToAction("userlist", "admin");
                    }

                    // isPersistent: 指出登入 Cookie 是否應該在關閉瀏覽器之後保存       參考: [SignInManager<TUser>.SignInAsync 方法 (Microsoft.AspNetCore.Identity) | Microsoft Docs](https://docs.microsoft.com/zh-tw/dotnet/api/microsoft.aspnetcore.identity.signinmanager-1.signinasync?view=aspnetcore-6.0)
                    await _signInManager.SignInAsync(user, isPersistent: false); 
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
            await _signInManager.SignOutAsync();
            return RedirectToAction("index","home");
        }
    }
}
