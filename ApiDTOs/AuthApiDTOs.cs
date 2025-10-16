namespace PetRescue.ApiDTOs;

public class LoginInputDTO
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public bool RememberMe { get; set; }
}

public class RegisterInputDTO
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string ConfirmPassword { get; set; } = default!;
}

public class ForgotPasswordInputDTO
{
    public string Email { get; set; } = default!;
}

public class ResetPasswordInputDTO
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string ConfirmPassword { get; set; } = default!;
    public string ResetCode { get; set; } = "";
}
