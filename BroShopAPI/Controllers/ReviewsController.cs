using BroShopAPI.Data;
using BroShopAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BroShopAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReviewsController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/reviews
        [HttpPost]
        public async Task<IActionResult> PostReview([FromBody] ReviewDto dto)
        {
            if (dto == null) return BadRequest("Данные отзыва пусты");
            if (string.IsNullOrWhiteSpace(dto.Text)) return BadRequest("Текст отзыва пуст");
            if (dto.Rating < 1 || dto.Rating > 5) return BadRequest("Рейтинг должен быть от 1 до 5");

            try
            {
                // Проверяем существование пользователя и товара
                var user = await _context.Users.FindAsync(dto.UserId);
                if (user == null) return NotFound("Пользователь не найден");

                bool productExists = await _context.Products.AnyAsync(p => p.ProductId == dto.ProductId);
                if (!productExists) return NotFound("Товар не найден");

                // Создаем сущность для БД (кастуем int рейтинг в decimal, если в БД тип decimal)
                var review = new Review
                {
                    ProductId = dto.ProductId,
                    UserId = dto.UserId,
                    Text = dto.Text.Trim(),
                    Rating = (int)dto.Rating
                };

                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();

                // Возвращаем DTO с заполненным логином
                dto.UserLogin = user.Login;
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
            }
        }
    }
}