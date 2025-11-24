using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

using PetRescue.Models;
using PetRescue.Services;


namespace PetRescue.Controllers;

[Authorize]
[Route("admin/pets")]
public class PetsController : Controller
{
    private readonly PetService _petService;

    public PetsController(PetService petService)
    {
        _petService = petService;
    }

    // GET: pets
    [Route("")]
    public async Task<IActionResult> Index(string searchString, AnimalSpecies? animalSpecies,
                                           AnimalGender? animalGender, int? age_gte, int? age_lte,
                                           string sortOrder, int pageSize = 6, int pageNumber = 1)
    {
        var finalPetObjects = await _petService.QueryPets(searchString, animalSpecies, animalGender,
                                age_gte, age_lte, sortOrder, pageSize, pageNumber);

        // Searching
        ViewData["SearchString"] = searchString;

        // Filtering
        ViewData["SpeciesOptions"] = new SelectList(new List<AnimalSpecies> {AnimalSpecies.Bird, AnimalSpecies.Cat,
            AnimalSpecies.Dog, AnimalSpecies.Lizard, AnimalSpecies.Rodent, AnimalSpecies.Snake, AnimalSpecies.Other});

        ViewData["GenderOptions"] = new SelectList(new List<AnimalGender> { AnimalGender.F, AnimalGender.M });

        if (animalSpecies != null)
        {
            ViewData["SpeciesFilter"] = animalSpecies;
        }

        if (animalGender != null)
        {
            ViewData["GenderFilter"] = animalGender;
        }

        if (age_gte != null)
        {
            ViewData["AgeGteFilter"] = age_gte;
        }

        if (age_lte != null)
        {
            ViewData["AgeLteFilter"] = age_lte;
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
        return View(finalPetObjects);
    }

    // GET: pets/id
    [Route("{id}")]
    public async Task<IActionResult> Details(int id)
    {
        var pet = await _petService.GetPet(id);
        if (pet == null)
        {
            return NotFound();
        }

        return View(pet);
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
    public async Task<IActionResult> Create([Bind("Id,Name,Species,Gender,Age,Description,PhotoUrl")] Pet pet)
    {
        if (ModelState.IsValid)
        {
            var createdPet = await _petService.CreatePet(pet);
            if (createdPet != null)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View(pet);
            }
        }
        return View(pet);
    }

    // GET: pets/edit/{id}
    [Route("edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var pet = await _petService.GetPet(id);
        if (pet == null)
        {
            return NotFound();
        }
        return View(pet);
    }

    // POST: pets/edit/{id}
    [Route("edit/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Species,Gender,Age,Description,PhotoUrl")] Pet pet)
    {
        if (id != pet.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var editedPet = await _petService.EditPet(pet);
            if (editedPet != null)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View(pet);
            }
        }
        return View(pet);
    }

    // GET: Pets/Delete/5
    [Route("delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var pet = await _petService.GetPet(id);
        if (pet == null)
        {
            return NotFound();
        }

        return View(pet);
    }

    // POST: pets/delete/{id}
    [Route("delete/{id}")]
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _petService.DeletePet(id);
        return RedirectToAction(nameof(Index));
    }
}
