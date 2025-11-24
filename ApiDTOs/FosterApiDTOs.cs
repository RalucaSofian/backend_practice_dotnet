using PetRescue.Models;


namespace PetRescue.ApiDTOs;

public class CreateFosterInputDTO
{
    public int PetId { get; set; } = default!;
    public string? Description { get; set; }
    public DateOnly StartDate { get; set; } = default!;
    public DateOnly? EndDate { get; set; }
}

public class FosterOutputDTO
{
    public int Id { get; private set; } = default!;
    public int PetId { get; private set; } = default!;
    public string? Description { get; private set; }
    public DateOnly StartDate { get; private set; } = default!;
    public DateOnly? EndDate { get; private set; }
    public PetOutputDTO? PetInfo { get; private set; } = default!;


    public static FosterOutputDTO FromDbFoster(Foster foster)
    {
        return new FosterOutputDTO()
        {
            Id = foster.Id,
            PetId = foster.PetID,
            Description = foster.Description,
            StartDate = foster.StartDate,
            EndDate = foster.EndDate,
            PetInfo = foster.Pet != null ? PetOutputDTO.FromDbPet(foster.Pet) : null
        };
    }
}
