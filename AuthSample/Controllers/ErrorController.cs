using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AuthSample.Controllers
{
    public class ErrorController : Controller
    {
        private readonly ILogger _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        [AllowAnonymous]
        public IActionResult Error()
        {
            // 獲取異常詳細資訊
            IExceptionHandlerPathFeature exceptionHandlerPathFeature = 
                HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            _logger.LogError($"路徑: {exceptionHandlerPathFeature.Path} 產生了一個錯誤( {exceptionHandlerPathFeature.Error})");
            return View("Error");
        }
    }
}
