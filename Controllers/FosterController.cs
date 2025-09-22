using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using PetRescue.Data;
using PetRescue.Models;
using PetRescue.Utilities;


namespace PetRescue.Controllers;

[Authorize]
[Route("admin/foster")]
public class FosterController : Controller
{
    private readonly PetRescueContext _context;

    public FosterController(PetRescueContext context)
    {
        _context = context;
    }

    private async Task ValidateFosterDates(Foster fosterModel)
    {
        var foundFosters = await _context.Fosters.Where(f => f.PetID == fosterModel.PetID && f.Id != fosterModel.Id).ToListAsync();

        if (foundFosters.Any(f => f.EndDate == null && f.StartDate < fosterModel.EndDate))
        {
            ModelState.AddModelError("", "(Open-ended Foster) Conflicting Foster interval for the same Pet.");
        }
        if (fosterModel.EndDate == null)
        {
            if (foundFosters.Any(f => f.EndDate > fosterModel.StartDate))
            {
                ModelState.AddModelError("", "(Open-ended Foster) Conflicting Foster interval for the same Pet.");
            }
        }

        if (foundFosters.Any(f => f.EndDate > fosterModel.StartDate && f.StartDate < fosterModel.EndDate))
        {
            ModelState.AddModelError("", "Conflicting Foster interval for the same Pet.");
        }
    }

