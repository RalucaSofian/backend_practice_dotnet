using System.ComponentModel.DataAnnotations;

namespace PetRescue.ViewModels;

public class ResetPasswordViewModel
{
    [Required]
    public string Email { get; set; } = default!;

    [Required]
    public string Password { get; set; } = default!;

    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = default!;

    public string ResetCode { get; set; } = "";
}
