using Microsoft.EntityFrameworkCore;

using PetRescue.Data;
using PetRescue.Models;
using PetRescue.Utilities;


namespace PetRescue.Services;

public class ClientService
{
    public class QueryOptions
    {
        public string? SearchString = null;
        public string? SortOrder = null;
        public bool? HasUser = null;
        public string? UserId = null;
        public int PageSize = 6;
        public int PageNumber = 1;
    }

    private readonly PetRescueContext _context;

    public ClientService(PetRescueContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<Client>> QueryClients(QueryOptions queryOptions)
    {
        IQueryable<Client> finalClientObjects = _context.Clients.Include(c => c.User).Where(c => c.Id != 0);

        // Searching
        if (!string.IsNullOrEmpty(queryOptions.SearchString))
        {
            var upperSearchString = queryOptions.SearchString.ToUpper();
            var couldParse = int.TryParse(queryOptions.SearchString, out int numValue);

            finalClientObjects = finalClientObjects.Where(s =>
                s.Name.ToUpper().Contains(upperSearchString) ||
                s.Address!.ToUpper().Contains(upperSearchString) ||
                s.Phone!.ToUpper().Contains(upperSearchString) ||
                s.Description!.ToUpper().Contains(upperSearchString) ||
                (couldParse && s.UserID!.Equals(numValue)) ||
                s.User!.Email!.ToUpper().Contains(upperSearchString));
        }

        // Filtering
        if (queryOptions.HasUser != null)
        {
            if (queryOptions.HasUser == true)
            {
                finalClientObjects = finalClientObjects.Where(c => c.UserID != null);
            }
            else
            {
                finalClientObjects = finalClientObjects.Where(c => c.UserID == null);
            }
        }

        if (queryOptions.UserId != null)
        {
            finalClientObjects = finalClientObjects.Where(fo => fo.User!.Id == queryOptions.UserId);
        }

        // Ordering
        if (!string.IsNullOrEmpty(queryOptions.SortOrder))
        {
            switch (queryOptions.SortOrder)
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
        else
        {
            finalClientObjects = finalClientObjects.OrderBy(c => c.Id);
        }

        return await PaginatedList<Client>.CreateAsyncList(finalClientObjects.AsNoTracking(),
            queryOptions.PageNumber, queryOptions.PageSize);
    }

    public async Task<Client?> GetClient(int id)
    {
        var client = await _context.Clients.Include(c => c.User).FirstOrDefaultAsync(m => m.Id == id);
        if (client == null)
        {
            return null;
        }

        return client;
    }

    public async Task<Client?> GetClientForUserId(string id)
    {
        var client = await _context.Clients.FirstAsync(m => m.UserID == id);
        if (client == null)
        {
            return null;
        }

        return client;
    }

    public async Task<List<Client>> GetAllClients()
    {
        return await _context.Clients.Where(c => c.Id != 0).ToListAsync();
    }

    public async Task<Client?> CreateClient(Client client)
    {
        _context.Add(client);
        await _context.SaveChangesAsync();

        var createdClient = await _context.FindAsync<Client>(client.Id);
        if (createdClient == null)
        {
            return null;
        }
        else
        {
            return createdClient;
        }
    }

    public async Task<Client?> EditClient(Client client)
    {
        try
        {
            _context.Update(client);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ClientExists(client.Id))
            {
                return null;
            }
        }

        var editedClient = await _context.Clients.FindAsync(client.Id);
        return editedClient;
    }

    public async Task DeleteClient(int id)
    {
        var client = await _context.Clients.FindAsync(id);
        if (client != null)
        {
            _context.Clients.Remove(client);
        }
        await _context.SaveChangesAsync();
    }


    private bool ClientExists(int id)
    {
        return _context.Clients.Any(e => e.Id == id);
    }
};
