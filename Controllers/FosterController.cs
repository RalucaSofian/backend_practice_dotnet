using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

using PetRescue.Models;
using PetRescue.Services;


namespace PetRescue.Controllers;

[Authorize]
[Route("admin/foster")]
public class FosterController : Controller
{
    private readonly FosterService _fosterService;
    private readonly ClientService _clientService;
    private readonly PetService _petService;

    public FosterController(FosterService fosterService, ClientService clientService, PetService petService)
    {
        _fosterService = fosterService;
        _clientService = clientService;
        _petService = petService;
    }


    private async Task ValidateFosterDates(Foster foster)
    {
        var foundFoster = (await _fosterService.GetFosterForPet(foster.PetID)).Where(f => f.Id != foster.Id);

        if (foundFoster.Any(f => f.EndDate == null && f.StartDate < foster.EndDate))
        {
            ModelState.AddModelError("", "(Open-ended Foster) Conflicting Foster interval for the same Pet.");
        }

        if (foster.EndDate == null)
        {
            if (foundFoster.Any(f => f.EndDate > foster.StartDate))
            {
                ModelState.AddModelError("", "(Open-ended Foster) Conflicting Foster interval for the same Pet.");
            }
        }

        if (foundFoster.Any(f => f.EndDate > foster.StartDate && f.StartDate < foster.EndDate))
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
        var queryOptions = new FosterService.QueryOptions
        {
            SearchString = string.IsNullOrEmpty(searchString) ? null : searchString,
            SortOrder = string.IsNullOrEmpty(sortOrder) ? null : sortOrder,
            StartDate_GTE = startDate_gte == DateOnly.MinValue ? null : startDate_gte,
            StartDate_LT = startDate_lt == DateOnly.MinValue ? null : startDate_lt,
            EndDate_GTE = endDate_gte == DateOnly.MinValue ? null : endDate_gte,
            EndDate_LT = endDate_lt == DateOnly.MinValue ? null : endDate_lt,
            ClientId = clientId == 0 ? null : clientId,
            PetId = petId == 0 ? null : petId,
            PageSize = pageSize,
            PageNumber = pageNumber
        };

        var finalFosterObjects = await _fosterService.QueryFoster(queryOptions);

        // Searching
        ViewData["SearchString"] = searchString;

        // Filtering
        var allClients = await _clientService.GetAllClients();
        var allPets = await _petService.GetAllPets();

        ViewData["ClientID"] = new SelectList(allClients, "Id", "Name");
        ViewData["PetID"] = new SelectList(allPets, "Id", "Name");

        if (startDate_gte != DateOnly.MinValue)
        {
            ViewData["StartDateGteFilter"] = startDate_gte.ToString("o", CultureInfo.InvariantCulture);
        }

        if (startDate_lt != DateOnly.MinValue)
        {
            ViewData["StartDateLtFilter"] = startDate_lt.ToString("o", CultureInfo.InvariantCulture);
        }

        if (endDate_gte != DateOnly.MinValue)
        {
            ViewData["EndDateGteFilter"] = endDate_gte.ToString("o", CultureInfo.InvariantCulture);
        }

        if (endDate_lt != DateOnly.MinValue)
        {
            ViewData["EndDateLtFilter"] = endDate_lt.ToString("o", CultureInfo.InvariantCulture);
        }

        if (clientId != 0)
        {
            ViewData["ClientFilter"] = clientId;
        }

        if (petId != 0)
        {
            ViewData["PetFilter"] = petId;
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
        return View(finalFosterObjects);
    }

    // GET: foster/{id}
    [Route("{id}")]
    public async Task<IActionResult> Details(int id)
    {
        var foster = await _fosterService.GetFoster(id);
        if (foster == null)
        {
            return NotFound();
        }

        return View(foster);
    }

    // GET: foster/create
    [Route("create")]
    public async Task<IActionResult> Create()
    {
        var allPets = await _petService.GetAllPets();
        var allClients = await _clientService.GetAllClients();

        ViewData["PetID"] = new SelectList(allPets, "Id", "Name");
        ViewData["ClientID"] = new SelectList(allClients, "Id", "Name");

        return View();
    }

    // POST: foster/create
    [Route("create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,ClientID,PetID,Description,StartDate,EndDate")] Foster foster)
    {
        var allPets = await _petService.GetAllPets();
        var allClients = await _clientService.GetAllClients();

        ViewData["PetID"] = new SelectList(allPets, "Id", "Name", foster.PetID);
        ViewData["ClientID"] = new SelectList(allClients, "Id", "Name", foster.ClientID);

        await ValidateFosterDates(foster);

        if (ModelState.IsValid)
        {
            var createdFoster = await _fosterService.CreateFoster(foster);
            if (createdFoster != null)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View(foster);
            }
        }
        return View(foster);
    }

    // GET: foster/edit/{id}
    [Route("edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var foster = await _fosterService.GetFoster(id);
        if (foster == null)
        {
            return NotFound();
        }

        var allPets = await _petService.GetAllPets();
        var allClients = await _clientService.GetAllClients();
    
        ViewData["PetID"] = new SelectList(allPets, "Id", "Name", foster.PetID);
        ViewData["ClientID"] = new SelectList(allClients, "Id", "Name", foster.ClientID);

        return View(foster);
    }

    // POST: foster/edit/{id}
    [Route("edit/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,ClientID,PetID,Description,StartDate,EndDate")] Foster foster)
    {
        if (id != foster.Id)
        {
            return NotFound();
        }

        await ValidateFosterDates(foster);

        if (ModelState.IsValid)
        {
            var editedFoster = await _fosterService.EditFoster(foster);
            if (editedFoster != null)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View(foster);
            }
        }

        var allPets = await _petService.GetAllPets();
        var allClients = await _clientService.GetAllClients();

        ViewData["PetID"] = new SelectList(allPets, "Id", "Name", foster.PetID);
        ViewData["ClientID"] = new SelectList(allClients, "Id", "Name", foster.ClientID);

        return View(foster);
    }

    // GET: foster/delete/{id}
    [Route("delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var foster = await _fosterService.GetFoster(id);
        if (foster == null)
        {
            return NotFound();
        }

        return View(foster);
    }

    // POST: foster/delete/{id}
    [Route("delete/{id}")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _fosterService.DeleteFoster(id);
        return RedirectToAction(nameof(Index));
    }
}
