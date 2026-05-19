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
}