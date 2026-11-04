using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ScholarHub.Controllers;

[AllowAnonymous]
public class ErrorController : Controller
{
    [Route("Error/{code:int}")]
    public IActionResult Index(int code)
    {
        Response.StatusCode = code;
        ViewBag.Code = code;
        ViewBag.Message = code switch
        {
            404 => "We couldn't find the page you were looking for.",
            403 => "You do not have permission to open this page.",
            _ => "Something went wrong on our side. Please try again."
        };
        return View();
    }
}
