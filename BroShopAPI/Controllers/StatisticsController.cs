using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BroShopAPI.Data;

namespace BroShopAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StatisticsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<object>> Get()
        {
            // Считаем все заказы
            var totalOrders = await _context.Orders.CountAsync();

            // Считаем выручку (только для доставленных/завершенных заказов)
            // Поменяйте "Доставлен" на тот статус, который вы считаете успешным
            var totalRevenue = await _context.Orders
                .Where(o => o.Status == "Доставлен")
                .SumAsync(o => (decimal?)o.Amount) ?? 0;

            // Считаем количество обычных пользователей (не админов, предполагаем RoleId админа = 1)
            var totalUsers = await _context.Users.CountAsync(u => u.RoleId != 1);

            // Заказы, требующие внимания
            var pendingOrders = await _context.Orders.CountAsync(o => o.Status == "В обработке");

            return Ok(new
            {
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue,
                TotalUsers = totalUsers,
                PendingOrders = pendingOrders
            });
        }
    }
}