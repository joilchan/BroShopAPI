namespace BroShopAPI.Models
{
    public class ProductDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }

        public string? BrandName { get; set; }
        public string? ProductTypeName { get; set; }
        public List<ProductVariantDto> ProductVariants { get; set; } = new();
        public List<ReviewDto> Reviews { get; set; } = new();
    }
}
