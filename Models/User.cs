using Microsoft.AspNetCore.Identity;


namespace PetRescue.Models;

public class User : IdentityUser
{
    public UserRole Role { get; set; }

    public string? Name { get; set; }

    public virtual ICollection<Client> Clients { get; set; } = new List<Client>();
}

public enum UserRole { ADMIN, USER }
