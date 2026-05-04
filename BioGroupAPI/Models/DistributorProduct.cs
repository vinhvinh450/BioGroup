namespace BioGroupAPI.Models;

public class DistributorProduct
{
    public int Id             { get; set; }
    public int DistributorId  { get; set; }
    public DistributorProfile Distributor { get; set; } = null!;

    public string  Name        { get; set; } = string.Empty;
    public string  Category    { get; set; } = string.Empty;
    public decimal Price       { get; set; }
    public int     Stock       { get; set; }
    public string  Description { get; set; } = string.Empty;
    public string? ImageUrl    { get; set; }
    public string  Icon        { get; set; } = "📦";

    // Pending | Approved | Rejected
    public string Status    { get; set; } = "Pending";
    public string AdminNote { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
