using System.ComponentModel.DataAnnotations;

namespace Aephy.API.Models
{
    public class LoginModel
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? FCMToken { get; set; }
        public string? Device { get; set; }
        public bool RememberMe { get; set; }
    }
}
