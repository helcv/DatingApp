using System.ComponentModel.DataAnnotations;

namespace API;

public class GoogleSignInDTO
{
    [Required]
    public string GoogleToken { get; set; }
    [Required]
    public string Gender { get; set; }
    [Required]
    public DateOnly? DateOfBirth { get; set; }
    [Required]
    public string City { get; set; }
    [Required]
    public string Country { get; set; }
    [Required]
    [StringLength(8, MinimumLength = 4)]
    public string Password { get; set; }

}
