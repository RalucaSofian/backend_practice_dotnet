using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

using PetRescue.Models;
using PetRescue.Services;


namespace PetRescue.Controllers;

[Authorize]
[Route("admin/users")]
public class UsersController : Controller
{
    private readonly UserService _userService;

    public UsersController(UserService userService)
    {
        _userService = userService;
    }

    // GET: users
    [Route("")]
    public async Task<IActionResult> Index(string searchString, UserRole? userRole, string sortOrder,
                                           int pageSize = 6, int pageNumber = 1)
    {
        var finalUserObjects = await _userService.QueryUsers(searchString, userRole, sortOrder, pageSize, pageNumber);

        // Searching
        ViewData["SearchString"] = searchString;

        // Filtering
        ViewData["UserRoleOptions"] = new SelectList(new List<UserRole> { UserRole.ADMIN, UserRole.USER });
        if (userRole != null)
        {
            ViewData["UserRoleFilter"] = userRole;
        }

        // Ordering
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
        return View(finalUserObjects);
    }

    // GET: users/{id}
    [Route("{id}")]
    public async Task<IActionResult> Details(string id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var user = await _userService.GetUser(id);
        if (user == null)
        {
            return NotFound();
        }

        return View(user);
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
    public async Task<IActionResult> Create([Bind("Id,Email,Role,Name")] User user)
    {
        if (ModelState.IsValid)
        {
            user.UserName = user.Email;
            var createdUser = await _userService.CreateUser(user);
            if (createdUser != null)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View(user);
            }
        }
        return View(user);
    }

    // GET: users/edit/{id}
    [Route("edit/{id}")]
    public async Task<IActionResult> Edit(string? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var user = await _userService.GetUser(id);
        if (user == null)
        {
            return NotFound();
        }
        return View(user);
    }

    // POST: users/edit/{id}
    [Route("edit/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, [Bind("Id,Email,Role,Name")] User user)
    {
        if (id != user.Id)
        {
            return NotFound();
        }

        bool emailValid = (ModelState["Email"] is not null) && (ModelState["Email"]?.ValidationState == ModelValidationState.Valid);
        bool roleValid  = (ModelState["Role"] is not null) && (ModelState["Role"]?.ValidationState == ModelValidationState.Valid);
        bool nameValid  = (ModelState["Name"] is not null) && (ModelState["Name"]?.ValidationState == ModelValidationState.Valid);

        if (emailValid && roleValid && nameValid)
        {
            var editedUser = await _userService.EditUser(user);
            if (editedUser != null)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View(user);
            }
        }
        return View(user);
    }

    // POST: users/edit/{id}/forgot_password
    [Route("edit/{id}/forgot_password")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(string id, [Bind("Id,Email,Role,Name")] User user)
    {
        if (id != user.Id)
        {
            return NotFound();
        }

        var editedUser = await _userService.ForgotPassword(user);
        if (editedUser != null)
        {
            return RedirectToAction(nameof(Index));
        }
        else
        {
            return View(user);
        }
    }

    // GET: users/delete/{id}
    [Route("delete/{id}")]
    public async Task<IActionResult> Delete(string? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var user = await _userService.GetUser(id);
        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    // POST: users/delete/{id}
    [Route("delete/{id}")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        await _userService.DeleteUser(id);
        return RedirectToAction(nameof(Index));
    }
}
