using System.ComponentModel.DataAnnotations;

namespace API;

public class DeleteDto
{
    [Required]
    public string Password { get; set; }

}
