using System.Security.Claims;

using PetRescue.Models;
using PetRescue.Services;


namespace PetRescue.Utilities;

public class AuthUtils
{
    public static async Task<User?> GetCurrentUser(UserService userService, ClaimsPrincipal claimsPrincipal)
    {
        var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return null;
        }

        var user = await userService.GetUser(userId);
        return user;
    }
}
