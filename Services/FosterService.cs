using Microsoft.EntityFrameworkCore;
using PetRescue.Data;
using PetRescue.Models;
using PetRescue.Utilities;


namespace PetRescue.Services;

public class FosterService
{
    private readonly PetRescueContext _context;

    public FosterService(PetRescueContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<Foster>> QueryFoster(string searchString, string sortOrder,
                                           DateOnly startDate_gte, DateOnly startDate_lt,
                                           DateOnly endDate_gte, DateOnly endDate_lt,
                                           int clientId, int petId,
                                           int pageSize = 6, int pageNumber = 1)
    {
        var fosterObjects = _context.Fosters.Include(f => f.Pet).Include(f => f.Client);
        IQueryable<Foster> finalFosterObjects = from fo in fosterObjects select fo;

        // Searching
        if (!string.IsNullOrEmpty(searchString))
        {
            var upperSearchString = searchString.ToUpper();
            finalFosterObjects = finalFosterObjects.Where(s =>
                s.Description!.ToUpper().Contains(upperSearchString) ||
                s.Client!.Name.ToUpper().Contains(upperSearchString) ||
                s.Pet!.Name.ToUpper().Contains(upperSearchString));
        }

        // Filtering
        if (startDate_gte != DateOnly.MinValue)
        {
            finalFosterObjects = finalFosterObjects.Where(fo => fo.StartDate >= startDate_gte);
        }

        if (startDate_lt != DateOnly.MinValue)
        {
            finalFosterObjects = finalFosterObjects.Where(fo => fo.StartDate < startDate_lt);
        }

        if (endDate_gte != DateOnly.MinValue)
        {
            finalFosterObjects = finalFosterObjects.Where(fo => fo.EndDate >= endDate_gte);
        }

        if (endDate_lt != DateOnly.MinValue)
        {
            finalFosterObjects = finalFosterObjects.Where(fo => fo.EndDate < endDate_lt);
        }

        if (clientId != 0)
        {
            finalFosterObjects = finalFosterObjects.Where(fo => fo.Client!.Id == clientId);
        }

        if (petId != 0)
        {
            finalFosterObjects = finalFosterObjects.Where(fo => fo.Pet!.Id == petId);
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

        return await PaginatedList<Foster>.CreateAsyncList(finalFosterObjects.AsNoTracking(), pageNumber, pageSize);
    }

    public async Task<List<Foster>> GetAllFoster()
    {
        return await _context.Fosters.ToListAsync();
    }

    public async Task<List<Foster>> GetFosterForPet(Foster foster)
    {
        var fosterList = await _context.Fosters.Where(f => f.PetID == foster.PetID && f.Id != foster.Id).ToListAsync();
        return fosterList;
    }

    public async Task<Foster?> GetFoster(int id)
    {
        var foster = await _context.Fosters.Include(f => f.Pet).Include(f => f.Client).FirstOrDefaultAsync(m => m.Id == id);
        if (foster == null)
        {
            return null;
        }
        else
        {
            return foster;
        }
    }

    public async Task<Foster?> CreateFoster(Foster foster)
    {
        _context.Add(foster);
        await _context.SaveChangesAsync();

        var createdFoster = await _context.FindAsync<Foster>(foster.Id);
        if (createdFoster == null)
        {
            return null;
        }
        else
        {
            return createdFoster;
        }
    }

    public async Task<Foster?> EditFoster(Foster foster)
    {
        try
        {
            _context.Update(foster);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!FosterExists(foster.Id))
            {
                return null;
            }
        }

        var editedFoster = await _context.Fosters.FindAsync(foster.Id);
        return editedFoster;
    }

    public async Task DeleteFoster(int id)
    {
        var foster = await _context.Fosters.FindAsync(id);
        if (foster != null)
        {
            _context.Fosters.Remove(foster);
        }
        await _context.SaveChangesAsync();
    }


    private bool FosterExists(int id)
    {
        return _context.Fosters.Any(e => e.Id == id);
    }
};
