using AirLines_BE.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace AirLines_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly string conString;
        public BookingController(IConfiguration config)
        {
            conString = config["ConnectionStrings:SqlServerDb"];
        }

        [HttpGet("{seatId}")]
        public IActionResult getBookingDetails(int seatId)
        {
            Booking booking = new Booking();
            try
            {
                using (var connect = new SqlConnection(conString))
                {
                    connect.Open();
                    string sql = "select s.seat_no,s.seat_id,ss.seat_cost,(seat_cost/100) * 10 as gst,f.flight_name,f.arrival_time," +
                        "f.arrival_date,f.depature_time,f.depature_date,f.from_location,f.from_airport,f.to_location,f.to_airport, " +
                        "a.airlines_name, a.airlines_logo,ss.seat_Cost + (seat_cost/100) * 10 as total,ss.seating_type,s.seat_status, b.booking_code,b.booking_date,b.payment_type,b.payment_from from seats s  inner join seating ss on ss.seating_id = s.seating_id " +
                        "inner join flights f on f.economic_seating = s.seating_id or f.business_seating = s.seating_id inner join " +
                        "airlines a on a.airlines_id = f.airlines left join bookings b on b.seat_id = s.seat_id  where s.seat_id = @seatId";
                    using (var command = new SqlCommand(sql, connect))
                    {
                        command.Parameters.AddWithValue("@seatId", seatId);
                        using (var reader = command.ExecuteReader())
                        {
                            reader.Read();
                            booking.seatNo = reader.GetString(0);
                            booking.seatId = reader.GetInt32(1);
                            booking.seatCost = reader.GetInt32(2);
                            booking.gst = reader.GetInt32(3);
                            booking.flightName = reader.GetString(4);
                            booking.arrivalTime = reader.GetString(5);
                            booking.arrivalDate = reader.GetString(6);
                            booking.depatureTime = reader.GetString(7);
                            booking.depatureDate = reader.GetString(8);
                            booking.fromLocation = reader.GetString(9);
                            booking.fromAirport = reader.GetString(10);
                            booking.toLocation = reader.GetString(11);
                            booking.toAirport = reader.GetString(12);
                            booking.airlines = reader.GetString(13);
                            booking.airlinesLogo = reader.GetString(14);
                            booking.total = reader.GetInt32(15);
                            booking.seatingType = reader.GetString(16);
                            if (reader.GetString(17) == "Booked")
                            {
                                booking.bookingId = (reader.GetString(18) != null) ? reader.GetString(18) : "";
                                booking.bookingDate = (reader.GetDateTime(19) != null) ? reader.GetDateTime(19).ToString() : "";
                                booking.PaymentType = reader.GetString(20);
                                booking.PaymentFrom = reader.GetString(21);
                            }
                            reader.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("User", ex.GetBaseException().ToString());
                return BadRequest(ModelState);
            }
            return Ok(booking);
        }

        [HttpPost("{seatId}")]
        public IActionResult confirmBooking(int seatId, string bookingId, int userId, string? paymentType, string? paymentFrom)
        {
            int flightId = 0;
            Booking booking = new Booking();
            try
            {
                using (var connect = new SqlConnection(conString))
                {
                    connect.Open();
                    string sql = "select s.seat_status,flight_id from seats s inner join seating stg on stg.seating_id = s.seating_id inner join flights f on (f.economic_seating = stg.seating_id or f.business_seating = stg.seating_id) where seat_id = @seatId";
                    using (var command = new SqlCommand(sql, connect))
                    {
                        command.Parameters.AddWithValue("@seatId", seatId);
                        var reader = command.ExecuteReader();
                        reader.Read();
                        if (reader.GetString(0) == "Booked")
                        {
                            return BadRequest();
                        }
                        flightId = reader.GetInt32(1);
                        reader.Close();
                        
                    }
                    sql = "update seats set seat_status='Booked',booking_date = current_timestamp,booked_by=@userId where seat_id = @seatId";
                    using (var command = new SqlCommand(sql, connect))
                    {
                        command.Parameters.AddWithValue("@seatId", seatId);
                        command.Parameters.AddWithValue("@userId", userId);
                        command.ExecuteNonQuery();
                    }

                    sql = "insert into bookings(booking_date,booked_by,seat_id,booking_code,gst,cost,payment_type,payment_from)" +
                        "values(current_timestamp,@userId,@flightId,@bookingCode,0,0,@paymentType,@paymentFrom)";
                    using (var command = new SqlCommand(sql, connect))
                    {
                        command.Parameters.AddWithValue("@userId", userId);
                        command.Parameters.AddWithValue("@flightId", seatId);
                        command.Parameters.AddWithValue("@bookingCode", bookingId);
                        command.Parameters.AddWithValue("@paymentType", paymentType);
                        command.Parameters.AddWithValue("@paymentFrom", paymentFrom);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("User", ex.GetBaseException().ToString());
                return BadRequest(ModelState);
            }
            return Ok(booking);
        }

        [HttpGet("{userId}/all")]
        public IActionResult listBooking(int userId)
        {
            List<Booking> bookings = new List<Booking>();
            try
            {
                using (var connect = new SqlConnection(conString))
                {
                    connect.Open();
                    string sql = "select s.seat_no,s.seat_id,ss.seat_cost,(seat_cost/100) * 10 as gst,f.flight_name,f.arrival_time," +
                        "f.arrival_date,f.depature_time,f.depature_date,f.from_location,f.from_airport,f.to_location,f.to_airport, " +
                        "a.airlines_name, a.airlines_logo,ss.seat_Cost + (seat_cost/100) * 10 as total,b.booking_code  from seats s  inner join seating ss on ss.seating_id = s.seating_id " +
                        "inner join flights f on f.economic_seating = s.seating_id or f.business_seating = s.seating_id inner join " +
                        "airlines a on a.airlines_id = f.airlines inner join bookings b  on b.seat_id = s.seat_id where s.booked_by = @userId order by s.booking_date desc";
                    using (var command = new SqlCommand(sql, connect))
                    {
                        command.Parameters.AddWithValue("@userId", userId);
                        var reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            Booking booking = new Booking();
                            booking.seatNo = reader.GetString(0);
                            booking.seatId = reader.GetInt32(1);
                            booking.seatCost = reader.GetInt32(2);
                            booking.gst = reader.GetInt32(3);
                            booking.flightName = reader.GetString(4);
                            booking.arrivalTime = reader.GetString(5);
                            booking.arrivalDate = reader.GetString(6);
                            booking.depatureTime = reader.GetString(7);
                            booking.depatureDate = reader.GetString(8);
                            booking.fromLocation = reader.GetString(9);
                            booking.fromAirport = reader.GetString(10);
                            booking.toLocation = reader.GetString(11);
                            booking.toAirport = reader.GetString(12);
                            booking.airlines = reader.GetString(13);
                            booking.airlinesLogo = reader.GetString(14);
                            booking.total = reader.GetInt32(15);
                            booking.bookingId = reader.GetString(16);
                            bookings.Add(booking);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("User", ex.GetBaseException().ToString());
                return BadRequest(ModelState);
            }
            return Ok(bookings);
        }
    }
}
