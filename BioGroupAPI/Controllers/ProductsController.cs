using BioGroupAPI.Data;
using BioGroupAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BioGroupAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly BioGroupContext _context;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(BioGroupContext context, ILogger<ProductsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/products
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
    {
        try
        {
            var products = await _context.Products
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            return Ok(products);
        }
        catch (Exception ex)
        { 
            _logger.LogError(ex, "Error fetching products");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    // GET: api/products/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        try
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound(new { message = "Product not found" });
            }

            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching product {id}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    // GET: api/products/category/{category}
    [HttpGet("category/{category}")]
    public async Task<ActionResult<IEnumerable<Product>>> GetProductsByCategory(string category)
    {
        try
        {
            var products = await _context.Products
                .Where(p => p.Category == category)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching products by category {category}", category);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    // GET: api/products/featured
    [HttpGet("featured")]
    public async Task<ActionResult<IEnumerable<Product>>> GetFeaturedProducts()
    {
        try
        {
            var products = await _context.Products
                .Where(p => p.IsFeatured)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching featured products");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    // GET: api/products/search/{keyword}
    [HttpGet("search/{keyword}")]
    public async Task<ActionResult<IEnumerable<Product>>> SearchProducts(string keyword)
    {
        try
        {
            var lowerKeyword = keyword.ToLower();
            var products = await _context.Products
                .Where(p => p.Name.ToLower().Contains(lowerKeyword) ||
                           p.Category.ToLower().Contains(lowerKeyword) ||
                           p.Description.ToLower().Contains(lowerKeyword))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching products with keyword {keyword}", keyword);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    // POST: api/products
    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct(Product product)
    {
        try
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    // PUT: api/products/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, Product product)
    {
        if (id != product.Id)
        {
            return BadRequest(new { message = "Product ID mismatch" });
        }

        try
        {
            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null)
            {
                return NotFound(new { message = "Product not found" });
            }

            existingProduct.Name = product.Name;
            existingProduct.Category = product.Category;
            existingProduct.Badge = product.Badge;
            existingProduct.BadgeClass = product.BadgeClass;
            existingProduct.Icon = product.Icon;
            existingProduct.ImageUrl = product.ImageUrl;
            existingProduct.Description = product.Description;
            existingProduct.Price = product.Price;
            existingProduct.Stock = product.Stock;
            existingProduct.IsFeatured = product.IsFeatured;
            existingProduct.IsActive = product.IsActive;
            existingProduct.DiscountPercent = Math.Clamp(product.DiscountPercent, 0, 100);
            existingProduct.DiscountEndDate = product.DiscountEndDate;

            _context.Products.Update(existingProduct);
            await _context.SaveChangesAsync();

            return Ok(existingProduct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {id}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    // DELETE: api/products/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        try
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound(new { message = "Product not found" });
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Product deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {id}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}
