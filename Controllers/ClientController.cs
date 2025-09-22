using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using PetRescue.Data;
using PetRescue.Models;
using PetRescue.Utilities;


namespace PetRescue.Controllers;

[Authorize]
[Route("admin/clients")]
public class ClientController : Controller
{
    private readonly PetRescueContext _context;

    public ClientController(PetRescueContext context)
    {
        _context = context;
    }

    // GET: clients
    [Route("")]
    public async Task<IActionResult> Index(string searchString, bool? hasUser, string sortOrder,
                                            int pageSize = 6, int pageNumber = 1)
    {
        IQueryable<Client> finalClientObjects = _context.Clients.Include(c => c.User);

        // Searching
        ViewData["SearchString"] = searchString;
        if (!string.IsNullOrEmpty(searchString))
        {
            var upperSearchString = searchString.ToUpper();
            var couldParse = int.TryParse(searchString, out int numValue);

            finalClientObjects = finalClientObjects.Where(s =>
                s.Name.ToUpper().Contains(upperSearchString) ||
                s.Address!.ToUpper().Contains(upperSearchString) ||
                s.Phone!.ToUpper().Contains(upperSearchString) ||
                s.Description!.ToUpper().Contains(upperSearchString) ||
                (couldParse && s.UserID!.Equals(numValue)) ||
                s.User!.Email!.ToUpper().Contains(upperSearchString));
        }

        // Filtering
        ViewData["HasUserOptions"] = new SelectList(new List<bool> { true, false });
        if (hasUser != null)
        {
            if (hasUser == true)
            {
                finalClientObjects = finalClientObjects.Where(c => c.UserID != null);
            }
            else
            {
                finalClientObjects = finalClientObjects.Where(c => c.UserID == null);
            }
            ViewData["UserFilter"] = hasUser;
        }

        // Ordering
        if (!string.IsNullOrEmpty(sortOrder))
        {
            switch (sortOrder)
            {
                case "id_asc":
                    finalClientObjects = finalClientObjects.OrderBy(c => c.Id);
                    break;
                case "id_desc":
                    finalClientObjects = finalClientObjects.OrderByDescending(c => c.Id);
                    break;
                case "name_asc":
                    finalClientObjects = finalClientObjects.OrderBy(c => c.Name);
                    break;
                case "name_desc":
                    finalClientObjects = finalClientObjects.OrderByDescending(c => c.Name);
                    break;
                case "addr_asc":
                    finalClientObjects = finalClientObjects.OrderBy(c => c.Address);
                    break;
                case "addr_desc":
                    finalClientObjects = finalClientObjects.OrderByDescending(c => c.Address);
                    break;
                case "user_asc":
                    finalClientObjects = finalClientObjects.OrderBy(c => c.User!.Email);
                    break;
                case "user_desc":
                    finalClientObjects = finalClientObjects.OrderByDescending(c => c.User!.Email);
                    break;
                default:
                    finalClientObjects = finalClientObjects.OrderBy(c => c.Id);
                    break;
            }
        }

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
        return View(await PaginatedList<Client>.CreateAsyncList(finalClientObjects.AsNoTracking(), pageNumber, pageSize));
    }

    // GET: clients/{id}
    [Route("{id}")]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var clientModel = await _context.Clients
            .Include(c => c.User)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (clientModel == null)
        {
            return NotFound();
        }

        return View(clientModel);
    }

    // GET: clients/create
    [Route("create")]
    public IActionResult Create()
    {
        ViewBag.UserID = new SelectList(_context.Users.ToList(), "Id", "Email");
        return View();
    }

    // POST: clients/create
    [Route("create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Name,Address,Phone,Description,UserID")] Client clientModel)
    {
        if (ModelState.IsValid)
        {
            _context.Add(clientModel);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(clientModel);
    }

    // GET: clients/edit/{id}
    [Route("edit/{id}")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        ViewBag.UserID = new SelectList(_context.Users.ToList(), "Id", "Email");

        var clientModel = await _context.Clients.FindAsync(id);
        if (clientModel == null)
        {
            return NotFound();
        }
        return View(clientModel);
    }

    // POST: clients/edit/{id}
    [Route("edit/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Address,Phone,Description,UserID")] Client clientModel)
    {
        if (id != clientModel.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(clientModel);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClientModelExists(clientModel.Id))
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
        return View(clientModel);
    }

    // GET: clients/delete/{id}
    [Route("delete/{id}")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var clientModel = await _context.Clients
            .FirstOrDefaultAsync(m => m.Id == id);
        if (clientModel == null)
        {
            return NotFound();
        }

        return View(clientModel);
    }

    // POST: clients/delete/{id}
    [Route("delete/{id}")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var clientModel = await _context.Clients.FindAsync(id);
        if (clientModel != null)
        {
            _context.Clients.Remove(clientModel);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ClientModelExists(int id)
    {
        return _context.Clients.Any(e => e.Id == id);
    }
}
