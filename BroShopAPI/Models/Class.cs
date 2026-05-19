namespace BroShopAPI.Models
{
    public class CartDTO
    {
        public int UserId { get; set; }
        public int ProductVariantId { get; set; }
        public int Quantity { get; set; }
    }
}
