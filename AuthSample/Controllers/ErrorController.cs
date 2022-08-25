using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AuthSample.Controllers
{
    [AllowAnonymous]
    public class ErrorController : Controller
    {
        private readonly ILogger _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        //使用屬性路由，如果狀態代碼為404，則路徑將變為Error/404
        [Route("Error/{statusCode}")]
        [HttpGet]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            var statusCodeResult =
                HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            switch (statusCode)
            {
                case 404:
                    ViewBag.ErrorMessage = "抱歉，你訪問的頁面不存在";
                    //LogWarning() 方法將異常記錄作為日誌中的警告類別記錄
                    _logger.LogWarning($"發生了一個404錯誤. 路徑 = " +
                $"{statusCodeResult.OriginalPath} 以及查詢字符串 = " +
                $"{statusCodeResult.OriginalQueryString}");
                    break;
            }
            return View("NotFound");
        }

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
