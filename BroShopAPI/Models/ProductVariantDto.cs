namespace BroShopAPI.Models
{
    public class ProductVariantDto
    {
        public int ProductVariantId { get; set; }
        public string Size { get; set; } = null!;
        public int StockQuantity { get; set; }
    }
}
