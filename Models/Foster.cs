using System.ComponentModel.DataAnnotations;


namespace PetRescue.Models;

public class Foster : IValidatableObject
{
    [Required]
    public int Id { get; set; }
    public int ClientID { get; set; }
    public int PetID { get; set; }

    [StringLength(100, MinimumLength = 3)]
    public string? Description { get; set; }

    [Required]
    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public virtual Client? Client { get; set; } = null!;
    public virtual Pet? Pet { get; set; } = null!;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EndDate <= StartDate)
        {
            yield return new ValidationResult("End Date must be greater than Start Date.");
        }

        if (EndDate < StartDate.AddDays(14))
        {
            yield return new ValidationResult("Foster period must be at least 14 Days.");
        }
    }
}
