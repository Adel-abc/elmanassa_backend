using elmanassa.DTOs;
using elmanassa.Services;
using Microsoft.AspNetCore.Mvc;

namespace elmanassa.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Register a new user (student/teacher/admin)
        /// </summary>
        [HttpPost("signup")]
        public async Task<ActionResult<AuthResponseDTO>> Signup([FromBody] RegisterDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _authService.Register(model);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new AuthResponseDTO { Success = false, Message = "An error occurred during registration" });
            }
        }

        /// <summary>
        /// Register a new teacher user
        /// </summary>
        [HttpPost("signup/teacher")]
        public async Task<ActionResult<AuthResponseDTO>> SignupTeacher([FromBody] TeacherRegisterDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _authService.RegisterTeacher(model);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during teacher registration");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new AuthResponseDTO { Success = false, Message = "An error occurred during teacher registration" });
            }
        }

        /// <summary>
        /// Login with email and password
        /// </summary>
        /// <param name="model">Login credentials</param>
        /// <returns>JWT token and user info if successful</returns>
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDTO>> Login([FromBody] LoginDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _authService.Login(model);

                if (!result.Success)
                {
                    return Unauthorized(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new AuthResponseDTO { Success = false, Message = "An error occurred during login" });
            }
        }


        

        /// <summary>
        /// Check if email is already registered
        /// </summary>
        /// <param name="email">Email to check</param>
        /// <returns>True if email exists, false otherwise</returns>
        [HttpGet("check-email")]
        public async Task<IActionResult> CheckEmail([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(new { message = "Email is required" });
            }

            try
            {
                var exists = await _authService.UserExists(email);
                return Ok(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking email");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "An error occurred while checking email" });
            }
        }
    }
}
