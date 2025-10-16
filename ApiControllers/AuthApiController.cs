using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;

using PetRescue.ApiDTOs;
using PetRescue.Models;
using PetRescue.Services;


namespace PetRescue.ApiControllers;

[ApiController]
[Route("api/auth")]
public class AuthApiController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ClientService _clientService;

    public AuthApiController(UserManager<User> userManager, SignInManager<User> signInManager, ClientService clientService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _clientService = clientService;
    }


    [HttpPost]
    [Route("login")]
    public async Task<ActionResult<string>> Login(LoginInputDTO loginInput)
    {
        var user = await _userManager.FindByEmailAsync(loginInput.Email);
        if (user == null)
        {
            return Unauthorized();
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, loginInput.Password, true);
        if (!result.Succeeded)
        {
            return Unauthorized();
        }

        var jwtSecToken = CreateJWT(user);
        if (jwtSecToken == string.Empty)
        {
            return Unauthorized();
        }

        return jwtSecToken;
    }

    [HttpPost]
    [Route("signup")]
    public async Task<ActionResult<string>> Signup(RegisterInputDTO signupInput)
    {
        var newUser = new User { UserName = signupInput.Email, Email = signupInput.Email, Role = UserRole.USER };
        var result = await _userManager.CreateAsync(newUser, signupInput.Password);
        if (!result.Succeeded)
        {
            return BadRequest();
        }

        // Create a Client for the New User
        var createdUser = await _userManager.FindByEmailAsync(newUser.Email);
        if (createdUser == null)
        {
            return BadRequest();
        }

        var newClient = new Client
        {
            Name = string.IsNullOrEmpty(createdUser.Name) ? createdUser.Email! : createdUser.Name,
            UserID = createdUser.Id
        };
        var createdClient = _clientService.CreateClient(newClient);
        if (createdClient == null)
        {
            return BadRequest();
        }

        var jwtSecToken = CreateJWT(newUser);
        if (jwtSecToken == string.Empty)
        {
            return BadRequest();
        }

        return jwtSecToken;
    }

    [HttpPost]
    [Route("forgot_password")]
    public async Task<ActionResult> ForgotPassword(ForgotPasswordInputDTO forgotPasswordInput)
    {
        var user = await _userManager.FindByEmailAsync(forgotPasswordInput.Email);
        if (user == null)
        {
            // Do not reveal here that user does not exist
            return Ok();
        }

        var generatedCode = await _userManager.GeneratePasswordResetTokenAsync(user);
        // Encode to avoid having special characters in the Password Reset Code
        byte[] generatedCodeBytes = Encoding.UTF8.GetBytes(generatedCode);
        var pwdResetCode = WebEncoders.Base64UrlEncode(generatedCodeBytes);

        var pwdResetUrl = Url.Action(nameof(ResetPassword), "AuthApi", new { pwdResetCode });

        Console.WriteLine("Password reset URL = ");
        Console.WriteLine(pwdResetUrl);
        return Ok();
    }

    [HttpPost]
    [Route("reset_password")]
    public async Task<ActionResult> ResetPassword(ResetPasswordInputDTO resetPasswordInput)
    {
        var user = await _userManager.FindByEmailAsync(resetPasswordInput.Email);
        if (user == null)
        {
            // Do not reveal here that user does not exist
            return Ok();
        }

        var decodedCodeBytes = WebEncoders.Base64UrlDecode(resetPasswordInput.ResetCode);
        var decodedResetCode = Encoding.UTF8.GetString(decodedCodeBytes);

        var result = await _userManager.ResetPasswordAsync(user, decodedResetCode, resetPasswordInput.Password);
        if (!result.Succeeded)
        {
            return BadRequest();
        }

        return Ok();
    }


    private string CreateJWT(User userInfo)
    {
        var claims = new[] {
            new Claim(JwtRegisteredClaimNames.Sub, userInfo.Id),
            new Claim(JwtRegisteredClaimNames.Email, userInfo.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var envSecretKey = Environment.GetEnvironmentVariable("SECRET_KEY");
        if (envSecretKey == null)
        {
            return string.Empty;
        }

        var secretKey = new SymmetricSecurityKey(Encoding.UTF32.GetBytes(envSecretKey));
        var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        var jwtOptions = new JwtSecurityToken(
            issuer: "http://localhost:5128",
            audience: "http://localhost:5128",
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: signingCredentials
        );

        var jwtString = new JwtSecurityTokenHandler().WriteToken(jwtOptions);
        return jwtString;
    }
}
