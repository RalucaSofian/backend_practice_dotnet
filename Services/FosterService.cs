using Microsoft.EntityFrameworkCore;

using PetRescue.Data;
using PetRescue.Models;
using PetRescue.Utilities;


namespace PetRescue.Services;

public class FosterService
{
    public class QueryOptions
    {
        public string? SearchString = null;
        public string? SortOrder = null;
        public DateOnly? StartDate_GTE = null;
        public DateOnly? StartDate_LT = null;
        public DateOnly? EndDate_GTE = null;
        public DateOnly? EndDate_LT = null;
        public int? ClientId = null;
        public int? PetId = null;
        public int PageSize = 6;
        public int PageNumber = 1;
    }


    private readonly PetRescueContext _context;

    public FosterService(PetRescueContext context)
    {
        _context = context;
    }


    public async Task<PaginatedList<Foster>> QueryFoster(QueryOptions queryOptions)
    {
        var fosterObjects = _context.Fosters.Include(f => f.Pet).Include(f => f.Client);
        IQueryable<Foster> finalFosterObjects = from fo in fosterObjects select fo;

        // Searching
        if (!string.IsNullOrEmpty(queryOptions.SearchString))
        {
            var upperSearchString = queryOptions.SearchString.ToUpper();
            finalFosterObjects = finalFosterObjects.Where(s =>
                s.Description!.ToUpper().Contains(upperSearchString) ||
                s.Client!.Name.ToUpper().Contains(upperSearchString) ||
                s.Pet!.Name.ToUpper().Contains(upperSearchString));
        }

        // Filtering
        if (queryOptions.StartDate_GTE != null)
        {
            finalFosterObjects = finalFosterObjects.Where(fo => fo.StartDate >= queryOptions.StartDate_GTE);
        }

        if (queryOptions.StartDate_LT != null)
        {
            finalFosterObjects = finalFosterObjects.Where(fo => fo.StartDate < queryOptions.StartDate_LT);
        }

        if (queryOptions.EndDate_GTE != null)
        {
            finalFosterObjects = finalFosterObjects.Where(fo => fo.EndDate >= queryOptions.EndDate_GTE);
        }

        if (queryOptions.EndDate_LT != null)
        {
            finalFosterObjects = finalFosterObjects.Where(fo => fo.EndDate < queryOptions.EndDate_LT);
        }

        if (queryOptions.ClientId != null)
        {
            finalFosterObjects = finalFosterObjects.Where(fo => fo.Client!.Id == queryOptions.ClientId);
        }

        if (queryOptions.PetId != null)
        {
            finalFosterObjects = finalFosterObjects.Where(fo => fo.Pet!.Id == queryOptions.PetId);
        }

        // Ordering
        if (!string.IsNullOrEmpty(queryOptions.SortOrder))
        {
            switch (queryOptions.SortOrder)
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
        else
        {
            finalFosterObjects = finalFosterObjects.OrderBy(fo => fo.Id);
        }

        return await PaginatedList<Foster>.CreateAsyncList(finalFosterObjects.AsNoTracking(),
            queryOptions.PageNumber, queryOptions.PageSize);
    }

    public async Task<List<Foster>> GetAllFoster()
    {
        return await _context.Fosters.ToListAsync();
    }

    public async Task<int> GetTotalNrOfFoster()
    {
        return await _context.Fosters.CountAsync();
    }

    public async Task<List<Foster>> GetFosterForPet(int petId)
    {
        var fosterList = await _context.Fosters.Where(f => f.PetID == petId).ToListAsync();
        return fosterList;
    }

    public async Task<Foster?> GetActiveFosterForPet(int petId)
    {
        var dateToday = DateOnly.FromDateTime(DateTime.Now);
        var fosterList = await GetFosterForPet(petId);

        var activeFoster = fosterList.Where(f => f.StartDate <= dateToday && (f.EndDate >= dateToday || f.EndDate == null)).FirstOrDefault();
        return activeFoster;
    }

    public async Task<int> GetNrOfFosteredPets()
    {
        return await _context.Fosters.Select(f => f.PetID).Distinct().CountAsync();
    }

    public async Task<float> GetAvgFosterDuration()
    {
        var fosters = await _context.Fosters.ToListAsync();
        var totalFosterDuration = 0;
        var nrOfFoster = 0;
        foreach (var foster in fosters)
        {
            if (foster.EndDate.HasValue)
            {
                totalFosterDuration += foster.EndDate.Value.DayNumber - foster.StartDate.DayNumber;
                nrOfFoster++;
            }
        }

        float avgFosterDuration = 0;
        if (nrOfFoster != 0)
        {
            avgFosterDuration = (float)totalFosterDuration / nrOfFoster;
        }
        return avgFosterDuration;
    }

    public async Task<Foster?> GetFoster(int id)
    {
        var foster = await _context.Fosters.Include(f => f.Pet).Include(f => f.Client).FirstOrDefaultAsync(m => m.Id == id);
        if (foster == null)
        {
            return null;
        }

        return foster;
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

        return createdFoster;
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
