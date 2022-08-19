using System.Collections.Generic;

namespace AuthSample.ViewModels.Admin
{
    public class EditRoleViewModel
    {
        public string Id { get; set; }
        public string RoleName { get; set; }
        public IList<string> Users { get; set; }
    }
}