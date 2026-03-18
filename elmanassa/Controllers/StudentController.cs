using elmanassa.DTOs;
using elmanassa.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace elmanassa.Controllers
{
    [ApiController]
    [Route("api/v1/student")]
    [Authorize(Roles = "student")]
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;
        private readonly ILogger<StudentController> _logger;

        public StudentController(IStudentService studentService, ILogger<StudentController> logger)
        {
            _studentService = studentService;
            _logger = logger;
        }

        private Guid GetUserId()
        {
            return Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
        }

        /// <summary>
        /// Get enrolled courses and subjects
        /// </summary>
        [HttpGet("enrollments")]
        public async Task<ActionResult<ApiResponse<List<EnrollmentDTO>>>> GetEnrollments()
        {
            try
            {
                var userId = GetUserId();
                var enrollments = await _studentService.GetEnrollmentsAsync(userId);

                return Ok(new ApiResponse<List<EnrollmentDTO>>(enrollments));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching enrollments");
                return StatusCode(500, new ApiResponse<object>(
                    "An error occurred", "SERVER_ERROR", false));
            }
        }

        /// <summary>
        /// Get student learning progress
        /// </summary>
        [HttpGet("progress")]
        public async Task<ActionResult<ApiResponse<StudentProgressDTO>>> GetProgress()
        {
            try
            {
                var userId = GetUserId();
                var progress = await _studentService.GetProgressAsync(userId);

                return Ok(new ApiResponse<StudentProgressDTO>(progress));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching progress");
                return StatusCode(500, new ApiResponse<object>(
                    "An error occurred", "SERVER_ERROR", false));
            }
        }

        /// <summary>
        /// Update lecture progress
        /// </summary>
        [HttpPost("progress")]
        public async Task<ActionResult<ApiResponse<StudentProgressDTO>>> UpdateProgress([FromBody] LectureProgressUpdateDTO model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ApiResponse<object>(
                        "Invalid progress data", "VALIDATION_ERROR", false));

                var userId = GetUserId();
                await _studentService.UpdateProgressAsync(userId, model);

                var progress = await _studentService.GetProgressAsync(userId);

                return Ok(new ApiResponse<StudentProgressDTO>(progress));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating progress");
                return StatusCode(500, new ApiResponse<object>(
                    "An error occurred", "SERVER_ERROR", false));
            }
        }
    }
}
