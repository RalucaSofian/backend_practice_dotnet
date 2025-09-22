// Entry point for the React app (serve built html)
using Microsoft.AspNetCore.Mvc;

namespace PetRescue.Controllers;

[Route("")]
public class IndexController : Controller
{
    [Route("")]
    public IActionResult Index()
    {
        return Redirect("/admin");
    }
};
