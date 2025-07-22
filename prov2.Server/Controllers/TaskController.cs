using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using prov2.Server.Entities;
using prov2.Server.ViewModels;
using prov2.Server.Data;

namespace prov2.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TaskController : ControllerBase
{
    private readonly AppDbContext _db;

    public TaskController(AppDbContext db) => _db = db;

    [Authorize]
    [HttpGet("project/{projectId}")]
    public async Task<IActionResult> GetProjectTasks(int projectId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isMember = await _db.UserProjects.AnyAsync(up => up.UserId == userId && up.ProjectId == projectId);
        if (!isMember) return Forbid();

        var tasks = await _db.Tasks
            .Include(t => t.AssignedTo)
            .Include(t => t.CreatedBy)
            .Where(t => t.ProjectId == projectId)
            .Select(t => new TaskDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                IsCompleted = t.IsCompleted,
                AssignedTo = t.AssignedTo.FullName,
                CreatedBy = t.CreatedBy.FullName
            })
            .ToListAsync();
        return Ok(tasks);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateTask(CreateTaskRequest req)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isMember = await _db.UserProjects.AnyAsync(up => up.UserId == userId && up.ProjectId == req.ProjectId);
        var targetIsMember = await _db.UserProjects.AnyAsync(up => up.UserId == req.AssignedToId && up.ProjectId == req.ProjectId);
        if (!isMember || !targetIsMember) return Forbid();

        var task = new ProjectTask
        {
            Title = req.Title,
            Description = req.Description,
            ProjectId = req.ProjectId,
            AssignedToId = req.AssignedToId,
            CreatedById = userId
        };
        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();
        return Ok(task);
    }

    [Authorize]
    [HttpPut("{taskId}/complete")]
    public async Task<IActionResult> MarkComplete(int taskId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == taskId);
        if (task == null) return NotFound();

        if (User.IsInRole("Employee") && task.AssignedToId != userId)
            return Forbid();

        task.IsCompleted = true;
        await _db.SaveChangesAsync();
        return Ok();
    }

    [Authorize(Roles = "Administrator")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var task = await _db.Tasks.FindAsync(id);
        if (task == null) return NotFound();
        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateTaskRequest req)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var task = await _db.Tasks.FindAsync(id);
        if (task == null) return NotFound();

        // Allow if Administrator OR assigned user
        var isAdmin = User.IsInRole("Administrator");
        var isAssignedUser = task.AssignedToId == userId;

        if (!isAdmin && !isAssignedUser)
            return Forbid();

        task.Title = req.Title;
        task.Description = req.Description;
        task.AssignedToId = req.AssignedToId;

        await _db.SaveChangesAsync();
        return Ok(task);
    }


}
