using System.ComponentModel.DataAnnotations;

namespace Shared.Models
{
    public class LoginViewModel
    {
        [Required]
        public required string Username { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Heslo musí mít alespoň 6 znaků.")]
        public required string Password { get; set; }
    }
}
