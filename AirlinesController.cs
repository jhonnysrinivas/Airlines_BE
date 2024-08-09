using AirLines_BE.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace AirLines_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AirlinesController : ControllerBase
    {
        private readonly string? conString;
        public AirlinesController(IConfiguration config)
        {
            conString = config["ConnectionStrings:SqlServerDb"];
        }

        [HttpGet("{id}")]
        public IActionResult getAirlinesDetails(int id)
        {
            Airlines airline = new Airlines();
            try
            {
                using (var connect = new SqlConnection(conString))
                {
                    connect.Open();
                    string sql = "select * from airlines where airlines_id = " + id;
                    using (var command = new SqlCommand(sql, connect))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            reader.Read();
                            airline.airlinesId = reader.GetInt32(0);
                            airline.airlinesName = reader.GetString(1);
                            airline.airlinesLogo = reader.GetString(2);
                        }
                    }
                    connect.Close();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("User", "No User Found");
                return BadRequest(ModelState);
            }
            return Ok(airline);
        }

        [HttpGet]
        public IActionResult getAirlines()
        {
            List<Airlines> airlines = new List<Airlines>();
            try
            {
                using (var connect = new SqlConnection(conString))
                {
                    connect.Open();
                    string sql = "select * from airlines";
                    using (var command = new SqlCommand(sql, connect))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Airlines airline = new Airlines();
                                airline.airlinesId = reader.GetInt32(0);
                                airline.airlinesName = reader.GetString(1);
                                airline.airlinesLogo = reader.GetString(2);
                                airlines.Add(airline);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("User", "No User Found");
                return BadRequest(ModelState);
            }
            return Ok(airlines);
        }

        [HttpPost]
        public IActionResult addAirlines(Airlines airline)
        {
            try
            {
                using (var connection = new SqlConnection(conString))
                {
                    connection.Open();
                    string sql = "INSERT INTO airlines(airlines_name,airlines_logo) VALUES" +
                        "(@airlinesName,@airlinesLogo)";
                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@airlinesName", airline.airlinesName);
                        command.Parameters.AddWithValue("@airlinesLogo", airline.airlinesLogo);
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("pl", ex.GetBaseException().ToString());
                return BadRequest(ModelState);
            }
            return Ok();
        }

        [HttpPut("{id}")]
        public IActionResult updateAirlines(int id, Airlines airlines)
        {
            try
            {
                using (var connection = new SqlConnection(conString))
                {
                    connection.Open();
                    string sql = "UPDATE airlines set airlines_name=@airlinesName,airlines_logo=@airlinesLogo";
                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@airlinesName", airlines.airlinesName);
                        command.Parameters.AddWithValue("@airlinesLogo", airlines.airlinesLogo);
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("pl", ex.GetBaseException().ToString());
                return BadRequest(ModelState);
            }
            return Ok();
        }
    }
}
