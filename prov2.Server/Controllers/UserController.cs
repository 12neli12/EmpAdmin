using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using prov2.Server.Data;
using prov2.Server.Entities;
using prov2.Server.Services;
using prov2.Server.ViewModels;
using System.IO;
using System.Security.Claims;

namespace prov2.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AutheController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly TokenService _tokenService;
    private readonly IWebHostEnvironment _env;

    public AutheController(AppDbContext db, TokenService tokenService, IWebHostEnvironment env)
    {
        _db = db;
        _tokenService = tokenService;
        _env = env;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _db.Users.SingleOrDefaultAsync(u => u.Username == request.Username);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized("Invalid credentials");

        var token = _tokenService.CreateToken(user);

        return Ok(new AuthResponse
        {
            Token = token,
            Role = user.Role,
            FullName = user.FullName
        });
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var username = User.FindFirst(ClaimTypes.Name)?.Value;

        if (string.IsNullOrEmpty(username))
            return Unauthorized("Username not found in token.");

        var user = await _db.Users.SingleOrDefaultAsync(u => u.Username == username);

        if (user == null)
            return NotFound("User not found.");

        // Generate the full URL for ProfilePictureUrl if it is a relative path
        string profileUrl = null;
        if (!string.IsNullOrEmpty(user.ProfilePictureUrl) && !user.ProfilePictureUrl.StartsWith("http"))
        {
            profileUrl = $"{Request.Scheme}://{Request.Host}{user.ProfilePictureUrl}";
        }
        else
        {
            profileUrl = user.ProfilePictureUrl;
        }

        return Ok(new
        {
            user.Id,
            user.Username,
            user.FullName,
            user.Role,
            ProfilePictureUrl = profileUrl
        });
    }


    [Authorize(Roles = "Administrator")]
    [HttpPut("employees/{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] CreateUserRequest request)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null)
            return NotFound();

        user.Username = request.Username;
        user.FullName = request.FullName;
        user.Role = request.Role;

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        }

        await _db.SaveChangesAsync();
        return Ok(new { message = "User updated successfully" });
    }



    [Authorize(Roles = "Administrator")]
    [HttpGet("employees")]
    public async Task<IActionResult> GetEmployees()
    {
        try
        {
            var users = await _db.Users
                .Select(u => new { u.Id, u.FullName, u.Username, u.Role })
                .ToListAsync();

            return Ok(users);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }


    [Authorize(Roles = "Administrator")]
    [HttpPost("create")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        // Check if user already exists
        var existingUser = await _db.Users.SingleOrDefaultAsync(u => u.Username == request.Username);
        if (existingUser != null)
        {
            return BadRequest("Username already exists");
        }

        // Create new user
        var newUser = new User
        {
            Username = request.Username,
            FullName = request.FullName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role // "User" or "Administrator"
        };

        // Add to database
        _db.Users.Add(newUser);
        await _db.SaveChangesAsync();

        return Ok(new { message = "User created successfully", user = newUser });
    }

    [Authorize(Roles = "Administrator")]
    [HttpDelete("employees/{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
            return NotFound("User not found");

        var isAssigned = await _db.Tasks.AnyAsync(t => t.AssignedToId == id);
        if (isAssigned)
            return BadRequest("This user is assigned to one or more tasks and cannot be deleted.");

        user.IsDeleted = true;
        await _db.SaveChangesAsync();

        return Ok(new { message = "User soft-deleted successfully" });
    }



    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileRequest req)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return NotFound();

        user.FullName = req.FullName;

        // Update password if a new one is provided
        if (!string.IsNullOrWhiteSpace(req.NewPassword))
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);

        // Validate and save profile picture if provided
        if (req.ProfilePicture != null && req.ProfilePicture.Length > 0)
        {
            // Validate file extension
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(req.ProfilePicture.FileName).ToLower();

            if (!allowedExtensions.Contains(fileExtension))
                return BadRequest("Invalid file type.");

            // Validate file size (max 5 MB)
            if (req.ProfilePicture.Length > 5 * 1024 * 1024) // 5 MB max size
                return BadRequest("File size exceeds 5 MB.");

            // Save file to the uploads folder
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);

            var fileName = $"profile_{user.Id}_{Path.GetRandomFileName()}{fileExtension}";
            var filePath = Path.Combine(uploadsPath, fileName);

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await req.ProfilePicture.CopyToAsync(stream);
                }

                // Save only the relative path in the database
                user.ProfilePictureUrl = $"/uploads/{fileName}";  // Relative path to the file
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error while saving profile picture.");
            }
        }

        await _db.SaveChangesAsync();
        return Ok();
    }
}
