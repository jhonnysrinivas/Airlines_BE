namespace AirLines_BE.Models
{
    public class AddFlight
    {
        public int? flightId { get; set; }
        public string? flightName { get; set; }
        public int? airLines { get; set; }
        public string? arrivalTime { get; set; }
        public string? arrivalDate { get; set; }
        public string? depatureTime { get; set; }
        public string? depatureDate { get; set; }
        public string? fromLocation { get; set; }
        public string? fromAirport { get; set; }
        public string? toLocation { get; set; }
        public string? toAirport { get; set; }
        public int ecoSeatingCapacity { get; set; }
        public int ecoSeatCost { get; set; }
        public int busSeatingCapacity { get; set; }
        public int busSeatCost { get; set; }

    }
}
