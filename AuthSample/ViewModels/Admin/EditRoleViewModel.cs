using System.Collections.Generic;

namespace AuthSample.ViewModels.Admin
{
    public class EditRoleViewModel
    {
        public EditRoleViewModel()
        {
            // 初始化使用者清單
            Users = new List<string>();
        }
        public string Id { get; set; }
        public string RoleName { get; set; }
        public IList<string> Users { get; set; }
    }
}