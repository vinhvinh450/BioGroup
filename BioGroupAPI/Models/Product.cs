namespace BioGroupAPI.Models;

public class Product
{
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string Category { get; set; } = string.Empty;
    
    public string Badge { get; set; } = string.Empty;
    
    public string BadgeClass { get; set; } = string.Empty;
    
    public string Icon { get; set; } = string.Empty;
    
    public string? ImageUrl { get; set; }
    
    public string Description { get; set; } = string.Empty;
    
    public decimal Price { get; set; } = 0;
    
    public int Stock { get; set; } = 0; // Số lượng tồn kho
    
    public bool IsFeatured { get; set; } = false;

    public bool IsActive { get; set; } = true;

    public int DiscountPercent { get; set; } = 0; // 0 = không KM

    public DateTime? DiscountEndDate { get; set; } // null = không hết hạn

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<ProductImage> Images { get; set; } =new();
}
