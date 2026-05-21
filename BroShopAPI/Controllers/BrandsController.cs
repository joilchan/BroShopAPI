using BroShopAPI.Data;
using BroShopAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class BrandsController : ControllerBase
{
    private readonly AppDbContext _context;

    public BrandsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Brand>>> GetBrands()
    {
        return await _context.Brands.ToListAsync();
    }

    // POST: api/Brands
    [HttpPost]
    public async Task<ActionResult<Brand>> CreateBrand([FromBody] CreateBrandRequest request)
    {
        // Валидация входящего DTO
        if (request == null || string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "Название бренда не может быть пустым." });
        }

        // Защита от дубликатов по имени
        var exists = await _context.Brands.AnyAsync(b => b.Name == request.Name);
        if (exists)
        {
            return Conflict(new { message = "Такой бренд уже существует." });
        }

        // Маппим DTO в реальную модель базы данных
        var newBrand = new Brand
        {
            Name = request.Name
            // Список Products остаётся пустым по умолчанию
        };

        _context.Brands.Add(newBrand);
        await _context.SaveChangesAsync();

        // Возвращаем статус 201 Created. 
        // Обрати внимание: EF Core автоматически заполнит newBrand.BrandId после сохранения
        return CreatedAtAction(nameof(GetBrands), new { id = newBrand.BrandId }, newBrand);
    }

    // DELETE: api/Brands/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBrand(int id)
    {
        // Ищем бренд в базе данных вместе с его товарами
        var brand = await _context.Brands
            .Include(b => b.Products)
            .FirstOrDefaultAsync(b => b.BrandId == id);

        // Если бренд не найден — 404
        if (brand == null)
        {
            return NotFound(new { message = "Бренд не найден." });
        }

        // Защита: проверяем, привязаны ли к бренду товары
        if (brand.Products != null && brand.Products.Any())
        {
            return BadRequest(new
            {
                message = $"Нельзя удалить бренд, так как к нему привязано товаров: {brand.Products.Count}. Сначала удалите или перенесите товары."
            });
        }

        // Удаляем бренд из контекста и сохраняем изменения
        _context.Brands.Remove(brand);
        await _context.SaveChangesAsync();

        // Возвращаем статус 204 No Content
        return NoContent();
    }

    public class BrandDto
    {
        public int BrandId { get; set; }
        public string Name { get; set; }
    }

    // Используется исключительно для POST-запроса на создание нового бренда
    public class CreateBrandRequest
    {
        public string Name { get; set; }
    }
}