    // GET: foster
    [Route("")]
    public async Task<IActionResult> Index(string searchString, string sortOrder,
                                           DateOnly startDate_gte, DateOnly startDate_lt,
                                           DateOnly endDate_gte, DateOnly endDate_lt,
                                           int clientId, int petId,
                                           int pageSize = 6, int pageNumber = 1)
    {
        var fosterObjects = _context.Fosters.Include(f => f.Pet).Include(f => f.Client);
        var finalFosterObjects = from fo in fosterObjects select fo;

        // Searching
        ViewData["SearchString"] = searchString;
        if (!string.IsNullOrEmpty(searchString))
        {
            var upperSearchString = searchString.ToUpper();
            finalFosterObjects = finalFosterObjects.Where(s =>
                s.Description!.ToUpper().Contains(upperSearchString) ||
                s.Client!.Name.ToUpper().Contains(upperSearchString) ||
                s.Pet!.Name.ToUpper().Contains(upperSearchString));
        }

        // Filtering
        ViewData["ClientID"] = new SelectList(_context.Clients, "Id", "Name");
        ViewData["PetID"] = new SelectList(_context.Pets, "Id", "Name");

        if (startDate_gte != DateOnly.MinValue)
        {
            finalFosterObjects = finalFosterObjects.Where(fo => fo.StartDate >= startDate_gte);
            ViewData["StartDateGteFilter"] = startDate_gte.ToString("o", CultureInfo.InvariantCulture);
        }

        if (startDate_lt != DateOnly.MinValue)
        {
            finalFosterObjects = finalFosterObjects.Where(fo => fo.StartDate < startDate_lt);
            ViewData["StartDateLtFilter"] = startDate_lt.ToString("o", CultureInfo.InvariantCulture);
        }

        if (endDate_gte != DateOnly.MinValue)
        {
            finalFosterObjects = finalFosterObjects.Where(fo => fo.EndDate >= endDate_gte);
            ViewData["EndDateGteFilter"] = endDate_gte.ToString("o", CultureInfo.InvariantCulture);
        }

        if (endDate_lt != DateOnly.MinValue)
        {
            finalFosterObjects = finalFosterObjects.Where(fo => fo.EndDate < endDate_lt);
            ViewData["EndDateLtFilter"] = endDate_lt.ToString("o", CultureInfo.InvariantCulture);
        }

        if (clientId != 0)
        {
            finalFosterObjects = finalFosterObjects.Where(fo => fo.Client!.Id == clientId);
            ViewData["ClientFilter"] = clientId;
        }

        if (petId != 0)
        {
            finalFosterObjects = finalFosterObjects.Where(fo => fo.Pet!.Id == petId);
            ViewData["PetFilter"] = petId;
        }

        // Ordering
        if (!string.IsNullOrEmpty(sortOrder))
        {
            switch (sortOrder)
            {
                case "id_asc":
                    finalFosterObjects = finalFosterObjects.OrderBy(fo => fo.Id);
                    break;
                case "id_desc":
                    finalFosterObjects = finalFosterObjects.OrderByDescending(fo => fo.Id);
                    break;
                case "startDate_asc":
                    finalFosterObjects = finalFosterObjects.OrderBy(fo => fo.StartDate);
                    break;
                case "startDate_desc":
                    finalFosterObjects = finalFosterObjects.OrderByDescending(fo => fo.StartDate);
                    break;
                case "endDate_asc":
                    finalFosterObjects = finalFosterObjects.OrderBy(fo => fo.EndDate);
                    break;
                case "endDate_desc":
                    finalFosterObjects = finalFosterObjects.OrderByDescending(fo => fo.EndDate);
                    break;
                case "clientId_asc":
                    finalFosterObjects = finalFosterObjects.OrderBy(fo => fo.ClientID);
                    break;
                case "clientId_desc":
                    finalFosterObjects = finalFosterObjects.OrderByDescending(fo => fo.ClientID);
                    break;
                case "petId_asc":
                    finalFosterObjects = finalFosterObjects.OrderBy(fo => fo.PetID);
                    break;
                case "petId_desc":
                    finalFosterObjects = finalFosterObjects.OrderByDescending(fo => fo.PetID);
                    break;
                default:
                    finalFosterObjects = finalFosterObjects.OrderBy(fo => fo.Id);
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

        if (string.IsNullOrEmpty(sortOrder) || !sortOrder.StartsWith("startDate_"))
        {
            ViewData["NextStDateSort"] = "startDate_asc";
        }
        else if (sortOrder == "startDate_asc")
        {
            ViewData["NextStDateSort"] = "startDate_desc";
        }

        if (string.IsNullOrEmpty(sortOrder) || !sortOrder.StartsWith("endDate_"))
        {
            ViewData["NextEndDateSort"] = "endDate_asc";
        }
        else if (sortOrder == "endDate_asc")
        {
            ViewData["NextEndDateSort"] = "endDate_desc";
        }

        if (string.IsNullOrEmpty(sortOrder) || !sortOrder.StartsWith("clientId_"))
        {
            ViewData["NextClientIdSort"] = "clientId_asc";
        }
        else if (sortOrder == "clientId_asc")
        {
            ViewData["NextClientIdSort"] = "clientId_desc";
        }

        if (string.IsNullOrEmpty(sortOrder) || !sortOrder.StartsWith("petId_"))
        {
            ViewData["NextPetIdSort"] = "petId_asc";
        }
        else if (sortOrder == "petId_asc")
        {
            ViewData["NextPetIdSort"] = "petId_desc";
        }

        ViewData["CrtSortOrder"] = sortOrder;

        // Paging
        ViewData["CrtPage"] = pageNumber;
        return View(await PaginatedList<Foster>.CreateAsyncList(finalFosterObjects.AsNoTracking(), pageNumber, pageSize));
    }

    // GET: foster/{id}
    [Route("{id}")]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var fosterModel = await _context.Fosters
            .Include(f => f.Pet)
            .Include(f => f.Client)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (fosterModel == null)
        {
            return NotFound();
        }

        return View(fosterModel);
    }

    // GET: foster/create
    [Route("create")]
    public IActionResult Create()
    {
        ViewData["PetID"] = new SelectList(_context.Pets, "Id", "Name");
        ViewData["ClientID"] = new SelectList(_context.Clients, "Id", "Name");
        return View();
    }

    // POST: foster/create
    [Route("create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,ClientID,PetID,Description,StartDate,EndDate")] Foster fosterModel)
    {
        ViewData["PetID"] = new SelectList(_context.Pets, "Id", "Name", fosterModel.PetID);
        ViewData["ClientID"] = new SelectList(_context.Clients, "Id", "Name", fosterModel.ClientID);

        await ValidateFosterDates(fosterModel);

        if (ModelState.IsValid)
        {
            _context.Add(fosterModel);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(fosterModel);
    }

    // GET: foster/edit/{id}
    [Route("edit/{id}")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var fosterModel = await _context.Fosters.FindAsync(id);
        if (fosterModel == null)
        {
            return NotFound();
        }

        ViewData["PetID"] = new SelectList(_context.Pets, "Id", "Name", fosterModel.PetID);
        ViewData["ClientID"] = new SelectList(_context.Clients, "Id", "Name", fosterModel.ClientID);

        return View(fosterModel);
    }

    // POST: foster/edit/{id}
    [Route("edit/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,ClientID,PetID,Description,StartDate,EndDate")] Foster fosterModel)
    {
        if (id != fosterModel.Id)
        {
            return NotFound();
        }

        await ValidateFosterDates(fosterModel);

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(fosterModel);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FosterModelExists(fosterModel.Id))
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

        ViewData["PetID"] = new SelectList(_context.Pets, "Id", "Name", fosterModel.PetID);
        ViewData["ClientID"] = new SelectList(_context.Clients, "Id", "Name", fosterModel.ClientID);

        return View(fosterModel);
    }

    // GET: foster/delete/{id}
    [Route("delete/{id}")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var fosterModel = await _context.Fosters
            .Include(f => f.Pet)
            .Include(f => f.Client)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (fosterModel == null)
        {
            return NotFound();
        }

        return View(fosterModel);
    }

    // POST: foster/delete/{id}
    [Route("delete/{id}")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var fosterModel = await _context.Fosters.FindAsync(id);
        if (fosterModel != null)
        {
            _context.Fosters.Remove(fosterModel);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool FosterModelExists(int id)
    {
        return _context.Fosters.Any(e => e.Id == id);
    }
}
