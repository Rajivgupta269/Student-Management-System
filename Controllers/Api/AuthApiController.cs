using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ScholarHub.Services;
using ScholarHub.ViewModels;

namespace ScholarHub.Controllers.Api;

[ApiController, AllowAnonymous]
[Route("api/auth")]
public class AuthApiController : ControllerBase
{
    private readonly UserManager<IdentityUser> _users;
    private readonly SignInManager<IdentityUser> _signIn;
    private readonly TokenService _tokens;

    public AuthApiController(UserManager<IdentityUser> users, SignInManager<IdentityUser> signIn, TokenService tokens)
    {
        _users = users;
        _signIn = signIn;
        _tokens = tokens;
    }

    /// <summary>POST api/auth/token  { "email": "...", "password": "..." }  ->  JWT bearer token</summary>
    [HttpPost("token")]
    public async Task<IActionResult> Token(TokenRequest request)
    {
        var user = await _users.FindByEmailAsync(request.Email);
        if (user != null)
        {
            var result = await _signIn.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                var (token, expires) = _tokens.Create(user);
                return Ok(new { token, expiresUtc = expires });
            }
        }
        return Unauthorized(new ProblemDetails { Title = "Invalid email or password", Status = 401 });
    }
}
