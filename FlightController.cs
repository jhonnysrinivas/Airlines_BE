using AirLines_BE.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Reflection.PortableExecutable;

namespace AirLines_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightController : ControllerBase
    {
        public string conString;
        public FlightController(IConfiguration config)
        {
            conString = config["ConnectionStrings:SqlServerDb"];
        }
        

        [HttpGet]
        public IActionResult listFlights(string? flightName,int? airlines, string? sorting)
        { 
            List<Flight> flights = new List<Flight>();
            try
            {
                using (var connect = new SqlConnection(conString))
                {
                    connect.Open();
                    flightName = (flightName == null || flightName == "") ? "" : " and f.flight_name like '%" + flightName + "%' ";
                    string airlinesFilter = (airlines == null || airlines == 0) ? "" : " and f.airlines = " + airlines + " ";
                    sorting = (sorting == null || sorting == "") ? " order by f.flight_name asc " : " order by " + sorting.Replace(";", " ");

                    string sql = "select f.flight_id,f.flight_name,f.airlines,f.arrival_time,f.depature_time,f.from_location,f.to_location," +
                        "al.airlines_name,al.airlines_logo,es.seat_cost,bs.seat_cost from flights f inner join airlines al on " +
                        "al.airlines_id = f.airlines inner join seating es on es.seating_id = economic_seating inner join seating bs " +
                        "on bs.seating_id = business_seating where 1=1 " + flightName + airlinesFilter + sorting;
                    using (var command = new SqlCommand(sql, connect))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Flight flight = new Flight();
                                flight.flightId = reader.GetInt32(0);
                                flight.flightName = reader.GetString(1);
                                flight.airLines = reader.GetInt32(2);
                                flight.arrivalTime = reader.GetString(3);
                                flight.depatureTime = reader.GetString(4);
                                flight.fromLocation = reader.GetString(5);
                                flight.toLocation = reader.GetString(6);
                                flight.airlinesName = reader.GetString(7);
                                flight.airlinesLogo = reader.GetString(8);
                                flight.economicCost = reader.GetInt32(9);
                                flight.businessCost = reader.GetInt32(10);
                                flights.Add(flight);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("User", ex.GetBaseException().ToString());
                return BadRequest(ModelState);
            } 
            return Ok(flights);
        }

        [HttpGet("{id}")]
        public IActionResult GetFlightDetails(int id)
        { 
            Flight flight = new Flight();
            try
            {
                using (var connect = new SqlConnection(conString))
                {
                    connect.Open();
                    string sql = "select f.flight_id,f.flight_name,f.airlines,al.airlines_name,al.airlines_logo,f.arrival_time,f.arrival_date,f.depature_time,f.depature_date,f.from_location,f.from_airport,f.to_location,f.to_airport,f.economic_seating,f.business_seating from flights f inner join airlines al on al.airlines_id = f.airlines where f.flight_id = " + id;
                    using (var command = new SqlCommand(sql, connect))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            reader.Read();
                            flight.flightId = reader.GetInt32(0);
                            flight.flightName = reader.GetString(1);
                            flight.airLines = reader.GetInt32(2);
                            flight.airlinesName = reader.GetString(3);
                            flight.airlinesLogo = reader.GetString(4);
                            flight.arrivalTime = reader.GetString(5);
                            flight.arrivalDate = reader.GetString(6);
                            flight.depatureTime = reader.GetString(7);
                            flight.depatureDate = reader.GetString(8);
                            flight.fromLocation = reader.GetString(9);
                            flight.fromAirport = reader.GetString(10);
                            flight.toLocation = reader.GetString(11);
                            flight.toAirport = reader.GetString(12);
                            flight.economicSeatings = this.getSeating(reader.GetInt32(13));
                            flight.businessSeatings = this.getSeating(reader.GetInt32(14));
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
            return Ok(flight);
        }
        /*

        [HttpPost]
        public IActionResult addFlight(Flight flight)
        { 
            int flightId = 0;
            try
            {
                using (var connect = new SqlConnection(conString))
                {
                    connect.Open();
                    int ecoSeatId = this.addSeating("Economic",flight.ecoSeatingCapacity,flight.ecoSeatCost);
                    int busSeatId = this.addSeating("Business", flight.busSeatingCapacity, flight.busSeatCost);
                    string sql = "INSERT INTO flights(flight_name,airlines,arrival_time,arrival_date,depature_time,depature_date,from_location,from_airport,to_location,to_airport,economic_seating,business_seating) VALUES(" +
                        "@flightName,@airlines,@arrivalTime,@arrivalDate,@depatureTime,@depatureDate,@fromLocation,@fromAirport,@toLocation,@toAirport,@ecoSeatId,@busSeatId)";

                    using (var command = new SqlCommand(sql, connect))
                    {
                        command.Parameters.AddWithValue("@flightName", flight.flightName);
                        command.Parameters.AddWithValue("@airlines", flight.airLines);
                        command.Parameters.AddWithValue("@arrivalTime", flight.arrivalTime);
                        command.Parameters.AddWithValue("@arrivalDate", flight.arrivalDate);
                        command.Parameters.AddWithValue("@depatureTime", flight.depatureTime);
                       command.Parameters.AddWithValue("@depatureDate", flight.depatureDate);
                       command.Parameters.AddWithValue("@fromLocation", flight.fromLocation);
                       command.Parameters.AddWithValue("@fromAirport", flight.fromAirport);
                       command.Parameters.AddWithValue("@toLocation", flight.toLocation);
                       command.Parameters.AddWithValue("@toAirport", flight.toAirport);
                       command.Parameters.AddWithValue("@ecoSeatId", ecoSeatId);
                       command.Parameters.AddWithValue("@busSeatId", busSeatId);
                       command.ExecuteNonQuery();
                    }

                    sql = "select max(flight_id) from flights";
                    using (var command = new SqlCommand( sql, connect)) {
                    
                        using (var reader = command.ExecuteReader())
                        {
                             reader.Read();
                            flightId = reader.GetInt32(0);
                        }
                     }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("User", ex.GetBaseException().ToString());
                return BadRequest(ModelState);
            }
            return Created();
        }
        */

        public int addSeating(string seatingType, int capacity, int seatCost)
        {
            int seatingId = 0;
            using (var connect = new SqlConnection(conString))
            {
                connect.Open();
                string sql = "INSERT INTO seating(seating_type,capacity,seat_cost) VALUES(@seatingType,@capacity, @seatCost)";
                using (var command = new SqlCommand(sql, connect))
                {
                    command.Parameters.AddWithValue("@seatingType", seatingType);
                    command.Parameters.AddWithValue("@capacity", capacity);
                    command.Parameters.AddWithValue("@seatCost", seatCost);
                    command.ExecuteNonQuery();
                }
                sql = "select max(seating_id) from seating";
                using (var command = new SqlCommand(sql, connect))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        reader.Read();
                        seatingId = reader.GetInt32(0);
                    }
                }
                sql = "INSERT INTO seats(seat_no,seat_status,seating_id) VALUES(@seatNo,@seatStatus,@seatingId)";
                for (int i = 1; i <= capacity; i++)
                {
                    addSeats(i, sql, connect, "A", seatingId);
                    addSeats(i, sql, connect, "B", seatingId);
                    addSeats(i, sql, connect, "C", seatingId);
                    addSeats(i, sql, connect, "D", seatingId);
                    addSeats(i, sql, connect, "E", seatingId);
                    addSeats(i, sql, connect, "F", seatingId);
                }
                connect.Close();
            }
            return seatingId;
        }
        public Seating getSeating(int seatingId)
        {
            Seating seating = new Seating();
            using (var connect = new SqlConnection(conString))
            {
                connect.Open();
                string sql = "select seating_id,seating_type,capacity,seat_cost from seating where seating_id = @seatingId";
                using (var command2 = new SqlCommand(sql, connect))
                {
                    command2.Parameters.AddWithValue("@seatingId", seatingId);
                    using (var reader2 = command2.ExecuteReader())
                    {
                        reader2.Read();
                        seating.seatingId = reader2.GetInt32(0);
                        seating.seatingType = reader2.GetString(1);
                        seating.seatingCapacity = reader2.GetInt32(2);
                        seating.seatCost = reader2.GetInt32(3);
                        sql = "select seat_id,seat_no,seat_status,booked_by,booking_date,seat_id from seats where seating_id = " + reader2.GetInt32(0);
                        reader2.Close();
                        using (var command3 = new SqlCommand(sql, connect))
                        {
                            using (var reader3 = command3.ExecuteReader())
                            {
                                List<Row> rows = new List<Row>();

                                for(int row=1;row<=seating.seatingCapacity; row++)
                                {
                                    Row rowData = new Row();
                                    rowData.seatsA = getSeat(reader3);
                                    rowData.seatsB = getSeat(reader3);
                                    rowData.seatsC = getSeat(reader3);
                                    rowData.seatsD = getSeat(reader3);
                                    rowData.seatsE = getSeat(reader3);
                                    rowData.seatsF = getSeat(reader3);
                                    rows.Add(rowData);
                                }
                                reader3.Close();
                                seating.seats = rows;                               
                            }
                        }
                    }
                }
                connect.Close();
            }
            return seating;
        }

        public Seat getSeat(SqlDataReader reader3)
        {
            reader3.Read();
            Seat seat = new Seat();
            seat.seatId = reader3.GetInt32(0);
            seat.seatNo = reader3.GetString(1);
            seat.seatStatus = reader3.GetString(2);
            seat.seatBookedBy = (reader3.GetSqlInt32(3).IsNull) ? 0 : reader3.GetInt32(3);
            seat.seatBookingDate = (reader3.GetSqlDateTime(4).IsNull) ? null : reader3.GetDateTime(4);
            seat.seatingId = reader3.GetInt32(5);
            return seat;
        }
        public void addSeats(int rowNo, string sql, SqlConnection connect, string initial, int seatingId)
        {
            using (var command = new SqlCommand(sql, connect))
            {
                command.Parameters.AddWithValue("@seatNo", initial + rowNo);
                command.Parameters.AddWithValue("@seatStatus", "Vacant");
                command.Parameters.AddWithValue("@seatingId", seatingId);
                command.ExecuteNonQuery();
            }
        }



        [HttpPost("{id}")]
        public IActionResult addFlight(AddFlight flight)
        {
            int flightId = 0;
            try
            {
                using (var connect = new SqlConnection(conString))
                {
                    connect.Open();
                    int ecoSeatId = this.addSeating("Economic", flight.ecoSeatingCapacity, flight.ecoSeatCost);
                    int busSeatId = this.addSeating("Business", flight.busSeatingCapacity, flight.busSeatCost);
                    string sql = "INSERT INTO flights(flight_name,airlines,arrival_time,arrival_date,depature_time,depature_date,from_location,from_airport,to_location,to_airport,economic_seating,business_seating) VALUES(" +
                        "@flightName,@airlines,@arrivalTime,@arrivalDate,@depatureTime,@depatureDate,@fromLocation,@fromAirport,@toLocation,@toAirport,@ecoSeatId,@busSeatId)";

                    using (var command = new SqlCommand(sql, connect))
                    {
                        command.Parameters.AddWithValue("@flightName", flight.flightName);
                        command.Parameters.AddWithValue("@airlines", flight.airLines);
                        command.Parameters.AddWithValue("@arrivalTime", flight.arrivalTime);
                        command.Parameters.AddWithValue("@arrivalDate", flight.arrivalDate);
                        command.Parameters.AddWithValue("@depatureTime", flight.depatureTime);
                        command.Parameters.AddWithValue("@depatureDate", flight.depatureDate);
                        command.Parameters.AddWithValue("@fromLocation", flight.fromLocation);
                        command.Parameters.AddWithValue("@fromAirport", flight.fromAirport);
                        command.Parameters.AddWithValue("@toLocation", flight.toLocation);
                        command.Parameters.AddWithValue("@toAirport", flight.toAirport);
                        command.Parameters.AddWithValue("@ecoSeatId", ecoSeatId);
                        command.Parameters.AddWithValue("@busSeatId", busSeatId);
                        command.ExecuteNonQuery();
                    }

                    sql = "select max(flight_id) from flights";
                    using (var command = new SqlCommand(sql, connect))
                    {

                        using (var reader = command.ExecuteReader())
                        {
                            reader.Read();
                            flightId = reader.GetInt32(0);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("User", ex.GetBaseException().ToString());
                return BadRequest(ModelState);
            }
            return Created();
        }
    }
}

