using elmanassa.DTOs;
using elmanassa.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace elmanassa.Controllers
{
    [ApiController]
    [Route("api/v1/teacher")]
    [Authorize(Roles = "teacher")]
    public class TeacherController : ControllerBase
    {
        private readonly ITeacherService _teacherService;
        private readonly ILogger<TeacherController> _logger;

        public TeacherController(ITeacherService teacherService, ILogger<TeacherController> logger)
        {
            _teacherService = teacherService;
            _logger = logger;
        }

        private Guid GetTeacherId()
        {
            return Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
        }


        [HttpGet]
        public IActionResult GetAllTeachers()
        {

            return Ok();
        }

        /// <summary>
        /// Get all subjects for teacher
        /// </summary>
        [HttpGet("subjects")]
        public async Task<ActionResult<ApiResponse<List<SubjectDTO>>>> GetSubjects()
        {
            try
            {
                var teacherId = GetTeacherId();
                var subjects = await _teacherService.GetTeacherSubjectsAsync(teacherId);

                return Ok(new ApiResponse<List<SubjectDTO>>(subjects));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching teacher subjects");
                return StatusCode(500, new ApiResponse<object>(
                    "An error occurred", "SERVER_ERROR", false));
            }
        }

        /// <summary>
        /// Create new subject with levels and lectures
        /// </summary>
        [HttpPost("subjects")]
        public async Task<ActionResult<ApiResponse<SubjectDTO>>> CreateSubject([FromBody] SubjectCreateDTO model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ApiResponse<object>(
                        "Invalid subject data", "VALIDATION_ERROR", false));

                var teacherId = GetTeacherId();
                var subject = await _teacherService.CreateSubjectAsync(teacherId, model);

                return CreatedAtAction(nameof(GetSubjects), 
                    new ApiResponse<SubjectDTO>(subject!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating subject");
                return StatusCode(500, new ApiResponse<object>(
                    "An error occurred", "SERVER_ERROR", false));
            }
        }

        /// <summary>
        /// Update subject
        /// </summary>
        [HttpPut("subjects/{id}")]
        public async Task<ActionResult<ApiResponse<SubjectDTO>>> UpdateSubject(Guid id, [FromBody] SubjectUpdateDTO model)
        {
            try
            {
                var teacherId = GetTeacherId();
                var subject = await _teacherService.UpdateSubjectAsync(id, teacherId, model);

                if (subject == null)
                    return NotFound(new ApiResponse<object>(
                        "Subject not found", "NOT_FOUND", false));

                return Ok(new ApiResponse<SubjectDTO>(subject));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating subject");
                return StatusCode(500, new ApiResponse<object>(
                    "An error occurred", "SERVER_ERROR", false));
            }
        }

        /// <summary>
        /// Delete subject
        /// </summary>
        [HttpDelete("subjects/{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteSubject(Guid id)
        {
            try
            {
                var teacherId = GetTeacherId();
                var success = await _teacherService.DeleteSubjectAsync(id, teacherId);

                if (!success)
                    return NotFound(new ApiResponse<object>(
                        "Subject not found", "NOT_FOUND", false));

                return Ok(new ApiResponse<object>(new { message = "تم الحذف" }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting subject");
                return StatusCode(500, new ApiResponse<object>(
                    "An error occurred", "SERVER_ERROR", false));
            }
        }

        /// <summary>
        /// Publish or unpublish subject
        /// </summary>
        [HttpPatch("subjects/{id}/publish")]
        public async Task<ActionResult<ApiResponse<SubjectDTO>>> PublishSubject(Guid id, [FromBody] PublishDTO model)
        {
            try
            {
                var teacherId = GetTeacherId();
                var success = await _teacherService.PublishSubjectAsync(id, teacherId, model.Status);

                if (!success)
                    return NotFound(new ApiResponse<object>(
                        "Subject not found", "NOT_FOUND", false));

                var subjects = await _teacherService.GetTeacherSubjectsAsync(teacherId);
                var subject = subjects.FirstOrDefault(s => s.Id == id);

                return Ok(new ApiResponse<SubjectDTO>(subject!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing subject");
                return StatusCode(500, new ApiResponse<object>(
                    "An error occurred", "SERVER_ERROR", false));
            }
        }

        /// <summary>
        /// Get teacher statistics
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<ApiResponse<TeacherStatsDTO>>> GetStats()
        {
            try
            {
                var teacherId = GetTeacherId();
                var stats = await _teacherService.GetTeacherStatsAsync(teacherId);

                return Ok(new ApiResponse<TeacherStatsDTO>(stats!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching teacher stats");
                return StatusCode(500, new ApiResponse<object>(
                    "An error occurred", "SERVER_ERROR", false));
            }
        }

        /// <summary>
        /// Get students enrolled in teacher's subjects
        /// </summary>
        [HttpGet("students")]
        public async Task<ActionResult<ApiResponse<List<StudentDTO>>>> GetStudents(
            [FromQuery] string? search = null,
            [FromQuery] int page = 1,
            [FromQuery] int per_page = 20)
        {
            try
            {
                var teacherId = GetTeacherId();
                var students = await _teacherService.GetTeacherStudentsAsync(teacherId, search, page, per_page);

                return Ok(new ApiResponse<List<StudentDTO>>(students, true, students.Count));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching teacher students");
                return StatusCode(500, new ApiResponse<object>(
                    "An error occurred", "SERVER_ERROR", false));
            }
        }
    }

    public class PublishDTO
    {
        public string Status { get; set; }
    }
}
