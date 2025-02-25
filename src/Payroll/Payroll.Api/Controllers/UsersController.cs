using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Payroll.Api.Models;
using Payroll.Api.Services;
using Payroll.Services;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private IUserAuthService _userService;
        private readonly Payroll.Services.UserResolverService userResolverService;
        private readonly NotificationService notificationService;

        public UsersController(IUserAuthService userAuthService, Payroll.Services.UserResolverService userResolverService, NotificationService notificationService)
        {
            _userService = userAuthService;
            this.userResolverService = userResolverService;
            this.notificationService = notificationService;
        }

        /// <summary>
        /// public route that accepts HTTP POST requests containing a username and password in the body. If the username and password are correct then a JWT authentication token and the user details are returned in the response body, and a refresh token cookie (HTTP Only) is returned in the response headers.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticateRequest model)
        {
            var response = await _userService.AuthenticateAsync(model, ipAddress());

            if (response == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            setTokenCookie(response.RefreshToken);

            return Ok(response);
        }

        /// <summary>
        /// public route that accepts HTTP POST requests with a refresh token cookie. If the cookie exists and the refresh token is valid then a new JWT authentication token and the user details are returned in the response body, a new refresh token cookie (HTTP Only) is returned in the response headers and the old refresh token is revoked.
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var response = await _userService.RefreshTokenAsync(refreshToken, ipAddress());

            if (response == null)
                return Unauthorized(new { message = "Invalid token" });

            setTokenCookie(response.RefreshToken);

            return Ok(response);
        }

        /// <summary>
        /// secure route that accepts HTTP POST requests containing a refresh token either in the body or in a cookie, if both are present the token in the body is used. If the refresh token is valid and active then it is revoked and can no longer be used to refresh JWT tokens.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest model)
        {
            // accept token from request body or cookie
            var token = model.Token ?? Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(token))
                return BadRequest(new { message = "Token is required" });

            var response = await _userService.RevokeTokenAsync(token, ipAddress());

            if (!response)
                return NotFound(new { message = "Token not found" });

            return Ok(new { message = "Token revoked" });
        }

        /// <summary>
        /// secure route that accepts HTTP GET requests and returns a list of all the users in the application if the HTTP Authorization header contains a valid JWT token. If there is no auth token or the token is invalid then a 401 Unauthorized response is returned.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        /// <summary>
        /// secure route that accepts HTTP GET requests and returns the details of the user with the specified id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null) return NotFound();

            return Ok(user);
        }

        /// <summary>
        /// secure route that accepts HTTP GET requests and returns a list of all refresh tokens (active and revoked) of the user with the specified id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/refresh-tokens")]
        public async Task<IActionResult> GetRefreshTokens(string id)
        {
            var user = await _userService.GetRefreshTokensAsync(id);
            if (user == null) return NotFound();

            return Ok(user.RefreshTokens);
        }

        /// <summary>
        /// Get Notifications for employee
        /// </summary>
        /// <param name="id"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        [HttpGet("{id}/notifications")]
        public async Task<IActionResult> GetNotifications(string id, DateTime? start = null, DateTime? end = null, int type = 0, bool showSeen = false, int page = 1, int limit = 10)
        {
            var user = await notificationService.GetUserNotifications(
                    id,
                    start,
                    end,
                    showSeen,
                    type,
                    page,
                    limit);
            if (user == null) return NotFound();

            return Ok(user);
        }

        /// <summary>
        /// Returns claims associated with authenticated user
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("diagnostics")]
        public async Task<IActionResult> Diagnostics(string id)
        {
            var json = User.Claims.ToDictionary(a => a.Type, a => a.Value);
            json.Add("GetUserId()", userResolverService.GetUserId());
            json.Add("GetCompanyId()", userResolverService.GetCompanyId().ToString());
            json.Add("GetEmployeeId()", userResolverService.GetEmployeeId().ToString());
            return Ok(json);
        }

        // helper methods
        private void setTokenCookie(string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("refreshToken", token, cookieOptions);
        }

        private string ipAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"];
            else
                return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        }
    }
}
