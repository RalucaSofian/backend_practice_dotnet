using Microsoft.AspNetCore.Mvc;

using PetRescue.ApiDTOs;
using PetRescue.Models;
using PetRescue.Services;


namespace PetRescue.ApiControllers;

[ApiController]
[Route("api/pets")]
public class PetApiController : ControllerBase
{
    private readonly PetService _petService;

    public PetApiController(PetService petService)
    {
        _petService = petService;
    }


    [HttpGet]
    [Route("")]
    public async Task<ActionResult<PaginatedListDTO<PetOutputDTO>>> GetPets([FromQuery] string? searchString, [FromQuery] AnimalSpecies? animalSpecies,
                                                              [FromQuery] AnimalGender? animalGender, [FromQuery] int? age_gte,
                                                              [FromQuery] int? age_lte, [FromQuery] string? sortOrder,
                                                              [FromQuery] int pageSize = 6, [FromQuery] int pageNumber = 1)
    {
        var queriedPets = await _petService.QueryPets(searchString, animalSpecies,
            animalGender, age_gte, age_lte, sortOrder, pageSize, pageNumber);
        if (queriedPets == null)
        {
            return NotFound();
        }

        // Imperative variant
        // var outputPets = new List<PetOutputDTO>();
        // foreach (var pet in queriedPets)
        // {
        //     outputPets.Add(PetOutputDTO.FromDbPet(pet));
        // }

        var outputPets = queriedPets.Select(PetOutputDTO.FromDbPet);
        var paginatedPets = PaginatedListDTO<PetOutputDTO>.ConvertList(outputPets.ToList(), queriedPets);
        return Ok(paginatedPets);
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<ActionResult<PetOutputDTO>> GetPet([FromRoute] int id)
    {
        var pet = await _petService.GetPet(id);
        if (pet == null)
        {
            return NotFound();
        }

        var outputPet = PetOutputDTO.FromDbPet(pet);
        return Ok(outputPet);
    }
}
