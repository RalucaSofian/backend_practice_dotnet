using System.ComponentModel.DataAnnotations;


namespace PetRescue.ViewModels;

public class LoginViewModel
{
    [Required]
    public string Email { get; set; } = default!;

    [Required]
    public string Password { get; set; } = default!;

    [Display(Name = "Remember me?")]
    public bool RememberMe { get; set; }
}
