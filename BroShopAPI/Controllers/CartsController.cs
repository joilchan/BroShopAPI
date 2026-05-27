using BroShopAPI.Data;
using BroShopAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BroShopAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public CartsController(AppDbContext context) => _context = context;

        // Получить корзину пользователя
        [HttpGet("{userId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetCart(int userId)
        {
            return await _context.Carts
                .Where(c => c.UserId == userId)
                .Select(c => new
                {
                    c.ProductVariantId,
                    c.Quantity,
                    Size = c.ProductVariant != null ? c.ProductVariant.Size : "Универсальный",
                    ProductName = c.ProductVariant != null ? c.ProductVariant.Product.Name : "Товар",
                    Price = c.ProductVariant.Product.Price,
                    Image = c.ProductVariant.Product.ImageUrl,
                    // Добавляем остаток со склада (из таблицы вариантов)
                    Stock = c.ProductVariant != null ? c.ProductVariant.StockQuantity : 0
                }).ToListAsync();
        }

        // Добавить или обновить товар в корзине
        [HttpPost]
        public async Task<IActionResult> AddToCart(CartDTO dto)
        {
            // Находим вариант товара, чтобы узнать остаток
            var variant = await _context.ProductVariants.FindAsync(dto.ProductVariantId);
            if (variant == null) return NotFound("Вариант товара не найден");

            var existing = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == dto.UserId && c.ProductVariantId == dto.ProductVariantId);

            if (existing != null)
            {
                // Проверяем, не превысит ли суммарное количество остаток на складе
                if (existing.Quantity + dto.Quantity > variant.StockQuantity)
                {
                    return BadRequest($"Нельзя добавить больше {variant.StockQuantity} шт. данного товара.");
                }
                existing.Quantity += dto.Quantity;
            }
            else
            {
                // Проверяем стартовое количество
                if (dto.Quantity > variant.StockQuantity)
                {
                    return BadRequest($"Доступно для заказа только {variant.StockQuantity} шт.");
                }

                var newCartEntry = new Cart
                {
                    UserId = dto.UserId,
                    ProductVariantId = dto.ProductVariantId,
                    Quantity = dto.Quantity
                };
                _context.Carts.Add(newCartEntry);
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("update-quantity")]
        public async Task<IActionResult> UpdateQuantity([FromBody] CartDTO dto)
        {
            var item = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == dto.UserId && c.ProductVariantId == dto.ProductVariantId);

            if (item == null) return NotFound();

            // Проверяем лимит при ручном изменении (+/-)
            var variant = await _context.ProductVariants.FindAsync(dto.ProductVariantId);
            if (variant != null && dto.Quantity > variant.StockQuantity)
            {
                return BadRequest($"На складе осталось только {variant.StockQuantity} шт.");
            }

            item.Quantity = dto.Quantity;

            if (item.Quantity <= 0)
                _context.Carts.Remove(item);

            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
