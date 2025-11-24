using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

using PetRescue.ApiDTOs;
using PetRescue.Services;


namespace PetRescue.ApiControllers;

[ApiController]
[EnableCors("default")]
[Route("api/stats")]
public class StatsApiController : ControllerBase
{
    private readonly FosterService _fosterService;
    private readonly PetService _petService;

    public StatsApiController(FosterService fosterService, PetService petService)
    {
        _fosterService = fosterService;
        _petService = petService;
    }


    [HttpGet]
    [Route("")]
    public async Task<ActionResult<StatsOutputDTO>> GetAllStats()
    {
        var nrOfPets = await _petService.GetTotalNrOfPets();
        var nrOfFoster = await _fosterService.GetTotalNrOfFoster();
        var nrOfFosteredPets = await _fosterService.GetNrOfFosteredPets();
        var avgFosterDuration = await _fosterService.GetAvgFosterDuration();

        var outStats = new StatsOutputDTO()
        {
            NrOfPets = nrOfPets,
            NrOfFoster = nrOfFoster,
            NrOfFosteredPets = nrOfFosteredPets,
            AvgFosterDuration = avgFosterDuration,
        };
        return Ok(outStats);
    }
}