using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

using PetRescue.ApiDTOs;
using PetRescue.Models;
using PetRescue.Services;
using PetRescue.Utilities;


namespace PetRescue.ApiControllers;

[ApiController]
[EnableCors("default")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/foster")]
public class FosterApiController : ControllerBase
{
    private readonly FosterService _fosterService;
    private readonly UserService _userService;
    private readonly ClientService _clientService;
    private readonly PetService _petService;

    public FosterApiController(FosterService fosterService, UserService userService, ClientService clientService, PetService petService)
    {
        _fosterService = fosterService;
        _userService = userService;
        _clientService = clientService;
        _petService = petService;
    }


    [HttpGet]
    [Route("")]
    public async Task<ActionResult<PaginatedListDTO<FosterOutputDTO>>> GetAllFoster()
    {
        var client = await GetCurrentClient();
        if (client == null)
        {
            return Unauthorized();
        }

        var fosterQueryOptions = new FosterService.QueryOptions
        {
            ClientId = client.Id,
            PageSize = 6,
            PageNumber = 1
        };
        var queriedFoster = await _fosterService.QueryFoster(fosterQueryOptions);
        if (queriedFoster == null)
        {
            return NotFound();
        }

        var outputFoster = queriedFoster.Select(FosterOutputDTO.FromDbFoster);
        var paginatedFoster = PaginatedListDTO<FosterOutputDTO>.ConvertList(outputFoster.ToList(), queriedFoster);
        return Ok(paginatedFoster);
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<ActionResult<FosterOutputDTO>> GetFoster([FromRoute] int id)
    {
        var client = await GetCurrentClient();
        if (client == null)
        {
            return Unauthorized();
        }

        var foster = await _fosterService.GetFoster(id);
        if (foster == null)
        {
            return NotFound();
        }

        if (foster.ClientID != client.Id)
        {
            return NotFound();
        }

        var outputFoster = FosterOutputDTO.FromDbFoster(foster);
        return Ok(outputFoster);
    }

    [HttpPost]
    [Route("")]
    public async Task<ActionResult<FosterOutputDTO>> CreateFoster(CreateFosterInputDTO createFosterInput)
    {
        var client = await GetCurrentClient();
        if (client == null)
        {
            return Unauthorized();
        }

        var pet = await _petService.GetPet(createFosterInput.PetId);
        if (pet == null)
        {
            return NotFound();
        }

        var newFoster = new Foster
        {
            ClientID = client.Id,
            PetID = pet.Id,
            Description = createFosterInput.Description,
            StartDate = createFosterInput.StartDate,
            EndDate = createFosterInput.EndDate
        };

        var result = await ValidateFosterDates(newFoster);
        if (result != "Success")
        {
            return BadRequest(result);
        }

        var createdFoster = await _fosterService.CreateFoster(newFoster);
        if (createdFoster == null)
        {
            return BadRequest();
        }

        var outputFoster = FosterOutputDTO.FromDbFoster(createdFoster);
        return Ok(outputFoster);
    }


    private async Task<Client?> GetCurrentClient()
    {
        var crtUser = await AuthUtils.GetCurrentUser(_userService, HttpContext.User);
        if (crtUser == null)
        {
            return null;
        }

        var client = await _clientService.GetClientForUserId(crtUser.Id);
        if (client == null)
        {
            return null;
        }
        return client;
    }

    private async Task<string> ValidateFosterDates(Foster foster)
    {
        var foundFoster = (await _fosterService.GetFosterForPet(foster.PetID)).Where(f => f.Id != foster.Id);

        if (foster.EndDate <= foster.StartDate)
        {
            return "End Date must be greater than Start Date.";
        }

        if (foster.EndDate < foster.StartDate.AddDays(14))
        {
            return "Foster period must be at least 14 Days.";
        }

        if (foundFoster.Any(f => f.EndDate == null && f.StartDate < foster.EndDate))
        {
            return "(Open-ended Foster) Conflicting Foster interval for the same Pet.";
        }

        if (foster.EndDate == null)
        {
            if (foundFoster.Any(f => f.EndDate > foster.StartDate))
            {
                return "(Open-ended Foster) Conflicting Foster interval for the same Pet.";
            }
        }

        if (foundFoster.Any(f => f.EndDate > foster.StartDate && f.StartDate < foster.EndDate))
        {
            return "Conflicting Foster interval for the same Pet.";
        }

        return "Success";
    }
}
