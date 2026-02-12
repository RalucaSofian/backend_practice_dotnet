using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;


namespace PetRescue.Models;

public class User : IdentityUser
{
    public UserRole Role { get; set; }

    public string? Name { get; set; }

    [JsonProperty("phone")]
    public override string? PhoneNumber { get; set; }

    public virtual ICollection<Client> Clients { get; set; } = new List<Client>();
}

public enum UserRole { ADMIN, USER }
