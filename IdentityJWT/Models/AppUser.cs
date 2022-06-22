using System;
using Microsoft.AspNetCore.Identity;

namespace IdentityJWT.Models
{
    public class AppUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string RefreshToken { get; set; }
        public DateTime? RefreshTokenEndDate { get; set; }
    }
}
