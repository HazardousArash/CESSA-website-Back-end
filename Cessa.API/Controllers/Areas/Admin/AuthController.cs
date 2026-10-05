namespace Cessa.Api.Controllers.Areas.Admin
{
    using Cessa.Api.Helpers;
    using Cessa.Application.Features.Auth;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Threading.Tasks;

    public record AdminRegisterRequest(string FirstName, string LastName, string Email, string Password);
    public record LoginRequest(string Email, string Password);
    public record OtpRequest(string Email, string Code);
    public record RefreshRequest(string? AccessToken, string? RefreshToken);
    public record ForgotPasswordRequest(string Email);
    public record ResetPasswordRequest(string Email, string Token, string NewPassword);

    [Route("api/admin/[controller]")]
    [ApiExplorerSettings(GroupName = "admin")]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("auth-policy")]
    public class AuthController : ApiControllerBase
    {
        // CRITICAL FIX: Only existing logged-in Admins can create new Admins
        [Authorize(Roles = "Admin")]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AdminRegisterRequest request)
        {
            var command = new RegisterCommand(request.FirstName, request.LastName, request.Email, request.Password, "Admin");
            var message = await Mediator.Send(command);
            return StatusCode(StatusCodes.Status201Created, ApiResult.Created(message));
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] Guid userId, [FromQuery] string token)
        {
            await Mediator.Send(new VerifyEmailCommand(userId, token));
            return Ok(ApiResult.Success("Email verified successfully. You can now login."));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var command = new LoginWithPasswordCommand(request.Email, request.Password, "Admin");
            var result = await Mediator.Send(command);
            SetTokenCookies(result.AccessToken, result.RefreshToken);
            return Ok(ApiResult.Success(result, "Admin logged in successfully."));
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] OtpRequest request)
        {
            var command = new LoginWithOtpCommand(request.Email, request.Code, "Admin");
            var result = await Mediator.Send(command);
            SetTokenCookies(result.AccessToken, result.RefreshToken);
            return Ok(ApiResult.Success(result, "OTP verified successfully."));
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest? request)
        {
            var accessToken = request?.AccessToken ?? Request.Cookies["jwt"];
            var refreshToken = request?.RefreshToken ?? Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
                return Unauthorized(ApiResult.Unauthorized("Tokens are missing"));

            var result = await Mediator.Send(new RefreshTokenCommand(accessToken, refreshToken));
            SetTokenCookies(result.AccessToken, result.RefreshToken);

            return Ok(ApiResult.Success(result, "Tokens refreshed successfully."));
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (!string.IsNullOrEmpty(refreshToken))
            {
                await Mediator.Send(new LogoutCommand(refreshToken));
            }
            ClearTokenCookies();
            return Ok(ApiResult.Success("Logged out successfully."));
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            await Mediator.Send(new ForgotPasswordCommand(request.Email));
            return Ok(ApiResult.Success("If your email is registered and confirmed, a reset link has been sent to your email."));
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            await Mediator.Send(new ResetPasswordCommand(request.Email, request.Token, request.NewPassword));
            return Ok(ApiResult.Success("Your password has been reset successfully. You can now log in."));
        }
    }
}
