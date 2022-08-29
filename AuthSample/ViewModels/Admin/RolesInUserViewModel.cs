namespace AuthSample.ViewModels.Admin
{
    /// <summary>
    /// 使用者擁有角色列表
    /// </summary>
    public class RolesInUserViewModel
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public bool IsSelected { get; set; }
    }
}
