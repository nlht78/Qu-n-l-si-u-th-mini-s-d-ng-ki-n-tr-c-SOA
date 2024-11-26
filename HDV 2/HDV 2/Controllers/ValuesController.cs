using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using HDV_2.Models;
using HDV.Filters;
namespace HDV_2.Controllers
{
    public class ValuesController : ApiController
    {
        // GET api/values

        [JwtAuthentication]
        public IEnumerable<string> Get()
        {
            return new string[] { "Hello" };
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
        private string GenerateJwtToken(string username)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("YourSuperSecretKeyForJwtToken123456!");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
            new Claim(ClaimTypes.Name, username)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        [HttpPost]
        [Route("login")]
        public IHttpActionResult Login([FromBody] LoginModel model)
        {
            using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["HDVConnection"].ConnectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT * FROM [User] WHERE UserName = @UserName AND Password = @Password", connection);
                command.Parameters.AddWithValue("@UserName", model.UserName);
                command.Parameters.AddWithValue("@Password", model.Password); // Hash tại client (MD5/Base64).

                var reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    var token = GenerateJwtToken(model.UserName);
                    return Ok(new { Token = token });
                }
                else
                {
                    return Unauthorized();
                }
            }
        }


    }
}
