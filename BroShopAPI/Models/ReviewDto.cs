namespace BroShopAPI.Models
{
    public class ReviewDto
    {
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public string Text { get; set; } = null!;
        public int Rating { get; set; }
        public string? UserLogin { get; set; }
    }
}