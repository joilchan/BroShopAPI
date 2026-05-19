namespace BroShopAPI.Models
{
    public class CreateOrderDto
    {
        public int UserId { get; set; }
        public string Address { get; set; }
        public string? PromoCode { get; set; }

        public List<int> SelectedVariantIds { get; set; }
    }
}