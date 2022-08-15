using Microsoft.AspNetCore.Identity;

namespace AuthSample.CustomMiddlewares
{
    /// <summary>
    /// 參考:[Identity-修改Error錯誤提示為中文 - zh1990 - 博客園](https://www.cnblogs.com/zh1990/p/6169871.html)
    /// </summary>
    public class CustomIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError DefaultError() { return new IdentityError { Code = nameof(DefaultError), Description = $"未知錯誤！" }; }
        public override IdentityError ConcurrencyFailure() { return new IdentityError { Code = nameof(ConcurrencyFailure), Description = "並發錯誤，對象已被修改！" }; }
        public override IdentityError PasswordMismatch() { return new IdentityError { Code = "Password", Description = "密碼錯誤！" }; }
        public override IdentityError InvalidToken() { return new IdentityError { Code = nameof(InvalidToken), Description = "無效 token." }; }
        public override IdentityError LoginAlreadyAssociated() { return new IdentityError { Code = nameof(LoginAlreadyAssociated), Description = "當前用戶已經登錄！" }; }
        public override IdentityError InvalidUserName(string userName) { return new IdentityError { Code = "UserName", Description = $"用戶名 '{userName}' 錯誤，只可以包含數字和字母！" }; }
        public override IdentityError InvalidEmail(string email) { return new IdentityError { Code = "Email", Description = $"信箱 '{email}' 格式錯誤！" }; }
        public override IdentityError DuplicateUserName(string userName) { return new IdentityError { Code = "UserName", Description = $"帳號 '{userName}' 已存在！" }; }
        public override IdentityError DuplicateEmail(string email) { return new IdentityError { Code = "Email", Description = $"信箱 '{email}' 已經存在！" }; }
        public override IdentityError InvalidRoleName(string role) { return new IdentityError { Code = nameof(InvalidRoleName), Description = $"角色 '{role}' 驗證錯誤！" }; }
        public override IdentityError DuplicateRoleName(string role) { return new IdentityError { Code = nameof(DuplicateRoleName), Description = $"角色名 '{role}' 已經存在！" }; }
        public override IdentityError UserAlreadyHasPassword() { return new IdentityError { Code = nameof(UserAlreadyHasPassword), Description = "此帳號設定密碼了" }; }
        public override IdentityError UserLockoutNotEnabled() { return new IdentityError { Code = nameof(UserLockoutNotEnabled), Description = "此帳號未鎖定" }; }
        public override IdentityError UserAlreadyInRole(string role) { return new IdentityError { Code = nameof(UserAlreadyInRole), Description = $"使用者已關聯角色 '{role}'." }; }
        public override IdentityError UserNotInRole(string role) { return new IdentityError { Code = nameof(UserNotInRole), Description = $"使用者沒有關聯角色  '{role}'." }; }
        public override IdentityError PasswordTooShort(int length) { return new IdentityError { Code = "Password", Description = $"密碼至少 {length} 位！" }; }
        public override IdentityError PasswordRequiresNonAlphanumeric() { return new IdentityError { Code = "Password", Description = "密碼必須至少有一個非字母數字字符." }; }
        public override IdentityError PasswordRequiresDigit() { return new IdentityError { Code = "Password", Description = "密碼至少有一個數字 ('0'-'9')." }; }
        public override IdentityError PasswordRequiresLower() { return new IdentityError { Code = "Password", Description = "密碼必須包含小寫字母 ('a'-'z')." }; }
        public override IdentityError PasswordRequiresUpper() { return new IdentityError { Code = "Password", Description = "密碼必須包含大寫字母 ('A'-'Z')." }; }
    }
}
