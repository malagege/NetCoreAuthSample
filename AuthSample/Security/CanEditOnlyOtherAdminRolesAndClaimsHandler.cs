using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AuthSample.Security
{
    /// <summary>
    /// 只有編輯其他Admin角色和聲明的處理程序
    /// </summary>
    public class CanEditOnlyOtherAdminRolesAndClaimsHandler : AuthorizationHandler<ManageAdminRolesAndClaimsRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CanEditOnlyOtherAdminRolesAndClaimsHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
          ManageAdminRolesAndClaimsRequirement requirement)
        {
            // 獲取httpContext上下文
            HttpContext httpContext = _httpContextAccessor.HttpContext;

            string loggedInAdminId =
                context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            string adminIdBeingEdited = _httpContextAccessor.HttpContext.Request.Query["userId"];

            //判斷用戶是Admin色，並且擁有claim.Type == "Edit Role"且值為true。
            if (context.User.IsInRole("Admin") &&
                context.User.HasClaim(claim => claim.Type == "EditUser" && claim.Value == true.ToString()))
            {
                //如果當前擁有admin角色的userid為空，說明進入的是角色列表頁面。無須判斷當前當前登錄用戶的id
                if (string.IsNullOrEmpty(adminIdBeingEdited))
                {
                    context.Succeed(requirement);
                }
                else if (adminIdBeingEdited.ToLower() != loggedInAdminId.ToLower())
                {
                    //表示成功滿足需求
                    context.Succeed(requirement);
                }
            }
            return Task.CompletedTask;
        }
    }
}
