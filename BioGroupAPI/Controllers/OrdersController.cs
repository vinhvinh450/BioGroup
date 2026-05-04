using BioGroupAPI.Data;
using BioGroupAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BioGroupAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly BioGroupContext _context;

    public OrdersController(BioGroupContext context)
    {
        _context = context;
    }

    // ✅ GET: api/orders — Lấy đơn hàng của user đang đăng nhập
    [HttpGet]
    public async Task<IActionResult> GetUserOrders()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized();

        var orders = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Where(o => o.UserId == userId.Value)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return Ok(orders.Select(MapOrderToDto));
    }

    // ✅ GET: api/orders/all — Lấy TẤT CẢ đơn hàng (chỉ Admin)
    [HttpGet("all")]
    public async Task<IActionResult> GetAllOrders()
    {
        var roleClaim = User.FindFirst("role")?.Value
                     ?? User.FindFirst(ClaimTypes.Role)?.Value;

        if (roleClaim != "Admin")
            return Forbid();

        var orders = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.User)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return Ok(orders.Select(o => new
        {
            o.Id,
            o.UserId,
            o.TotalAmount,
            Status = o.Status.ToString(),
            o.ShippingAddress,
            o.Phone,
            o.Notes,
            o.CreatedAt,
            o.UpdatedAt,
            // ✅ Trả về tên user cho Admin dashboard
            User = o.User == null ? null : new
            {
                o.User.Id,
                o.User.Name,
                o.User.Email,
                o.User.Phone
            },
            Items = o.OrderItems.Select(i => new
            {
                i.Id,
                i.ProductId,
                ProductName = i.Product?.Name ?? "",
                ProductIcon = i.Product?.Icon ?? "📦",
                i.Quantity,
                i.UnitPrice,
                SubTotal = i.Quantity * i.UnitPrice
            })
        }));
    }

    // ✅ GET: api/orders/{id} — Lấy chi tiết 1 đơn hàng
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized();

        var order = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId.Value);

        if (order == null)
            return NotFound();

        return Ok(MapOrderToDto(order));
    }

    // ✅ GET: api/orders/{id}/tracking — Lịch sử theo dõi
    [HttpGet("{id}/tracking")]
    public async Task<IActionResult> GetOrderTracking(int id)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized();

        var tracking = await _context.OrderTrackings
            .Where(ot => ot.OrderId == id && ot.Order!.UserId == userId.Value)
            .OrderBy(ot => ot.Timestamp)
            .ToListAsync();

        if (!tracking.Any())
            return NotFound();

        return Ok(tracking.Select(t => new
        {
            t.Id,
            t.OrderId,
            Status = t.Status.ToString(),
            t.Description,
            t.Timestamp
        }));
    }

    // ✅ POST: api/orders — Tạo đơn hàng mới
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
            return Unauthorized();

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        decimal totalAmount = 0;
        var orderItems = new List<OrderItem>();

        foreach (var item in request.Items)
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product == null)
                return BadRequest(new { message = $"Sản phẩm {item.ProductId} không tồn tại" });

            if (product.Stock < item.Quantity)
                return BadRequest(new { message = $"Sản phẩm '{product.Name}' chỉ còn {product.Stock} sản phẩm trong kho" });

            totalAmount += product.Price * item.Quantity;

            orderItems.Add(new OrderItem
            {
                ProductId = item.ProductId,
                Quantity  = item.Quantity,
                UnitPrice = product.Price
            });
        }

        var order = new Order
        {
            UserId          = userId.Value,
            TotalAmount     = totalAmount,
            Status          = OrderStatus.Pending,
            ShippingAddress = request.ShippingAddress,
            Phone           = request.Phone,
            Notes           = request.Notes,
            OrderItems      = orderItems,
            CreatedAt       = DateTime.UtcNow
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Giảm tồn kho
        foreach (var item in request.Items)
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product != null)
            {
                product.Stock -= item.Quantity;
                _context.Products.Update(product);
            }
        }
        await _context.SaveChangesAsync();

        // Thêm tracking ban đầu
        _context.OrderTrackings.Add(new OrderTracking
        {
            OrderId     = order.Id,
            Status      = OrderStatus.Pending,
            Description = "Đơn hàng đã được tạo"
        });
        await _context.SaveChangesAsync();

        // ✅ Chỉ return id để tránh circular reference
        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, new { id = order.Id });
    }

    // ✅ PUT: api/orders/{id}/status — Cập nhật trạng thái (Admin)
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusRequest request)
    {
        var roleClaim = User.FindFirst("role")?.Value
                     ?? User.FindFirst(ClaimTypes.Role)?.Value;

        if (roleClaim != "Admin")
            return Forbid();

        // ✅ Include OrderItems để hoàn tồn kho khi hủy đơn
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return NotFound();

        if (!Enum.IsDefined(typeof(OrderStatus), request.Status))
            return BadRequest(new { message = "Trạng thái không hợp lệ" });

        var oldStatus = order.Status;
        order.Status    = request.Status;
        order.UpdatedAt = DateTime.UtcNow;

        // Hoàn tồn kho nếu hủy đơn
        if (request.Status == OrderStatus.Cancelled && oldStatus != OrderStatus.Cancelled)
        {
            foreach (var item in order.OrderItems)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.Stock += item.Quantity;
                    _context.Products.Update(product);
                }
            }
        }

        _context.OrderTrackings.Add(new OrderTracking
        {
            OrderId     = id,
            Status      = request.Status,
            Description = request.Description ?? $"Trạng thái cập nhật thành {request.Status}"
        });

        await _context.SaveChangesAsync();

        return Ok(MapOrderToDto(order));
    }

    // ✅ Helper: Map Order → DTO (không circular reference)
    private static object MapOrderToDto(Order order) => new
    {
        order.Id,
        order.UserId,
        order.TotalAmount,
        Status          = order.Status.ToString(),
        order.ShippingAddress,
        order.Phone,
        order.Notes,
        order.CreatedAt,
        order.UpdatedAt,
        Items = order.OrderItems.Select(i => new
        {
            i.Id,
            i.ProductId,
            ProductName = i.Product?.Name ?? "",
            ProductIcon = i.Product?.Icon ?? "📦",
            i.Quantity,
            i.UnitPrice,
            SubTotal    = i.Quantity * i.UnitPrice
        })
    };

    private int? GetCurrentUserId()
    {
        var claim = User.FindFirst("userId")?.Value;
        return int.TryParse(claim, out var id) ? id : null;
    }
}

public class UpdateOrderStatusRequest
{
    public OrderStatus Status      { get; set; }
    public string?     Description { get; set; }
}