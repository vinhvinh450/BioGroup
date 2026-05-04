// Controllers/UsersController.cs
using BioGroupAPI.Data;
using BioGroupAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BioGroupAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")] // Thêm [Authorize(Roles = "Admin")] nếu bạn có role system
public class UsersController : ControllerBase
{
    private readonly BioGroupContext _context;

    public UsersController(BioGroupContext context)
    {
        _context = context;
    }

    // GET /api/users — Lấy danh sách tất cả user (Admin only)
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _context.Users
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new UserDto
            {
                Id        = u.Id,
                Name      = u.Name,
                Email     = u.Email,
                Phone     = u.Phone,
                Address   = u.Address,
                Role      = u.Role,
                IsActive  = u.IsActive,
                CreatedAt = u.CreatedAt,
            })
            .ToListAsync();

        return Ok(users);
    }

    // GET /api/users/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound(new { message = "Không tìm thấy người dùng" });

        return Ok(new UserDto
        {
            Id        = user.Id,
            Name      = user.Name,
            Email     = user.Email,
            Phone     = user.Phone,
            Address   = user.Address,
            Role      = user.Role,
            CreatedAt = user.CreatedAt,
        });
    }

    // PUT /api/users/{id}/toggle-status — Khóa / mở khóa tài khoản
    [HttpPut("{id}/toggle-status")]
    public async Task<IActionResult> ToggleStatus(int id, [FromBody] ToggleStatusRequest request)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound(new { message = "Không tìm thấy người dùng" });

        user.IsActive = request.IsActive;
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message  = request.IsActive ? "Đã mở khóa tài khoản" : "Đã khóa tài khoản",
            isActive = user.IsActive,
        });
    }

    // PUT /api/users/{id}/role — Thay đổi role
    [HttpPut("{id}/role")]
    public async Task<IActionResult> ChangeRole(int id, [FromBody] ChangeRoleRequest request)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound(new { message = "Không tìm thấy người dùng" });

        user.Role = request.Role;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Đã cập nhật role", role = user.Role });
    }
}

// Request model
public class ToggleStatusRequest
{
    public bool IsActive { get; set; }
}

public class ChangeRoleRequest
{
    public string Role { get; set; } = string.Empty;
}