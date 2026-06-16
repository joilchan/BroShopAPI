using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BroShopAPI.Data;
using System.Linq;

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

        [HttpGet("detailed")]
        public async Task<ActionResult<object>> GetDetailedStats(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            // По умолчанию берем статистику за последний месяц, если даты не переданы
            var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
            var end = endDate ?? DateTime.UtcNow;

            // Базовый запрос: прокладываем путь через ProductVariant
            var ordersQuery = _context.Orders
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.ProductVariant) // Сначала идем к варианту (размеру)
                        .ThenInclude(pv => pv.Product)    // От варианта идем к самому товару
                            .ThenInclude(p => p.Brand)    // От товара к бренду
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                            .ThenInclude(p => p.ProductType) // От товара к категории
                .Where(o => o.OrderDate >= start && o.OrderDate <= end && o.Status != "Отменен");

            // 1. Общие показатели
            var totalOrders = await ordersQuery.CountAsync();
            var totalRevenue = await ordersQuery.SumAsync(o => (decimal?)o.Amount) ?? 0;
            var averageCheck = totalOrders > 0 ? totalRevenue / totalOrders : 0;
            var pendingOrders = await _context.Orders.CountAsync(o => o.Status == "В обработке");

            // 2. Данные для графика выручки по дням
            var ordersList = await ordersQuery.ToListAsync();
            var chartData = ordersList
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new
                {
                    Date = g.Key.ToString("dd.MM.yyyy"),
                    Revenue = g.Sum(o => o.Amount),
                    OrdersCount = g.Count()
                })
                .OrderBy(d => DateTime.Parse(d.Date))
                .ToList();

            // 3. Топ продаваемых товаров
            var topProducts = ordersList
                .SelectMany(o => o.OrderProducts)
                .Where(op => op.ProductVariant?.Product != null) // Защита от нулевых ссылок
                .GroupBy(op => op.ProductVariant.Product.Name)   // Группируем по имени товара
                .Select(g => new
                {
                    Name = g.Key,
                    QuantitySold = g.Sum(op => op.Quantity),
                    // Учитываем, что PriceAtPurchase может быть null (заменяем на 0)
                    Revenue = g.Sum(op => (op.PriceAtPurchase ?? 0) * op.Quantity)
                })
                .OrderByDescending(x => x.QuantitySold)
                .Take(5)
                .ToList();

            // 4. Распределение по категориям
            var categoryStats = ordersList
                .SelectMany(o => o.OrderProducts)
                .Where(op => op.ProductVariant?.Product?.ProductType != null)
                .GroupBy(op => op.ProductVariant.Product.ProductType.Name)
                .Select(g => new
                {
                    Name = g.Key,
                    Value = g.Sum(op => op.Quantity)
                })
                .ToList();

            // 5. Распределение по брендам
            var brandStats = ordersList
                .SelectMany(o => o.OrderProducts)
                .Where(op => op.ProductVariant?.Product?.Brand != null)
                .GroupBy(op => op.ProductVariant.Product.Brand.Name)
                .Select(g => new
                {
                    Name = g.Key,
                    Value = g.Sum(op => op.Quantity)
                })
                .ToList();

            return Ok(new
            {
                Period = new { Start = start.ToString("dd.MM.yyyy"), End = end.ToString("dd.MM.yyyy") },
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue,
                AverageCheck = averageCheck,
                PendingOrders = pendingOrders,
                ChartData = chartData,
                TopProducts = topProducts,
                CategoryStats = categoryStats,
                BrandStats = brandStats
            });
        }
    }
}