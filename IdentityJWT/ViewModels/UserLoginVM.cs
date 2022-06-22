using System.ComponentModel.DataAnnotations;

namespace IdentityJWT.ViewModels
{
    public class UserLoginVM
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}
