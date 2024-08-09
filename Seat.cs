namespace AirLines_BE.Models
{
    public class Seat
    {
        public int seatId {  get; set; }
        public string? seatNo { get; set; }
        public string? seatStatus { get; set; }
        public int seatBookedBy { get; set; }
        public DateTime? seatBookingDate { get; set; }
        public int seatingId { get; set; }
    }
}
