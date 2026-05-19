using BroShopAPI.Data;
using BroShopAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BroShopAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context) => _context = context;

        // GET: api/Orders/user/5
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Order>>> GetUserOrders(int userId)
        {
            // Получаем заказы пользователя, сортируем от новых к старым
            return await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        // POST: api/Orders
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            // 1. Получаем корзину, но только те элементы, чьи ProductVariantId есть в SelectedVariantIds
            var cartItems = await _context.Carts
                .Include(c => c.ProductVariant)
                    .ThenInclude(pv => pv.Product)
                .Where(c => c.UserId == dto.UserId && dto.SelectedVariantIds.Contains(c.ProductVariantId))
                .ToListAsync();

            if (!cartItems.Any())
                return BadRequest("Выбранные товары не найдены в корзине");

            // 2. Считаем сумму только для выбранных
            decimal totalAmount = cartItems.Sum(item => item.Quantity * item.ProductVariant.Product.Price);
            decimal deliveryCost = 500;

            // 3. Создаем заказ
            var newOrder = new Order
            {
                UserId = dto.UserId,
                Address = dto.Address,
                OrderDate = DateTime.Now,
                Amount = totalAmount,
                DeliveryCost = deliveryCost,
                Status = "В обработке"
            };

            _context.Orders.Add(newOrder);
            await _context.SaveChangesAsync();

            // 4. Переносим выбранные товары в OrderProducts
            var orderProducts = cartItems.Select(item => new OrderProduct
            {
                OrderId = newOrder.OrderId,
                ProductVariantId = item.ProductVariantId,
                Quantity = item.Quantity,
                PriceAtPurchase = item.ProductVariant.Product.Price
            }).ToList();

            _context.OrderProducts.AddRange(orderProducts);

            // 5. Очищаем из корзины ТОЛЬКО ТЕ товары, которые купили
            _context.Carts.RemoveRange(cartItems);

            await _context.SaveChangesAsync();

            return Ok(newOrder);
        }

        // GET: api/Orders/details/5
        [HttpGet("details/{orderId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetOrderDetails(int orderId)
        {
            var details = await _context.OrderProducts
                .Include(op => op.ProductVariant)
                    .ThenInclude(pv => pv.Product)
                .Where(op => op.OrderId == orderId)
                .Select(op => new
                {
                    ProductName = op.ProductVariant.Product.Name,
                    ImageUrl = op.ProductVariant.Product.ImageUrl,
                    Size = op.ProductVariant.Size,
                    Quantity = op.Quantity,
                    Price = op.PriceAtPurchase // Используем цену, которая была на момент заказа
                })
                .ToListAsync();

            return Ok(details);
        }

        // GET: api/Orders/all
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<object>>> GetAllOrders()
        {
            // Получаем все заказы и джоиним с Users, чтобы вытащить имя заказчика
            var orders = await (from o in _context.Orders
                                join u in _context.Users on o.UserId equals u.UserId
                                orderby o.OrderDate descending
                                select new
                                {
                                    o.OrderId,
                                    o.UserId,
                                    UserName = string.IsNullOrEmpty(u.FullName) ? u.Login : u.FullName,
                                    o.Address,
                                    o.OrderDate,
                                    o.Amount,
                                    o.DeliveryCost,
                                    o.Status
                                }).ToListAsync();

            return Ok(orders);
        }


        // PUT: api/Orders/status/5
        [HttpPut("status/{orderId}")]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] UpdateStatusDto dto)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                return NotFound("Заказ не найден");

            order.Status = dto.Status;
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}