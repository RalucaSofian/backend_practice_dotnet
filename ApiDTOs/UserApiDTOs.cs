using PetRescue.Models;


namespace PetRescue.ApiDTOs;

public class UserOutputDTO
{
    public string? Name { get; private set; }
    public string? UserName { get; private set; }
    public string Email { get; private set; } = default!;
    public string? Phone { get; private set; }


    public static UserOutputDTO FromDbUser(User user)
    {
        return new UserOutputDTO()
        {
            Name = user.Name,
            UserName = user.UserName,
            Email = user.Email!,
            Phone = user.PhoneNumber
        };
    }
}
