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
            var products = await _context.Products
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
                    Brand = p.Brand == null
                        ? null
                        : new BrandShortDto
                        {
                            BrandId = p.Brand.BrandId,
                            Name = p.Brand.Name
                        },

                    ProductTypeId = p.ProductTypeId,
                    ProductType = p.ProductType == null
                        ? null
                        : new ProductTypeShortDto
                        {
                            ProductTypeId = p.ProductType.ProductTypeId,
                            Name = p.ProductType.Name
                        },

                    ProductVariants = p.ProductVariants
                        .Select(v => new ProductVariantDto
                        {
                            ProductVariantId = v.ProductVariantId,
                            Size = v.Size,
                            StockQuantity = v.StockQuantity
                        })
                        .ToList()
                })
                .ToListAsync();

            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            var product = await _context.Products
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
                    Brand = p.Brand == null
                        ? null
                        : new BrandShortDto
                        {
                            BrandId = p.Brand.BrandId,
                            Name = p.Brand.Name
                        },

                    ProductTypeId = p.ProductTypeId,
                    ProductType = p.ProductType == null
                        ? null
                        : new ProductTypeShortDto
                        {
                            ProductTypeId = p.ProductType.ProductTypeId,
                            Name = p.ProductType.Name
                        },

                    ProductVariants = p.ProductVariants
                        .Select(v => new ProductVariantDto
                        {
                            ProductVariantId = v.ProductVariantId,
                            Size = v.Size,
                            StockQuantity = v.StockQuantity
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (product == null)
                return NotFound();

            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult> PostProduct(ProductCreateUpdateDto dto)
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

            if (dto.ProductVariants != null)
            {
                foreach (var variant in dto.ProductVariants)
                {
                    _context.ProductVariants.Add(new ProductVariant
                    {
                        ProductId = product.ProductId,
                        Size = variant.Size,
                        StockQuantity = variant.StockQuantity
                    });
                }

                await _context.SaveChangesAsync();
            }

            return CreatedAtAction(
                nameof(GetProduct),
                new { id = product.ProductId },
                dto
            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, ProductCreateUpdateDto dto)
        {
            if (id != dto.ProductId)
                return BadRequest("ID mismatch");

            var product = await _context.Products
                .Include(p => p.ProductVariants)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
                return NotFound();

            product.Name = dto.Name;
            product.Price = dto.Price;
            product.Description = dto.Description;
            product.ImageUrl = dto.ImageUrl;
            product.Discount = dto.Discount;
            product.BrandId = dto.BrandId;
            product.ProductTypeId = dto.ProductTypeId;

            _context.ProductVariants.RemoveRange(product.ProductVariants);

            if (dto.ProductVariants != null)
            {
                foreach (var variant in dto.ProductVariants)
                {
                    _context.ProductVariants.Add(new ProductVariant
                    {
                        ProductId = id,
                        Size = variant.Size,
                        StockQuantity = variant.StockQuantity
                    });
                }
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

            if (product == null)
                return NotFound();

            _context.ProductVariants.RemoveRange(product.ProductVariants);
            _context.Reviews.RemoveRange(product.Reviews);
            _context.Products.Remove(product);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
