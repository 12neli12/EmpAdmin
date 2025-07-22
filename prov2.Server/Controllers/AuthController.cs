using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prov2.Server.Data;
using prov2.Server.ViewModels;
using prov2.Server.Services;

namespace prov2.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _database;
    private readonly TokenService _token_Service;

    public AuthController(AppDbContext db, TokenService tokenService)
    {
        _database = db;
        _token_Service = tokenService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _database.Users.SingleOrDefaultAsync(u => u.Username == request.Username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized("Invalid credentials");

        var token = _token_Service.CreateToken(user);

        return Ok(new AuthResponse
        {
            Token = token,
            Role = user.Role,
            FullName = user.FullName
        });
    }
}
