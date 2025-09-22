using System.ComponentModel.DataAnnotations;


namespace PetRescue.Models;

public class Client
{
    [Required]
    public int Id { get; set; }

    public string? UserID { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; } = default!;

    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Description { get; set; }

    public virtual User? User { get; set; } = null!;
}
