using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using PetRescue.Data;
using PetRescue.Models;
using PetRescue.Utilities;


namespace PetRescue.Controllers;

[Authorize]
[Route("admin/pets")]
public class PetsController : Controller
{
    private readonly PetRescueContext _context;

    public PetsController(PetRescueContext context)
    {
        _context = context;
    }

    // GET: pets
    [Route("")]
    public async Task<IActionResult> Index(string searchString, AnimalSpecies? animalSpecies,
                                           AnimalGender? animalGender, int? age_gte, int? age_lte,
                                           string sortOrder, int pageSize = 6, int pageNumber = 1)
    {
        var finalPetObjects = from p in _context.Pets select p;

        // Searching
        ViewData["SearchString"] = searchString;
        if (!string.IsNullOrEmpty(searchString))
        {
            var upperSearchString = searchString.ToUpper();
            finalPetObjects = finalPetObjects.Where(p =>
                p.Name.ToUpper().Contains(upperSearchString) ||
                p.Species.ToString().ToUpper().Contains(upperSearchString) ||
                p.Description!.ToUpper().Contains(upperSearchString));
        }

        // Filtering
        ViewData["SpeciesOptions"] = new SelectList(new List<AnimalSpecies> {AnimalSpecies.Bird, AnimalSpecies.Cat, AnimalSpecies.Dog, AnimalSpecies.Lizard,
            AnimalSpecies.Rodent, AnimalSpecies.Snake, AnimalSpecies.Other});

        ViewData["GenderOptions"] = new SelectList(new List<AnimalGender> { AnimalGender.F, AnimalGender.M });

        if (animalSpecies != null)
        {
            finalPetObjects = finalPetObjects.Where(p => p.Species == animalSpecies);
            ViewData["SpeciesFilter"] = animalSpecies;
        }

        if (animalGender != null)
        {
            finalPetObjects = finalPetObjects.Where(p => p.Gender == animalGender);
            ViewData["GenderFilter"] = animalGender;
        }

        if (age_gte != null)
        {
            finalPetObjects = finalPetObjects.Where(p => p.Age >= age_gte);
            ViewData["AgeGteFilter"] = age_gte;
        }

        if (age_lte != null)
        {
            finalPetObjects = finalPetObjects.Where(p => p.Age <= age_lte);
            ViewData["AgeLteFilter"] = age_lte;
        }

        // Ordering
        if (!string.IsNullOrEmpty(sortOrder))
        {
            switch (sortOrder)
            {
                case "id_asc":
                    finalPetObjects = finalPetObjects.OrderBy(p => p.Id);
                    break;
                case "id_desc":
                    finalPetObjects = finalPetObjects.OrderByDescending(p => p.Id);
                    break;
                case "name_asc":
                    finalPetObjects = finalPetObjects.OrderBy(p => p.Name);
                    break;
                case "name_desc":
                    finalPetObjects = finalPetObjects.OrderByDescending(p => p.Name);
                    break;
                case "species_asc":
                    finalPetObjects = finalPetObjects.OrderBy(p => p.Species);
                    break;
                case "species_desc":
                    finalPetObjects = finalPetObjects.OrderByDescending(p => p.Species);
                    break;
                case "gender_asc":
                    finalPetObjects = finalPetObjects.OrderBy(p => p.Gender);
                    break;
                case "gender_desc":
                    finalPetObjects = finalPetObjects.OrderByDescending(p => p.Gender);
                    break;
                case "age_asc":
                    finalPetObjects = finalPetObjects.OrderBy(p => p.Age);
                    break;
                case "age_desc":
                    finalPetObjects = finalPetObjects.OrderByDescending(p => p.Age);
                    break;
                default:
                    finalPetObjects = finalPetObjects.OrderBy(p => p.Id);
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

        if (string.IsNullOrEmpty(sortOrder) || !sortOrder.StartsWith("species_"))
        {
            ViewData["NextSpeciesSort"] = "species_asc";
        }
        else if (sortOrder == "species_asc")
        {
            ViewData["NextSpeciesSort"] = "species_desc";
        }

        if (string.IsNullOrEmpty(sortOrder) || !sortOrder.StartsWith("gender_"))
        {
            ViewData["NextGenderSort"] = "gender_asc";
        }
        else if (sortOrder == "gender_asc")
        {
            ViewData["NextGenderSort"] = "gender_desc";
        }

        if (string.IsNullOrEmpty(sortOrder) || !sortOrder.StartsWith("age_"))
        {
            ViewData["NextAgeSort"] = "age_asc";
        }
        else if (sortOrder == "age_asc")
        {
            ViewData["NextAgeSort"] = "age_desc";
        }

        ViewData["CrtSortOrder"] = sortOrder;

        // Paging
        ViewData["CrtPage"] = pageNumber;
        return View(await PaginatedList<Pet>.CreateAsyncList(finalPetObjects.AsNoTracking(), pageNumber, pageSize));
    }

    // GET: pets/id
    [Route("{id}")]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var petsModel = await _context.Pets
            .FirstOrDefaultAsync(m => m.Id == id);
        if (petsModel == null)
        {
            return NotFound();
        }

        return View(petsModel);
    }

    // GET: pets/create
    [Route("create")]
    public IActionResult Create()
    {
        return View();
    }

    // POST: Pets/Create
    [Route("create")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Name,Species,Gender,Age,Description")] Pet petsModel)
    {
        if (ModelState.IsValid)
        {
            _context.Add(petsModel);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(petsModel);
    }

    // GET: pets/edit/{id}
    [Route("edit/{id}")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var petsModel = await _context.Pets.FindAsync(id);
        if (petsModel == null)
        {
            return NotFound();
        }
        return View(petsModel);
    }

    // POST: pets/edit/{id}
    [Route("edit/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Species,Gender,Age,Description")] Pet petsModel)
    {
        if (id != petsModel.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(petsModel);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PetsModelExists(petsModel.Id))
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
        return View(petsModel);
    }

    // GET: Pets/Delete/5
    [Route("delete/{id}")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var petsModel = await _context.Pets
            .FirstOrDefaultAsync(m => m.Id == id);
        if (petsModel == null)
        {
            return NotFound();
        }

        return View(petsModel);
    }

    // POST: pets/delete/{id}
    [Route("delete/{id}")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var petsModel = await _context.Pets.FindAsync(id);
        if (petsModel != null)
        {
            _context.Pets.Remove(petsModel);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool PetsModelExists(int id)
    {
        return _context.Pets.Any(e => e.Id == id);
    }
}
