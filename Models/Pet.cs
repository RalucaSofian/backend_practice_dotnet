using System.ComponentModel.DataAnnotations;


namespace PetRescue.Models;

public class Pet
{
    [Required]
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public AnimalSpecies Species { get; set; }
    public AnimalGender Gender { get; set; }

    [Range(0, 30)]
    public int? Age { get; set; }
    public string? Description { get; set; }

    public virtual ICollection<Foster> Fosters { get; set; } = new List<Foster>();
}

public enum AnimalGender { M, F }
public enum AnimalSpecies { Cat, Dog, Rodent, Bird, Snake, Lizard, Other }
