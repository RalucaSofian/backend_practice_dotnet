using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PetRescue.Models;
using PetRescue.ViewModels;


namespace PetRescue.Controllers;

[Route("admin")]
public class AdminController : Controller
{
    private readonly ILogger<AdminController> _logger;
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public AdminController(ILogger<AdminController> logger, UserManager<User> userManager, SignInManager<User> signInManager)
    {
        _logger = logger;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [Route("")]
    public IActionResult Index()
    {
        return View();
    }

    [Route("welcome")]
    public IActionResult Welcome()
    {
        return View();
    }

    [Route("privacy")]
    public IActionResult Privacy()
    {
        return View();
    }

    [Route("login")]
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Login(string returnUrl = "/")
    {
        // Signout used for clearing external cookies (ensure clean login)
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [Route("login")]
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel loginModel, string returnUrl = "/")
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(loginModel.Email, loginModel.Password, loginModel.RememberMe, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                _logger.LogInformation("[AdminController] User successfully logged in");
                return Redirect(returnUrl);
            }
            else if (result.IsLockedOut)
            {
                _logger.LogWarning("[AdminController] User locked out of account");
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                _logger.LogError("[AdminController] Invalid login attept");
                ModelState.AddModelError(string.Empty, "Incorrect Email or Password");
                return View(loginModel);
            }
        }
        return View(loginModel);
    }

    [Route("lockout")]
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Lockout()
    {
        return View();
    }

    [Route("register")]
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register(string returnUrl = "/")
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [Route("register")]
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel registerModel, string returnUrl = "/")
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (ModelState.IsValid)
        {
            var newUser = new User { UserName = registerModel.Email, Email = registerModel.Email, Role = UserRole.ADMIN };
            var result = await _userManager.CreateAsync(newUser, registerModel.Password);
            if (result.Succeeded)
            {
                _logger.LogInformation("[AdminController] User created a new account");
                await _signInManager.SignInAsync(newUser, isPersistent: false);

                return Redirect(returnUrl);
            }
            ModelState.AddModelError(string.Empty, result.Errors.ToString()!);
        }

        return View(registerModel);
    }

    [Route("logout")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("[AdminController] User logged out");
        return RedirectToAction(nameof(Index));
    }

    [Route("forgot_password")]
    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [Route("forgot_password")]
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel forgotPasswordModel)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(forgotPasswordModel.Email);
            if (user == null)
            {
                // Do not reveal here that user does not exist
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            var resetCode = await _userManager.GeneratePasswordResetTokenAsync(user);
            var passwResetUrl = Url.Action(nameof(ResetPassword), "Admin", new { resetCode });
            _logger.LogInformation("[AdminController] Password reset URL = ");
            _logger.LogInformation(passwResetUrl);

            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        return View();
    }

    [Route("forgot_password_confirmation")]
    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPasswordConfirmation()
    {
        return View();
    }

    [Route("reset_password")]
    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword(string resetCode)
    {
        if (resetCode == "")
        {
            _logger.LogError("[AdminController] Incorrect Password Reset Code");
            ModelState.AddModelError(string.Empty, "Incorrect Reset Code");
        }
        else
        {
            var resetModel = new ResetPasswordViewModel { ResetCode = resetCode };
            return View(resetModel);
        }

        return View();
    }

    [Route("reset_password")]
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel resetPasswordModel)
    {
        if (!ModelState.IsValid)
        {
            return View(resetPasswordModel);
        }

        var user = await _userManager.FindByEmailAsync(resetPasswordModel.Email);
        if (user == null)
        {
            // Do not reveal here that user does not exist
            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }

        var result = await _userManager.ResetPasswordAsync(user, resetPasswordModel.ResetCode, resetPasswordModel.Password);
        if (result.Succeeded)
        {
            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }
        ModelState.AddModelError(string.Empty, result.Errors.ToString()!);

        return View();
    }

    [Route("reset_password_confirmation")]
    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPasswordConfirmation()
    {
        return View();
    }


    [Route("error")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
