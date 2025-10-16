using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

using PetRescue.Models;
using PetRescue.Services;


namespace PetRescue.Controllers;

[Authorize]
[Route("admin/clients")]
public class ClientController : Controller
{
    private readonly ClientService _clientService;
    private readonly UserService _userService;

    public ClientController(ClientService clientService, UserService userService)
    {
        _clientService = clientService;
        _userService = userService;
    }

    // GET: clients
    [Route("")]
    public async Task<IActionResult> Index(string searchString, bool? hasUser,
                                           string? userId, string sortOrder,
                                           int pageSize = 6, int pageNumber = 1)
    {
        var queryOptions = new ClientService.QueryOptions
        {
            SearchString = string.IsNullOrEmpty(searchString) ? null : searchString,
            SortOrder = string.IsNullOrEmpty(sortOrder) ? null : sortOrder,
            HasUser = hasUser is null ? null : hasUser,
            UserId = string.IsNullOrEmpty(userId) ? null : userId,
            PageSize = pageSize,
            PageNumber = pageNumber
        };
        var finalClientObjects = await _clientService.QueryClients(queryOptions);

        // Searching
        ViewData["SearchString"] = searchString;

        // Filtering
        ViewData["HasUserOptions"] = new SelectList(new List<bool> { true, false });
        if (hasUser != null)
        {
            ViewData["UserSetFilter"] = hasUser;
        }

        var allUsers = await _userService.GetAllUsers();
        ViewData["UserEmailOptions"] = new SelectList(allUsers, "Id", "Email");

        if (!string.IsNullOrEmpty(userId))
        {
            ViewData["UserMailFilter"] = userId;
        }

        // Ordering
        if (string.IsNullOrEmpty(sortOrder) || !sortOrder.StartsWith("id_"))
        {
            ViewData["NextIdSort"] = "id_asc";
        }
        else if (sortOrder == "id_asc")
        {
            ViewData["NextIdSort"] = "id_desc";
        }

        if (string.IsNullOrEmpty(sortOrder) || !sortOrder.StartsWith("name_"))
        {
            ViewData["NextNameSort"] = "name_asc";
        }
        else if (sortOrder == "name_asc")
        {
            ViewData["NextNameSort"] = "name_desc";
        }

        if (string.IsNullOrEmpty(sortOrder) || !sortOrder.StartsWith("addr_"))
        {
            ViewData["NextAddrSort"] = "addr_asc";
        }
        else if (sortOrder == "addr_asc")
        {
            ViewData["NextAddrSort"] = "addr_desc";
        }

        if (string.IsNullOrEmpty(sortOrder) || !sortOrder.StartsWith("user_"))
        {
            ViewData["NextUserSort"] = "user_asc";
        }
        else if (sortOrder == "user_asc")
        {
            ViewData["NextUserSort"] = "user_desc";
        }

        ViewData["CrtSortOrder"] = sortOrder;

        // Paging
        ViewData["CrtPage"] = pageNumber;
        return View(finalClientObjects);
    }

    // GET: clients/{id}
    [Route("{id}")]
    public async Task<IActionResult> Details(int id)
    {
        var client = await _clientService.GetClient(id);
        if (client == null)
        {
            return NotFound();
        }

        return View(client);
    }

    // GET: clients/create
    [Route("create")]
    public async Task<IActionResult> Create()
    {
        var allUsers = await _userService.GetAllUsers();
        ViewBag.UserID = new SelectList(allUsers, "Id", "Email");
        return View();
    }

    // POST: clients/create
    [Route("create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Name,Address,Phone,Description,UserID")] Client client)
    {
        if (ModelState.IsValid)
        {
            var createdClient = await _clientService.CreateClient(client);
            if (createdClient != null)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View(client);
            }
        }
        return View(client);
    }

    // GET: clients/edit/{id}
    [Route("edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var allUsers = await _userService.GetAllUsers();
        ViewBag.UserID = new SelectList(allUsers, "Id", "Email");

        var client = await _clientService.GetClient(id);
        if (client == null)
        {
            return NotFound();
        }
        return View(client);
    }

    // POST: clients/edit/{id}
    [Route("edit/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Address,Phone,Description,UserID")] Client client)
    {
        if (id != client.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var editedClient = await _clientService.EditClient(client);
            if (editedClient != null)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View(client);
            }
        }

        return View(client);
    }

    // GET: clients/delete/{id}
    [Route("delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var client = await _clientService.GetClient(id);
        if (client == null)
        {
            return NotFound();
        }

        return View(client);
    }

    // POST: clients/delete/{id}
    [Route("delete/{id}")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _clientService.DeleteClient(id);
        return RedirectToAction(nameof(Index));
    }
}
