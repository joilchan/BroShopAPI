using BroShopAPI.Data;
using BroShopAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace BroShopAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ReviewsController(AppDbContext context) => _context = context;

        [HttpPost]
        public async Task<IActionResult> AddReview([FromBody] ReviewDto dto)
        {
            if (dto == null) return BadRequest("Данные отсутствуют");

            try
            {
                var newReview = new Review
                {
                    ProductId = dto.ProductId,
                    UserId = dto.UserId,
                    Text = dto.Text,
                    Rating = dto.Rating,
                    // Навигационные свойства НЕ заполняем, EF сам подтянет связи по ID
                };

                _context.Reviews.Add(newReview);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                // Это поможет увидеть ошибку в логах сервера
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.InnerException?.Message ?? ex.Message}");
            }
        }


    }
}