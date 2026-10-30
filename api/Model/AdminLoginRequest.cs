using System.ComponentModel.DataAnnotations;

namespace api.Models
{
    public class AdminLoginRequest
    {
        [Required]
    [StringLength(100)]
        public string Username { get; set; }
        [Required]
        [StringLength(100)]
        public string Password { get; set; }
        }
    }