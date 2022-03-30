using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class RegisterDto
    {
        [Required]
        [StringLength(16)]
        public string Username { get; set; }
        [Required]
        [StringLength(32, MinimumLength = 6)]
        public string Password { get; set; }
    }
}
