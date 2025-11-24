namespace PetRescue.ApiDTOs;

public class StatsOutputDTO
{
    public int NrOfPets { get; set; } = default!;
    public int NrOfFoster { get; set; } = default!;
    public int NrOfFosteredPets { get; set; } = default!;
    public float AvgFosterDuration { get; set; } = default!;
}
