namespace BroShopAPI.Models
{
    public class ProductDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public int? Discount { get; set; }

        public BrandShortDto? Brand { get; set; }
        public ProductTypeShortDto? ProductType { get; set; }

        public List<ProductVariantDto> ProductVariants { get; set; } = new();
    }
}
