using System.ComponentModel.DataAnnotations;


namespace PetRescue.ViewModels;

public class ForgotPasswordViewModel
{
    [Required]
    public string Email { get; set; } = default!;
}
