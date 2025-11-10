using LegendaryCMS.Services;
using Microsoft.AspNetCore.Mvc;

namespace LegendaryCMS.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly DatabaseService _db;

        public AuthController(DatabaseService db)
        {
            _db = db;
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Get session ID from cookie
            if (Request.Cookies.TryGetValue("AGPCMS_SESSION", out var sessionId))
            {
                try
                {
                    using var connection = _db.GetConnection();
                    var command = connection.CreateCommand();
                    command.CommandText = "DELETE FROM Sessions WHERE Id = @sessionId";
                    command.Parameters.AddWithValue("@sessionId", sessionId);
                    command.ExecuteNonQuery();
                }
                catch
                {
                    // Ignore errors
                }
            }

            // Clear cookies
            Response.Cookies.Delete("AGPCMS_SESSION");
            Response.Cookies.Delete("AGPCMS_USER");

            return Ok(new { success = true, message = "Logged out successfully" });
        }

        [HttpGet("status")]
        public IActionResult GetAuthStatus()
        {
            var userId = _db.GetAuthenticatedUserId(HttpContext);

            if (userId == null)
            {
                return Ok(new { authenticated = false });
            }

            var user = _db.GetUserById(userId.Value);

            return Ok(new
            {
                authenticated = true,
                username = user?.Username,
                role = user?.Role
            });
        }
    }
}
