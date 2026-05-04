using BioGroupAPI.Data;
using BioGroupAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BioGroupAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DistributorController : ControllerBase
{
    private readonly BioGroupContext _context;
    private readonly ILogger<DistributorController> _logger;

    public DistributorController(BioGroupContext context, ILogger<DistributorController> logger)
    {
        _context = context;
        _logger = logger;
    }

    int? CurrentUserId() =>
        int.TryParse(User.FindFirst("userId")?.Value, out var id) ? id : null;

    string? CurrentRole() =>
        User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("role")?.Value;

    // ─────────────────────────────────────────────────────────────
    // PUBLIC: sản phẩm distributor đã được duyệt
    // ─────────────────────────────────────────────────────────────
    [HttpGet("approved")]
    public async Task<IActionResult> GetApprovedProducts()
    {
        var products = await _context.DistributorProducts
            .Include(p => p.Distributor)
            .Where(p => p.Status == "Approved" && p.Stock > 0)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new {
                p.Id, p.Name, p.Category, p.Price, p.Stock,
                p.Description, p.ImageUrl, p.Icon,
                distributorName = p.Distributor.BusinessName,
                distributorId   = p.DistributorId,
                isDistributor   = true
            })
            .ToListAsync();
        return Ok(products);
    }

    // ─────────────────────────────────────────────────────────────
    // DISTRIBUTOR: đăng ký
    // ─────────────────────────────────────────────────────────────
    [HttpPost("apply")]
    [Authorize]
    public async Task<IActionResult> Apply([FromBody] ApplyRequest req)
    {
        var uid = CurrentUserId();
        if (uid == null) return Unauthorized();

        var existing = await _context.DistributorProfiles.FirstOrDefaultAsync(d => d.UserId == uid);
        if (existing != null)
            return BadRequest(new { message = "Bạn đã có đơn đăng ký trước đó." });

        var profile = new DistributorProfile
        {
            UserId          = uid.Value,
            BusinessName    = req.BusinessName  ?? string.Empty,
            BusinessAddress = req.BusinessAddress ?? string.Empty,
            TaxCode         = req.TaxCode        ?? string.Empty,
            ContactPhone    = req.ContactPhone    ?? string.Empty,
            Description     = req.Description    ?? string.Empty,
            Status          = "Pending"
        };
        _context.DistributorProfiles.Add(profile);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Đã gửi đơn thành công", id = profile.Id });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        var uid = CurrentUserId();
        if (uid == null) return Unauthorized();

        var profile = await _context.DistributorProfiles
            .FirstOrDefaultAsync(d => d.UserId == uid);

        if (profile == null) return NotFound(new { message = "Chưa đăng ký nhà phân phối." });
        return Ok(profile);
    }

    // ─────────────────────────────────────────────────────────────
    // DISTRIBUTOR: quản lý sản phẩm của mình
    // ─────────────────────────────────────────────────────────────
    [HttpGet("products")]
    [Authorize]
    public async Task<IActionResult> GetMyProducts()
    {
        var uid = CurrentUserId();
        if (uid == null) return Unauthorized();

        var profile = await _context.DistributorProfiles.FirstOrDefaultAsync(d => d.UserId == uid);
        if (profile == null || profile.Status != "Approved")
            return Forbid();

        var products = await _context.DistributorProducts
            .Where(p => p.DistributorId == profile.Id)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
        return Ok(products);
    }

    [HttpPost("products")]
    [Authorize]
    public async Task<IActionResult> AddProduct([FromBody] ProductRequest req)
    {
        var uid = CurrentUserId();
        if (uid == null) return Unauthorized();

        var profile = await _context.DistributorProfiles.FirstOrDefaultAsync(d => d.UserId == uid);
        if (profile == null || profile.Status != "Approved")
            return BadRequest(new { message = "Tài khoản chưa được duyệt làm nhà phân phối." });

        var product = new DistributorProduct
        {
            DistributorId = profile.Id,
            Name          = req.Name,
            Category      = req.Category,
            Price         = req.Price,
            Stock         = req.Stock,
            Description   = req.Description,
            ImageUrl      = req.ImageUrl,
            Icon          = string.IsNullOrEmpty(req.Icon) ? "📦" : req.Icon,
            Status        = "Pending"
        };
        _context.DistributorProducts.Add(product);
        await _context.SaveChangesAsync();
        return Ok(product);
    }

    [HttpPut("products/{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductRequest req)
    {
        var uid = CurrentUserId();
        if (uid == null) return Unauthorized();

        var profile = await _context.DistributorProfiles.FirstOrDefaultAsync(d => d.UserId == uid);
        if (profile == null) return Forbid();

        var product = await _context.DistributorProducts.FindAsync(id);
        if (product == null || product.DistributorId != profile.Id)
            return NotFound();

        product.Name        = req.Name;
        product.Category    = req.Category;
        product.Price       = req.Price;
        product.Stock       = req.Stock;
        product.Description = req.Description;
        product.ImageUrl    = req.ImageUrl;
        product.Icon        = string.IsNullOrEmpty(req.Icon) ? "📦" : req.Icon;
        product.Status      = "Pending"; // reset về pending khi sửa
        product.AdminNote   = string.Empty;

        await _context.SaveChangesAsync();
        return Ok(product);
    }

    [HttpDelete("products/{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var uid = CurrentUserId();
        if (uid == null) return Unauthorized();

        var profile = await _context.DistributorProfiles.FirstOrDefaultAsync(d => d.UserId == uid);
        if (profile == null) return Forbid();

        var product = await _context.DistributorProducts.FindAsync(id);
        if (product == null || product.DistributorId != profile.Id)
            return NotFound();

        _context.DistributorProducts.Remove(product);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Đã xóa sản phẩm." });
    }

    // ─────────────────────────────────────────────────────────────
    // ADMIN: quản lý đơn đăng ký
    // ─────────────────────────────────────────────────────────────
    [HttpGet("admin/applications")]
    [Authorize]
    public async Task<IActionResult> AdminGetApplications()
    {
        if (CurrentRole() != "Admin") return Forbid();

        var list = await _context.DistributorProfiles
            .Include(d => d.User)
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new {
                d.Id, d.BusinessName, d.BusinessAddress, d.TaxCode,
                d.ContactPhone, d.Description, d.Status, d.AdminNote,
                d.CreatedAt, d.ReviewedAt,
                userName  = d.User.Name,
                userEmail = d.User.Email
            })
            .ToListAsync();
        return Ok(list);
    }

    public record ApplyRequest(string BusinessName, string BusinessAddress, string TaxCode, string ContactPhone, string Description);
    public record StatusUpdate(string Status, string AdminNote);
    public record ProductRequest(string Name, string Category, decimal Price, int Stock, string Description, string? ImageUrl, string Icon);

    [HttpPut("admin/applications/{id}")]
    [Authorize]
    public async Task<IActionResult> AdminReviewApplication(int id, [FromBody] StatusUpdate req)
    {
        if (CurrentRole() != "Admin") return Forbid();

        var profile = await _context.DistributorProfiles
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == id);
        if (profile == null) return NotFound();

        profile.Status     = req.Status;
        profile.AdminNote  = req.AdminNote ?? string.Empty;
        profile.ReviewedAt = DateTime.UtcNow;

        // Cập nhật role user khi duyệt / huỷ
        if (req.Status == "Approved")
            profile.User.Role = "Distributor";
        else if (req.Status == "Rejected" && profile.User.Role == "Distributor")
            profile.User.Role = "User";

        await _context.SaveChangesAsync();
        return Ok(profile);
    }

    // ─────────────────────────────────────────────────────────────
    // ADMIN: quản lý sản phẩm nhà phân phối
    // ─────────────────────────────────────────────────────────────
    [HttpGet("admin/products")]
    [Authorize]
    public async Task<IActionResult> AdminGetProducts([FromQuery] string status = "")
    {
        if (CurrentRole() != "Admin") return Forbid();

        var query = _context.DistributorProducts
            .Include(p => p.Distributor)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(p => p.Status == status);

        var list = await query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new {
                p.Id, p.Name, p.Category, p.Price, p.Stock,
                p.Description, p.ImageUrl, p.Icon, p.Status, p.AdminNote, p.CreatedAt,
                distributorName = p.Distributor.BusinessName,
                distributorId   = p.DistributorId
            })
            .ToListAsync();
        return Ok(list);
    }

    [HttpPut("admin/products/{id}")]
    [Authorize]
    public async Task<IActionResult> AdminReviewProduct(int id, [FromBody] StatusUpdate req)
    {
        if (CurrentRole() != "Admin") return Forbid();

        var product = await _context.DistributorProducts.FindAsync(id);
        if (product == null) return NotFound();

        product.Status    = req.Status;
        product.AdminNote = req.AdminNote ?? string.Empty;
        await _context.SaveChangesAsync();
        return Ok(product);
    }
}
