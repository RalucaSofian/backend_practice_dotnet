using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using PetRescue.Data;
using PetRescue.Models;
using PetRescue.Utilities;


namespace PetRescue.Controllers;

[Authorize]
[Route("admin/users")]
public class UsersController : Controller
{
    private readonly PetRescueContext _context;
    private readonly UserManager<User> _userManager;

    public UsersController(PetRescueContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: users
    [Route("")]
    public async Task<IActionResult> Index(string searchString, UserRole? userRole, string sortOrder,
                                           int pageSize = 6, int pageNumber = 1)
    {
        var finalUserObjects = from u in _context.Users select u;

        // Searching
        ViewData["SearchString"] = searchString;
        if (!string.IsNullOrEmpty(searchString))
        {
            var upperSearchString = searchString.ToUpper();
            finalUserObjects = finalUserObjects.Where(u =>
                u.Email!.ToUpper().Contains(upperSearchString) ||
                u.Name!.ToUpper().Contains(upperSearchString));
        }

        // Filtering
        ViewData["UserRoleOptions"] = new SelectList(new List<UserRole> { UserRole.ADMIN, UserRole.USER });
        if (userRole != null)
        {
            finalUserObjects = finalUserObjects.Where(u => u.Role == userRole);
            ViewData["UserRoleFilter"] = userRole;
        }

        // Ordering
        if (!string.IsNullOrEmpty(sortOrder))
        {
            switch (sortOrder)
            {
                case "email_asc":
                    finalUserObjects = finalUserObjects.OrderBy(u => u.Email);
                    break;
                case "email_desc":
                    finalUserObjects = finalUserObjects.OrderByDescending(u => u.Email);
                    break;
                case "name_asc":
                    finalUserObjects = finalUserObjects.OrderBy(u => u.Name);
                    break;
                case "name_desc":
                    finalUserObjects = finalUserObjects.OrderByDescending(u => u.Name);
                    break;
                case "role_asc":
                    finalUserObjects = finalUserObjects.OrderBy(u => u.Role);
                    break;
                case "role_desc":
                    finalUserObjects = finalUserObjects.OrderByDescending(u => u.Role);
                    break;
                default:
                    finalUserObjects = finalUserObjects.OrderBy(u => u.Email);
                    break;
            }
        }

        if (string.IsNullOrEmpty(sortOrder) || !sortOrder.StartsWith("email_"))
        {
            ViewData["NextEmailSort"] = "email_asc";
        }
        else if (sortOrder == "email_asc")
        {
            ViewData["NextEmailSort"] = "email_desc";
        }

        if (string.IsNullOrEmpty(sortOrder) || !sortOrder.StartsWith("name_"))
        {
            ViewData["NextNameSort"] = "name_asc";
        }
        else if (sortOrder == "name_asc")
        {
            ViewData["NextNameSort"] = "name_desc";
        }

        if (string.IsNullOrEmpty(sortOrder) || !sortOrder.StartsWith("role_"))
        {
            ViewData["NextRoleSort"] = "role_asc";
        }
        else if (sortOrder == "role_asc")
        {
            ViewData["NextRoleSort"] = "role_desc";
        }

        ViewData["CrtSortOrder"] = sortOrder;

        // Paging
        ViewData["CrtPage"] = pageNumber;
        return View(await PaginatedList<User>.CreateAsyncList(finalUserObjects.AsNoTracking(), pageNumber, pageSize));
    }

    // GET: users/{id}
    [Route("{id}")]
    public async Task<IActionResult> Details(string? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var usersModel = await _context.Users
            .FirstOrDefaultAsync(m => m.Id == id);
        if (usersModel == null)
        {
            return NotFound();
        }

        return View(usersModel);
    }

    // GET: users/create
    [Route("create")]
    public IActionResult Create()
    {
        return View();
    }

    // POST: users/create
    [Route("create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Email,Role,Name")] User usersModel)
    {
        if (ModelState.IsValid)
        {
            usersModel.UserName = usersModel.Email;

            var createResult = await _userManager.CreateAsync(usersModel);
            if (createResult.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View(usersModel);
            }
        }
        return View(usersModel);
    }

    // GET: users/edit/{id}
    [Route("edit/{id}")]
    public async Task<IActionResult> Edit(string? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var usersModel = await _context.Users.FindAsync(id);
        if (usersModel == null)
        {
            return NotFound();
        }
        return View(usersModel);
    }

    // POST: users/edit/{id}
    [Route("edit/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, [Bind("Id,Email,Role,Name")] User usersModel)
    {
        if (id != usersModel.Id)
        {
            return NotFound();
        }

        bool emailValid = (ModelState["Email"] is not null) && (ModelState["Email"]?.ValidationState == ModelValidationState.Valid);
        bool roleValid = (ModelState["Role"] is not null) && (ModelState["Role"]?.ValidationState == ModelValidationState.Valid);
        bool nameValid = (ModelState["Name"] is not null) && (ModelState["Name"]?.ValidationState == ModelValidationState.Valid);

        if (emailValid && roleValid && nameValid)
        {
            try
            {
                _context.Attach(usersModel);
                _context.Entry(usersModel).Property("Email").IsModified = true;
                _context.Entry(usersModel).Property("Role").IsModified = true;
                _context.Entry(usersModel).Property("Name").IsModified = true;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsersModelExists(usersModel.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(usersModel);
    }

    // POST: users/edit/{id}/forgot_password
    [Route("edit/{id}/forgot_password")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(string id, [Bind("Id,Email,Role,Name")] User usersModel)
    {
        if (id != usersModel.Id)
        {
            return NotFound();
        }

        try
        {
            await _userManager.RemovePasswordAsync(usersModel);
            string resetToken = await _userManager.GeneratePasswordResetTokenAsync(usersModel);
            // Send an email with this token to the user
            Console.WriteLine("Password Reset Token");
            Console.WriteLine(resetToken);

            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!UsersModelExists(usersModel.Id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }
        return RedirectToAction(nameof(Index));
    }

    // GET: users/delete/{id}
    [Route("delete/{id}")]
    public async Task<IActionResult> Delete(string? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var usersModel = await _context.Users
            .FirstOrDefaultAsync(m => m.Id == id);
        if (usersModel == null)
        {
            return NotFound();
        }

        return View(usersModel);
    }

    // POST: users/delete/{id}
    [Route("delete/{id}")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var usersModel = await _context.Users.FindAsync(id);
        if (usersModel != null)
        {
            _context.Users.Remove(usersModel);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool UsersModelExists(string id)
    {
        return _context.Users.Any(e => e.Id == id);
    }
}
