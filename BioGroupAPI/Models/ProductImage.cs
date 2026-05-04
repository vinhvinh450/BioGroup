namespace BioGroupAPI.Models;

public class ProductImage
{
    public int Id { get; set; }

    public string ImagePath { get; set; } = string.Empty;

    public string? AltText { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsMainImage { get; set; }

    public int ProductId { get; set; }

    public Product Product { get; set; }
}