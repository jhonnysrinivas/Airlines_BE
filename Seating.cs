namespace AirLines_BE.Models
{
    public class Seating
    {
        public int seatingId { get; set; }
        public string? seatingType { get; set; }
        public int seatingCapacity { get; set; }
        public int seatCost {  get; set; }
        public List<Row>? seats { get; set; }

    }
}
