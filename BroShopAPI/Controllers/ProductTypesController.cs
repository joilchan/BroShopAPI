using BroShopAPI.Data;
using BroShopAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class ProductTypesController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductTypesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductType>>> GetProductTypes()
    {
        // Возвращаем все типы (напр. Кроссовки, Худи, Футболки)
        return await _context.ProductTypes.ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<ProductType>> CreateProductType([FromBody] CreateProductTypeRequest request)
    {
        // Проверяем входящий DTO
        if (request == null || string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "Название категории не может быть пустым." });
        }

        // Проверка на дубликаты по имени
        var exists = await _context.ProductTypes.AnyAsync(c => c.Name == request.Name);
        if (exists)
        {
            return Conflict(new { message = "Такая категория уже существует." });
        }

        // Маппим DTO в реальную модель сущности базы данных
        var newCategory = new ProductType
        {
            Name = request.Name
            // Свойство Products не трогаем, оно по умолчанию будет пустым списком
        };

        _context.ProductTypes.Add(newCategory);
        await _context.SaveChangesAsync();

        // Возвращаем статус 201 Created. 
        // В анонимном объекте используем newCategory.ProductTypeId, так как EF Core заполнит его после SaveChangesAsync
        return CreatedAtAction(nameof(GetProductTypes), new { id = newCategory.ProductTypeId }, newCategory);
    }

    // DELETE: api/ProductTypes/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProductType(int id)
    {
        // Ищем категорию в базе данных
        var productType = await _context.ProductTypes
            .Include(pt => pt.Products) // Подтягиваем связанные товары для проверки
            .FirstOrDefaultAsync(pt => pt.ProductTypeId == id);

        // Если категория не найдена — возвращаем 404
        if (productType == null)
        {
            return NotFound(new { message = "Категория не найдена." });
        }

        // Защита: проверяем, есть ли товары в этой категории
        if (productType.Products != null && productType.Products.Any())
        {
            return BadRequest(new
            {
                message = $"Нельзя удалить категорию, так как к ней привязано товаров: {productType.Products.Count}. Сначала удалите или перенесите товары."
            });
        }

        // Если товаров нет — спокойно удаляем
        _context.ProductTypes.Remove(productType);
        await _context.SaveChangesAsync();

        // Возвращаем статус 204 No Content (успешно удалено, возвращать нечего)
        return NoContent();
    }

    public class ProductTypeDto
    {
        public int ProductTypeId { get; set; }
        public string Name { get; set; }
    }

    public class CreateProductTypeRequest
    {
        public string Name { get; set; }
    }
}