using Microsoft.AspNetCore.Authorization;

namespace AuthSample.Security
{
    /// <summary>
    /// 管理Admin角色與聲明的授權需求
    /// </summary>
    public class ManageAdminRolesAndClaimsRequirement : IAuthorizationRequirement
    {
    }
}