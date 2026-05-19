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
}