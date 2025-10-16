using PetRescue.Models;


namespace PetRescue.ApiDTOs;

public class ClientOutputDTO
{
    public int Id { get; private set; } = default!;
    public string? UserId { get; private set; }
    public string Name { get; private set; } = default!;
    public string? Address { get; private set; }
    public string? Phone { get; private set; }
    public string? Description { get; private set; }
    public UserOutputDTO? UserInfo { get; private set; }


    public static ClientOutputDTO FromDbClient(Client client)
    {
        return new ClientOutputDTO()
        {
            Id = client.Id,
            UserId = client.UserID,
            Name = client.Name,
            Address = client.Address,
            Phone = client.Phone,
            Description = client.Description,
            UserInfo = client.User == null ? null : UserOutputDTO.FromDbUser(client.User)
        };
    }
}
