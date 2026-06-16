using BroShopAPI.Data;
using BroShopAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
            var end = endDate ?? DateTime.UtcNow;

            // Базовый запрос
            var ordersQuery = _context.Orders
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                            .ThenInclude(p => p.Brand)
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                            .ThenInclude(p => p.ProductType)
                .Where(o => o.OrderDate >= start && o.OrderDate <= end && o.Status != "Отменен");

            var totalOrders = await ordersQuery.CountAsync();
            var totalRevenue = await ordersQuery.SumAsync(o => (decimal?)o.Amount) ?? 0;
            var averageCheck = totalOrders > 0 ? totalRevenue / totalOrders : 0;
            var pendingOrders = await _context.Orders.CountAsync(o => o.Status == "В обработке");

            var ordersList = await ordersQuery.ToListAsync();

            var chartData = ordersList
                .GroupBy(o => o.OrderDate.Date)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    Date = g.Key.ToString("dd.MM.yyyy"),
                    Revenue = g.Sum(o => o.Amount),
                    OrdersCount = g.Count()
                })
                .ToList();

            // БЕЗОПАСНЫЙ ТОП-5 (Защита от NULL в БД)
            var topProducts = ordersList
                .SelectMany(o => o.OrderProducts ?? Enumerable.Empty<OrderProduct>())
                .Where(op => op.ProductVariant != null && op.ProductVariant.Product != null)
                .GroupBy(op => op.ProductVariant.Product.Name)
                .Select(g => new
                {
                    Name = g.Key,
                    QuantitySold = g.Sum(op => op.Quantity),
                    Revenue = g.Sum(op => (op.PriceAtPurchase ?? 0) * op.Quantity)
                })
                .OrderByDescending(x => x.QuantitySold)
                .Take(5)
                .ToList();

            // БЕЗОПАСНЫЕ КАТЕГОРИИ
            var categoryStats = ordersList
                .SelectMany(o => o.OrderProducts ?? Enumerable.Empty<OrderProduct>())
                .Where(op => op.ProductVariant?.Product?.ProductType != null)
                .GroupBy(op => op.ProductVariant.Product.ProductType.Name)
                .Select(g => new
                {
                    Name = g.Key,
                    Value = g.Sum(op => op.Quantity)
                })
                .ToList();

            // БЕЗОПАСНЫЕ БРЕНДЫ
            var brandStats = ordersList
                .SelectMany(o => o.OrderProducts ?? Enumerable.Empty<OrderProduct>())
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