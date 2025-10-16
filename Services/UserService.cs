using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using PetRescue.Models;
using PetRescue.Data;
using PetRescue.Utilities;


namespace PetRescue.Services;

public class UserService
{
    private readonly PetRescueContext _context;
    private readonly UserManager<User> _userManager;

    public UserService(PetRescueContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }


    public async Task<PaginatedList<User>> QueryUsers(string searchString, UserRole? userRole, string sortOrder,
                              int pageSize = 6, int pageNumber = 1)
    {
        IQueryable<User> finalUserObjects = from u in _context.Users select u;

        // Searching
        if (!string.IsNullOrEmpty(searchString))
        {
            var upperSearchString = searchString.ToUpper();
            finalUserObjects = finalUserObjects.Where(u =>
                u.Email!.ToUpper().Contains(upperSearchString) ||
                u.Name!.ToUpper().Contains(upperSearchString));
        }

        // Filtering
        if (userRole != null)
        {
            finalUserObjects = finalUserObjects.Where(u => u.Role == userRole);
        }

        // Ordering
        if (!string.IsNullOrEmpty(sortOrder))
        {
            switch (sortOrder)
            {
                case "email_asc":
                    finalUserObjects = finalUserObjects.OrderBy(u => u.Email);
                    break;
                case "email_desc":
                    finalUserObjects = finalUserObjects.OrderByDescending(u => u.Email);
                    break;
                case "name_asc":
                    finalUserObjects = finalUserObjects.OrderBy(u => u.Name);
                    break;
                case "name_desc":
                    finalUserObjects = finalUserObjects.OrderByDescending(u => u.Name);
                    break;
                case "role_asc":
                    finalUserObjects = finalUserObjects.OrderBy(u => u.Role);
                    break;
                case "role_desc":
                    finalUserObjects = finalUserObjects.OrderByDescending(u => u.Role);
                    break;
                default:
                    finalUserObjects = finalUserObjects.OrderBy(u => u.Email);
                    break;
            }
        }
        else
        {
            finalUserObjects = finalUserObjects.OrderBy(u => u.Id);
        }

        return await PaginatedList<User>.CreateAsyncList(finalUserObjects.AsNoTracking(), pageNumber, pageSize);
    }

    public async Task<User?> GetUser(string id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(m => m.Id == id);
        if (user == null)
        {
            return null;
        }

        return user;
    }

    public async Task<List<User>> GetAllUsers()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task<User?> CreateUser(User user)
    {
        var createResult = await _userManager.CreateAsync(user);
        if (createResult.Succeeded)
        {
            var createdUser = await _userManager.FindByEmailAsync(user.Email!);
            return createdUser;
        }
        else
        {
            return null;
        }
    }

    public async Task<User?> EditUser(User user)
    {
        try
        {
            var dbUser = await GetUser(user.Id);
            dbUser!.Email = user.Email;
            dbUser!.Role = user.Role;
            dbUser!.Name = user.Name;

            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!UserExists(user.Id))
            {
                return null;
            }
        }

        var editedUser = await _userManager.FindByIdAsync(user.Id);
        return editedUser;
    }

    public async Task DeleteUser(string id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
        }
        await _context.SaveChangesAsync();
    }

    public async Task<User?> ForgotPassword(User user)
    {
        try
        {
            await _userManager.RemovePasswordAsync(user);
            string resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            Console.WriteLine("Password Reset Token");
            Console.WriteLine(resetToken);

            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!UserExists(user.Id))
            {
                return null;
            }
        }

        var editedUser = await _userManager.FindByEmailAsync(user.Email!);
        return editedUser;
    }


    private bool UserExists(string id)
    {
        return _context.Users.Any(e => e.Id == id);
    }
};
