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

        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
        {
            return await _context.Products
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
                    ProductTypeName = p.ProductType != null ? p.ProductType.Name : null
                })
                .ToListAsync();
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            var productDto = await _context.Products
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
                    ProductTypeName = p.ProductType != null ? p.ProductType.Name : null
                })
                .FirstOrDefaultAsync();

            if (productDto == null)
            {
                return NotFound();
            }

            return productDto;
        }

        // POST: api/Products
        [HttpPost]
        public async Task<ActionResult<ProductDto>> PostProduct(ProductDto dto)
        {
            // Маппим DTO обратно в доменную модель сущности базы данных
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

            // Присваиваем сгенерированный базой ID обратно в DTO
            dto.ProductId = product.ProductId;

            // Если нужно вернуть актуальные имена Бренда/Типа после сохранения:
            var info = await _context.Products
                .Where(p => p.ProductId == product.ProductId)
                .Select(p => new {
                    BrandName = p.Brand != null ? p.Brand.Name : null,
                    TypeName = p.ProductType != null ? p.ProductType.Name : null
                })
                .FirstOrDefaultAsync();

            if (info != null)
            {
                dto.BrandName = info.BrandName;
                dto.ProductTypeName = info.TypeName;
            }

            return CreatedAtAction(nameof(GetProduct), new { id = dto.ProductId }, dto);
        }

        // PUT: api/Products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, ProductDto dto)
        {
            if (id != dto.ProductId) return BadRequest("ID mismatch");

            var existingProduct = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (existingProduct == null) return NotFound();

            // Обновляем свойства существующего продукта из пришедшего DTO
            existingProduct.Name = dto.Name;
            existingProduct.Price = dto.Price;
            existingProduct.Description = dto.Description;
            existingProduct.ImageUrl = dto.ImageUrl;
            existingProduct.Discount = dto.Discount;
            existingProduct.BrandId = dto.BrandId;
            existingProduct.ProductTypeId = dto.ProductTypeId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return NoContent();
        }

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductVariants)
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return NotFound();

            // Удаляем связанные сущности во избежание конфликтов каскадного удаления
            _context.ProductVariants.RemoveRange(product.ProductVariants);
            _context.Reviews.RemoveRange(product.Reviews);

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}