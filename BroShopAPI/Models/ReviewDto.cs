namespace BroShopAPI.Models
{
    public class ReviewDto
    {
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public string Text { get; set; } = null!;
        public decimal Rating { get; set; }
    }
}