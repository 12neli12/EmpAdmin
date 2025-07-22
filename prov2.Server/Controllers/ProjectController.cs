using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using prov2.Server.Entities;
using prov2.Server.Data;
using prov2.Server.ViewModels;

namespace prov2.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProjectController(AppDbContext db) => _db = db;

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetMyProjects()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr))
            return Unauthorized("No user ID found in token");

        var userId = int.Parse(userIdStr);

        var projects = await _db.UserProjects
            .Include(up => up.Project)
                .ThenInclude(p => p.Tasks)
            .Include(up => up.Project)
                .ThenInclude(p => p.UserProjects)
                    .ThenInclude(up => up.User)
            .Where(up => up.UserId == userId)
            .Select(up => new
            {
                up.Project.Id,
                up.Project.Name,
                up.Project.Description,
                Tasks = up.Project.Tasks.Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.IsCompleted,
                    t.AssignedToId
                }),
                AssignedEmployees = up.Project.UserProjects.Select(up2 => new
                {
                    up2.User.Id,
                    up2.User.FullName
                })
            })
            .ToListAsync();

        return Ok(projects);
    }



    [Authorize(Roles = "Administrator")]
    [HttpPost]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request)
    {
        var project = new Project
        {
            Name = request.Name,
            Description = request.Description,
            UserProjects = request.EmployeeIds.Select(empId => new UserProject
            {
                UserId = empId
            }).ToList()
        };

        _db.Projects.Add(project);
        await _db.SaveChangesAsync();

        return Ok(new
        {
            project.Id,
            project.Name,
            AssignedEmployees = request.EmployeeIds
        });
    }

    [Authorize(Roles = "Administrator")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProject(int id)
    {
        var project = await _db.Projects
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null)
            return NotFound();

        if (project.Tasks.Any(t => !t.IsCompleted))
            return BadRequest("Cannot delete a project with open tasks");

        _db.Projects.Remove(project);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [Authorize(Roles = "Administrator")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProject(int id, [FromBody] UpdateProjectRequest req)
    {
        var project = await _db.Projects.FindAsync(id);
        if (project == null) return NotFound();

        project.Name = req.Name;
        project.Description = req.Description;

        await _db.SaveChangesAsync();
        return Ok(project);
    }

    [Authorize(Roles = "Administrator")]
    [HttpPost("{projectId}/add-member/{userId}")]
    public async Task<IActionResult> AddMember(int projectId, int userId)
    {
        var exists = await _db.UserProjects.AnyAsync(up => up.UserId == userId && up.ProjectId == projectId);
        if (exists) return BadRequest("User already in project.");

        _db.UserProjects.Add(new UserProject { UserId = userId, ProjectId = projectId });
        await _db.SaveChangesAsync();
        return Ok();
    }

    [Authorize(Roles = "Administrator")]
    [HttpDelete("{projectId}/remove-member/{userId}")]
    public async Task<IActionResult> RemoveMember(int projectId, int userId)
    {
        var up = await _db.UserProjects.FirstOrDefaultAsync(up => up.ProjectId == projectId && up.UserId == userId);
        if (up == null) return NotFound();

        _db.UserProjects.Remove(up);
        await _db.SaveChangesAsync();
        return NoContent();
    }

}
