using Microsoft.EntityFrameworkCore;

using PetRescue.Data;
using PetRescue.Models;
using PetRescue.Utilities;


namespace PetRescue.Services;

public class ClientService
{
    private readonly PetRescueContext _context;

    public ClientService(PetRescueContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<Client>> QueryClients(string searchString, bool? hasUser, string sortOrder,
                                            int pageSize = 6, int pageNumber = 1)
    {
        IQueryable<Client> finalClientObjects = _context.Clients.Include(c => c.User);

        // Searching
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

        return await PaginatedList<Client>.CreateAsyncList(finalClientObjects.AsNoTracking(), pageNumber, pageSize);
    }

    public async Task<Client?> GetClient(int id)
    {
        var client = await _context.Clients.Include(c => c.User).FirstOrDefaultAsync(m => m.Id == id);
        if (client == null)
        {
            return null;
        }
        else
        {
            return client;
        }
    }

    public async Task<List<Client>> GetAllClients()
    {
        return await _context.Clients.ToListAsync();
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