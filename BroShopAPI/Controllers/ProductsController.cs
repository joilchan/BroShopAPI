using BroShopAPI.Data;
using BroShopAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BroShopAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
        {
            return await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.ProductType)
                .Include(p => p.ProductVariants)
                .Select(p => new ProductDto
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Price = p.Price,
                    Description = p.Description,
                    ImageUrl = p.ImageUrl,
                    Discount = p.Discount,
                    BrandId = p.BrandId,
                    BrandName = p.Brand != null ? p.Brand.Name : null,
                    ProductTypeId = p.ProductTypeId,
                    ProductTypeName = p.ProductType != null ? p.ProductType.Name : null,
                    ProductVariants = p.ProductVariants.Select(v => new ProductVariantDto
                    {
                        ProductVariantId = v.ProductVariantId,
                        Size = v.Size,
                        StockQuantity = v.StockQuantity
                    }).ToList()
                })
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            var productDto = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.ProductType)
                .Include(p => p.ProductVariants)
                .Where(p => p.ProductId == id)
                .Select(p => new ProductDto
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Price = p.Price,
                    Description = p.Description,
                    ImageUrl = p.ImageUrl,
                    Discount = p.Discount,
                    BrandId = p.BrandId,
                    BrandName = p.Brand != null ? p.Brand.Name : null,
                    ProductTypeId = p.ProductTypeId,
                    ProductTypeName = p.ProductType != null ? p.ProductType.Name : null,
                    ProductVariants = p.ProductVariants.Select(v => new ProductVariantDto
                    {
                        ProductVariantId = v.ProductVariantId,
                        Size = v.Size,
                        StockQuantity = v.StockQuantity
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (productDto == null) return NotFound();
            return productDto;
        }

        [HttpPost]
        public async Task<ActionResult<ProductDto>> PostProduct(ProductDto dto)
        {
            var product = new Product
            {
                Name = dto.Name,
                Price = dto.Price,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                Discount = dto.Discount,
                BrandId = dto.BrandId,
                ProductTypeId = dto.ProductTypeId
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Добавляем варианты
            foreach (var vDto in dto.ProductVariants)
            {
                _context.ProductVariants.Add(new ProductVariant { ProductId = product.ProductId, Size = vDto.Size, StockQuantity = vDto.StockQuantity });
            }
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = product.ProductId }, dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, ProductDto dto)
        {
            if (id != dto.ProductId) return BadRequest("ID mismatch");

            var existingProduct = await _context.Products
                .Include(p => p.ProductVariants)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (existingProduct == null) return NotFound();

            existingProduct.Name = dto.Name;
            existingProduct.Price = dto.Price;
            existingProduct.Description = dto.Description;
            existingProduct.ImageUrl = dto.ImageUrl;
            existingProduct.Discount = dto.Discount;
            existingProduct.BrandId = dto.BrandId;
            existingProduct.ProductTypeId = dto.ProductTypeId;

            // Удаляем старые варианты и добавляем обновленные
            _context.ProductVariants.RemoveRange(existingProduct.ProductVariants);
            foreach (var vDto in dto.ProductVariants)
            {
                _context.ProductVariants.Add(new ProductVariant { ProductId = id, Size = vDto.Size, StockQuantity = vDto.StockQuantity });
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductVariants)
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return NotFound();

            _context.ProductVariants.RemoveRange(product.ProductVariants);
            _context.Reviews.RemoveRange(product.Reviews);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}