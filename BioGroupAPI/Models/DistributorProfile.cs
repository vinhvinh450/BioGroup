namespace BioGroupAPI.Models;

public class DistributorProfile
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string BusinessName    { get; set; } = string.Empty;
    public string BusinessAddress { get; set; } = string.Empty;
    public string TaxCode         { get; set; } = string.Empty;
    public string ContactPhone    { get; set; } = string.Empty;
    public string Description     { get; set; } = string.Empty;

    // Pending | Approved | Rejected
    public string Status    { get; set; } = "Pending";
    public string AdminNote { get; set; } = string.Empty;

    public DateTime  CreatedAt   { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt  { get; set; }

    public List<DistributorProduct> Products { get; set; } = new();
}
