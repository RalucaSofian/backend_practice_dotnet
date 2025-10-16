using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

using PetRescue.Services;
using PetRescue.Utilities;
using PetRescue.ApiDTOs;


namespace PetRescue.ApiControllers;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/users")]
public class UserApiController : ControllerBase
{
    private readonly UserService _userService;

    public UserApiController(UserService userService)
    {
        _userService = userService;
    }


    [HttpGet]
    [Route("me")]
    public async Task<ActionResult<UserOutputDTO>> GetUser()
    {
        var user = await AuthUtils.GetCurrentUser(_userService, HttpContext.User);
        if (user == null)
        {
            return NotFound();
        }

        var outputUser = UserOutputDTO.FromDbUser(user);
        return Ok(outputUser);
    }

    [HttpPatch]
    [Route("me")]
    public async Task<ActionResult<UserOutputDTO>> PatchUser()
    {
        var user = await AuthUtils.GetCurrentUser(_userService, HttpContext.User);
        if (user == null)
        {
            return NotFound();
        }

        var bodyReader = new StreamReader(Request.Body);
        var userJson = await bodyReader.ReadToEndAsync();

        var serializer = new JsonSerializer();
        using (var reader = new StringReader(userJson))
        {
            serializer.Populate(reader, user);
        }

        var editedUser = await _userService.EditUser(user);
        if (editedUser == null)
        {
            return NotFound();
        }
        
        var outputUser = UserOutputDTO.FromDbUser(editedUser);
        return Ok(outputUser);
    }

    [HttpDelete]
    [Route("me")]
    public async Task<ActionResult> DeleteUser()
    {
        var user = await AuthUtils.GetCurrentUser(_userService, HttpContext.User);
        if (user == null)
        {
            return NotFound();
        }

        await _userService.DeleteUser(user.Id);
        return Ok();
    }
}
