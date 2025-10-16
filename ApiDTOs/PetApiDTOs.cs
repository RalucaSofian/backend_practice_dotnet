using PetRescue.Models;


namespace PetRescue.ApiDTOs;

public class PetOutputDTO
{
    public int Id { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string Species { get; private set; } = default!;
    public string Gender { get; private set; } = default!;
    public int? Age { get; private set; }
    public string? Description { get; private set; }


    public static PetOutputDTO FromDbPet(Pet pet)
    {
        return new PetOutputDTO()
        {
            Id = pet.Id,
            Name = pet.Name,
            Species = pet.Species.ToString(),
            Gender = pet.Gender.ToString(),
            Age = pet.Age,
            Description = pet.Description
        };
    }
}
