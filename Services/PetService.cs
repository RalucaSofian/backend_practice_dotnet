using Microsoft.EntityFrameworkCore;
using PetRescue.Data;
using PetRescue.Models;
using PetRescue.Utilities;


namespace PetRescue.Services;

public class PetService
{
    private readonly PetRescueContext _context;

    public PetService(PetRescueContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<Pet>> QueryPets(string searchString, AnimalSpecies? animalSpecies,
                                           AnimalGender? animalGender, int? age_gte, int? age_lte,
                                           string sortOrder, int pageSize = 6, int pageNumber = 1)
    {
        IQueryable<Pet> finalPetObjects = from p in _context.Pets select p;

        // Searching
        if (!string.IsNullOrEmpty(searchString))
        {
            var upperSearchString = searchString.ToUpper();
            finalPetObjects = finalPetObjects.Where(p =>
                p.Name.ToUpper().Contains(upperSearchString) ||
                p.Species.ToString().ToUpper().Contains(upperSearchString) ||
                p.Description!.ToUpper().Contains(upperSearchString));
        }

        // Filtering
        if (animalSpecies != null)
        {
            finalPetObjects = finalPetObjects.Where(p => p.Species == animalSpecies);
        }

        if (animalGender != null)
        {
            finalPetObjects = finalPetObjects.Where(p => p.Gender == animalGender);
        }

        if (age_gte != null)
        {
            finalPetObjects = finalPetObjects.Where(p => p.Age >= age_gte);
        }

        if (age_lte != null)
        {
            finalPetObjects = finalPetObjects.Where(p => p.Age <= age_lte);
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

        return await PaginatedList<Pet>.CreateAsyncList(finalPetObjects.AsNoTracking(), pageNumber, pageSize);
    }

    public async Task<Pet?> GetPet(int id)
    {
        var pet = await _context.Pets.FirstOrDefaultAsync(m => m.Id == id);
        if (pet == null)
        {
            return null;
        }
        else
        {
            return pet;
        }
    }

    public async Task<List<Pet>> GetAllPets()
    {
        return await _context.Pets.ToListAsync();
    }

    public async Task<Pet?> CreatePet(Pet pet)
    {
        _context.Add(pet);
        await _context.SaveChangesAsync();

        var createdPet = await _context.FindAsync<Pet>(pet.Id);
        if (createdPet == null)
        {
            return null;
        }
        else
        {
            return createdPet;
        }
    }

    public async Task<Pet?> EditPet(Pet pet)
    {
        try
        {
            _context.Update(pet);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!PetExists(pet.Id))
            {
                return null;
            }
        }

        var editedPet = await _context.Pets.FindAsync(pet.Id);
        return editedPet;
    }

    public async Task DeletePet(int id)
    {
        var pet = await _context.Pets.FindAsync(id);
        if (pet != null)
        {
            _context.Pets.Remove(pet);
        }
        await _context.SaveChangesAsync();
    }


    private bool PetExists(int id)
    {
        return _context.Pets.Any(e => e.Id == id);
    }
};