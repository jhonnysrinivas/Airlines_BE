namespace AirLines_BE.Models
{
    public class User
    {
        public int? userId { get; set; }
        public string? userName { get; set; }
        public string? email { get; set; } 
        public string? password { get; set; }
        public string? accountType { get; set; }
        public string? cardNumber { get; set; }
        public string? cardName { get; set; }
        public string? expiryDate { get; set; }
        public string? cvvCode { get; set; }
        public string? icon {  get; set; }

    }
}
