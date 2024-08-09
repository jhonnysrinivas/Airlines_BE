namespace AirLines_BE.Models
{
    public class Booking
    {
        public string? bookingId {  get; set; }
        public string? flightName {  get; set; }
        public string? arrivalTime { get; set; }
        public string? arrivalDate { get; set; }
        public string? depatureTime { get; set; }
        public string? depatureDate { get; set; }
        public string? fromLocation { get; set; }
        public string? fromAirport { get; set; }
        public string? toLocation { get; set; }
        public string? toAirport { get; set; }
        public string? airlines { get; set; }
        public string? airlinesLogo { get; set; }
        public string? seatNo { get; set; }
        public int seatId { get; set; }
        public int seatCost { get; set; }
        public int total { get; set; }
        public int gst {  get; set; }
        public string? seatingType { get; set; }
        public string? bookingDate { get; set; }
        public string? PaymentType { get; set; }
        public string? PaymentFrom { get; set; }


    }
}
