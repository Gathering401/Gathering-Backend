using System.ComponentModel.DataAnnotations;

namespace GatheringAPI.Models.Api
{
    public class LoginData
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
