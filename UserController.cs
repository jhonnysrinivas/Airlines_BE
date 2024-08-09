using AirLines_BE.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace AirLines_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly string conString;
        public UserController(IConfiguration config) {
            conString = config["ConnectionStrings:SqlServerDb"];        
        }
        public List<User> list = new List<User>();

        [HttpGet("{id}")]
        public IActionResult getUserDetails(int id)
        {
            User user = new User();
            try
            {
                using (var connect = new SqlConnection(conString))
                {
                    connect.Open();
                    string sql = "select user_id,user_name,email,password,user_type,COALESCE(card_number, '') card_number, COALESCE(card_name, '') card_name,COALESCE(card_expiry, '') card_expiry,COALESCE(card_cvv, '') card_cvv,icon from users where user_id = " + id;
                    using (var command = new SqlCommand(sql, connect))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            reader.Read();
                            user.userId = reader.GetInt32(0);
                            user.userName = reader.GetString(1);
                            user.email = reader.GetString(2);
                            user.password = reader.GetString(3);
                            user.accountType = reader.GetString(4);
                            user.cardNumber = reader.GetString(5);
                            user.cardName = reader.GetString(6);
                            user.expiryDate = reader.GetString(7);
                            user.cvvCode = reader.GetString(8);
                            user.icon = reader.GetString(9);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                ModelState.AddModelError("message", ex.Message);
                return BadRequest(ModelState);
            }
            return Ok(user);
        }
        
        [HttpGet]
        public IActionResult getUsers()
        {
            List<User> users = new List<User>();
            try
            {
                using (var connect = new SqlConnection(conString))
                {
                    connect.Open();
                    string sql = "select * from users";
                    using (var command = new SqlCommand(sql, connect))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                User user = new User();
                                user.userId = reader.GetInt32(0);
                                user.userName = reader.GetString(1);
                                user.email = reader.GetString(2);
                                user.password = reader.GetString(3);
                                user.accountType = reader.GetString(4);
                                user.icon = reader.GetString(5);
                                users.Add(user);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("message","No User Found");
                return BadRequest(ModelState);
            }
            return Ok(users); 
        }

        [HttpPost]
        public IActionResult addUser(User user)
        {
            //validations
            if(user.userName.Length < 8)
            {
                ModelState.AddModelError("message", "UserName must have 8 Characters");
                return BadRequest(ModelState);
            }
            if (user.password.Length < 8)
            {
                ModelState.AddModelError("message", "Password must have 8 Characters");
                return BadRequest(ModelState);
            }
            if (user.email.Length < 8)
            {
                ModelState.AddModelError("message", "Email must have 8 Characters");
                return BadRequest(ModelState);
            }

            try
            {
                using (var connection = new SqlConnection(conString))
                {
                    connection.Open();
                    String sql = "select count(*) from users where user_name = @userName";
                    var command = new SqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@userName", user.userName);
                    var reader = command.ExecuteReader();
                    reader.Read();
                    if(reader.GetInt32(0) > 0)
                    {
                        ModelState.AddModelError("message", "UserName Already Exists");
                        return BadRequest(ModelState);
                    }
                    reader.Close();
                    sql = "INSERT INTO users(user_name,email,password,user_type,icon) VALUES" +
                        "(@userName,@email,@password,@accountType,@icon)";
                    using (command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@userName", user.userName);
                        command.Parameters.AddWithValue("@email", user.email);
                        command.Parameters.AddWithValue("@password", user.password);
                        command.Parameters.AddWithValue("@accountType", user.accountType);
                        command.Parameters.AddWithValue("@icon", "https://bootdey.com/img/Content/avatar/avatar7.png");
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("message", ex.GetBaseException().ToString());
                return BadRequest(ModelState);
            }
            return Created();
        }

        [HttpPut("{id}")]
        public IActionResult updateUser(int id, User user)
        {
            if (user.userName.Length < 8)
            {
                ModelState.AddModelError("message", "UserName must have 8 Characters");
                return BadRequest(ModelState);
            }
            if (user.password.Length < 8)
            {
                ModelState.AddModelError("message", "Password must have 8 Characters");
                return BadRequest(ModelState);
            }
            if (user.email.Length < 8)
            {
                ModelState.AddModelError("message", "Email must have 8 Characters");
                return BadRequest(ModelState);
            }
            try
            {
                using (var connection = new SqlConnection(conString))
                {
                    connection.Open();
                    String sql = "select count(*) from users where user_name = @userName and user_id <> @userId";
                    var command = new SqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@userName", user.userName);
                    command.Parameters.AddWithValue("@userId", id);
                    var reader = command.ExecuteReader();
                    reader.Read();
                    if (reader.GetInt32(0) > 0)
                    {
                        ModelState.AddModelError("message", "UserName Already Exists");
                        return BadRequest(ModelState);
                    }
                    reader.Close();

                    sql = "UPDATE users set user_name=@userName,email=@email,password=@password," +
                        "card_number=@cardNumber,card_name=@cardName,card_expiry=@cardExpiry,card_cvv=@cardCvv,icon=@icon where user_id = @id";
                    using (command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@userName", user.userName);
                        command.Parameters.AddWithValue("@email", user.email);
                        command.Parameters.AddWithValue("@password", user.password);
                        command.Parameters.AddWithValue("@cardNumber", user.cardNumber);
                        command.Parameters.AddWithValue("@cardName", user.cardName);
                        command.Parameters.AddWithValue("@cardExpiry", user.expiryDate);
                        command.Parameters.AddWithValue("@cardCvv", user.cvvCode);
                        command.Parameters.AddWithValue("@id", id);
                        command.Parameters.AddWithValue("@icon", user.icon);
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("message", ex.GetBaseException().ToString());
                return BadRequest(ModelState);
            }
            return Ok();
        }
        
        [HttpPost("login")]
        public IActionResult loginUser(string? userName, string? password)
        {
            User user = new User();
            try
            {
                using (var connection = new SqlConnection(conString))
                {
                    connection.Open();

                    String sql = "select count(*) from users where user_name = @userName and password = @password";
                    var command = new SqlCommand(sql, connection);
                    command.Parameters.AddWithValue("@userName", userName);
                    command.Parameters.AddWithValue("@password", password);
                    var reader = command.ExecuteReader();
                    reader.Read();
                    if (reader.GetInt32(0) == 0)
                    {
                        ModelState.AddModelError("message", "Invalid User Credentials");
                        return BadRequest(ModelState);
                    }
                    reader.Close();

                    sql = "select * from  users where user_name = @userName and password = @password";
                    using (command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@userName", userName);
                        command.Parameters.AddWithValue("@password", password);
                        using (reader = command.ExecuteReader())
                        {
                            if(reader.Read())
                            {
                                user.userId = reader.GetInt32(0);
                                user.userName = reader.GetString(1);
                                user.email = reader.GetString(2);
                                user.password = reader.GetString(3);
                                user.accountType = reader.GetString(4);
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("message", ex.GetBaseException().ToString());
                return BadRequest(ModelState);
            }
            return Ok(user);
        }
    }
}