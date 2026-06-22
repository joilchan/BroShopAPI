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
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            return await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.ProductType)
                .Include(p => p.ProductVariants)
                .Include(p => p.Reviews)
                .ThenInclude(r => r.User)
                .ToListAsync();
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.ProductType)
                .Include(p => p.ProductVariants)
                .Include(p => p.Reviews)
                .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        // POST: api/Products
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            product.Brand = null;
            product.ProductType = null;

            if (product.ProductVariants != null)
            {
                foreach (var variant in product.ProductVariants)
                {
                    variant.Product = null;
                }
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
 
            var createdProduct = await _context.Products
                .Include(p => p.ProductVariants)
                .Include(p => p.Brand)
                .Include(p => p.ProductType)
                .FirstOrDefaultAsync(p => p.ProductId == product.ProductId);

            return CreatedAtAction(nameof(GetProduct), new { id = product.ProductId }, createdProduct);
        }

        // PUT: api/Products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, Product product)
        {
            if (id != product.ProductId) return BadRequest("ID mismatch");

            var existingProduct = await _context.Products
                .Include(p => p.ProductVariants)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (existingProduct == null) return NotFound();

            _context.Entry(existingProduct).CurrentValues.SetValues(product);

            existingProduct.Brand = null;
            existingProduct.ProductType = null;

            _context.ProductVariants.RemoveRange(existingProduct.ProductVariants);

            if (product.ProductVariants != null)
            {
                foreach (var variant in product.ProductVariants)
                {
                    variant.ProductId = id;
                    variant.Product = null;
                    _context.ProductVariants.Add(variant);
                }
            }

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

            _context.ProductVariants.RemoveRange(product.ProductVariants);
            _context.Reviews.RemoveRange(product.Reviews);

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }
}