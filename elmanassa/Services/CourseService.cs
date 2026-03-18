using elmanassa.ApplicationDbContext;
using elmanassa.DTOs;
using elmanassa.Models;
using Microsoft.EntityFrameworkCore;
using elmanassa.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace elmanassa.Services
{
    public interface ICourseService
    {
        Task<List<CourseDTO>> GetCoursesAsync(string? category = null, string? level = null, string? search = null, int page = 1, int perPage = 12);
        Task<List<CourseDTO>> GetPopularCoursesAsync(int page = 1, int perPage = 12);
        Task<CourseDTO?> GetCourseByIdAsync(int id);
        Task<List<ReviewDTO>> GetCourseReviewsAsync(int courseId, int page = 1, int perPage = 10);
        Task<ReviewDTO?> AddReviewAsync(int courseId, Guid userId, ReviewCreateDTO dto);
        Task<CourseDTO> CreateCourseAsync(Guid instructorId, CourseCreateDTO dto);
        Task<int> GetCourseCountAsync();
    }

    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _repo;

        public CourseService(ICourseRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<CourseDTO>> GetCoursesAsync(string? category = null, string? level = null, string? search = null, int page = 1, int perPage = 12)
        {
            var query = _repo.QueryPublished().AsQueryable();

            if (!string.IsNullOrEmpty(category))
                query = query.Where(c => c.Category == category);

            if (!string.IsNullOrEmpty(level))
                query = query.Where(c => c.Level == level);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(c => c.Title.Contains(search) || c.Description!.Contains(search));

            return await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .Select(c => new CourseDTO
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    Category = c.Category,
                    InstructorId = c.InstructorId,
                    Rating = c.Rating,
                    Duration = c.Duration,
                    LecturesCount = c.LecturesCount,
                    Level = c.Level,
                    Language = c.Language,
                    StudentsCount = c.StudentsCount,
                    Price = c.Price,
                    ImageUrl = c.ImageUrl,
                    Status = c.Status,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<List<CourseDTO>> GetPopularCoursesAsync(int page = 1, int perPage = 12)
        {
            var query = _repo.QueryPublished().AsQueryable();

            return await query
                .OrderByDescending(c => c.Rating)
                .ThenByDescending(c => c.StudentsCount)
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .Select(c => new CourseDTO
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    Category = c.Category,
                    InstructorId = c.InstructorId,
                    Rating = c.Rating,
                    Duration = c.Duration,
                    LecturesCount = c.LecturesCount,
                    Level = c.Level,
                    Language = c.Language,
                    StudentsCount = c.StudentsCount,
                    Price = c.Price,
                    ImageUrl = c.ImageUrl,
                    Status = c.Status,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<CourseDTO?> GetCourseByIdAsync(int id)
        {
            var c = await _repo.GetByIdAsync(id);
            if (c == null || c.Status != "published") return null;

            return new CourseDTO
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                Category = c.Category,
                InstructorId = c.InstructorId,
                Rating = c.Rating,
                Duration = c.Duration,
                LecturesCount = c.LecturesCount,
                Level = c.Level,
                Language = c.Language,
                StudentsCount = c.StudentsCount,
                Price = c.Price,
                ImageUrl = c.ImageUrl,
                Status = c.Status,
                CreatedAt = c.CreatedAt
            };
        }

        public async Task<List<ReviewDTO>> GetCourseReviewsAsync(int courseId, int page = 1, int perPage = 10)
        {
            var reviews = await _repo.GetReviewsAsync(courseId, page, perPage);
            return reviews.Select(r => new ReviewDTO
            {
                Id = r.Id,
                UserId = r.UserId,
                UserName = r.User?.Name ?? "Unknown",
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            }).ToList();
        }

        public async Task<ReviewDTO?> AddReviewAsync(int courseId, Guid userId, ReviewCreateDTO dto)
        {
            // Check if user is enrolled
            var enrollment = await _repo.GetEnrollmentAsync(userId, courseId);
            if (enrollment == null)
                return null;

            // Check if already reviewed
            var existing = (await _repo.GetReviewsAsync(courseId)).FirstOrDefault(r => r.UserId == userId);
            if (existing != null)
                return null;

            var review = new Review
            {
                UserId = userId,
                CourseId = courseId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddReviewAsync(review);
            await _repo.SaveChangesAsync();

            var userName = enrollment.User?.Name ?? "Unknown";

            return new ReviewDTO
            {
                Id = review.Id,
                UserId = review.UserId,
                UserName = userName,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt
            };
        }

        public async Task<CourseDTO> CreateCourseAsync(Guid instructorId, CourseCreateDTO dto)
        {
            var course = new Course
            {
                Title = dto.Title,
                Description = dto.Description,
                Category = dto.Category,
                InstructorId = instructorId,
                Rating = 0,
                Duration = dto.Duration,
                LecturesCount = dto.LecturesCount,
                Level = dto.Level,
                Language = dto.Language,
                StudentsCount = 0,
                Price = dto.Price,
                ImageUrl = dto.ImageUrl,
                Status = dto.Status ?? "draft",
                CreatedAt = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
            };

            await _repo.AddCourseAsync(course);
            await _repo.SaveChangesAsync();

            return new CourseDTO
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                Category = course.Category,
                InstructorId = course.InstructorId,
                Rating = course.Rating,
                Duration = course.Duration,
                LecturesCount = course.LecturesCount,
                Level = course.Level,
                Language = course.Language,
                StudentsCount = course.StudentsCount,
                Price = course.Price,
                ImageUrl = course.ImageUrl,
                Status = course.Status,
                CreatedAt = course.CreatedAt
            };
        }

        public async Task<int> GetCourseCountAsync()
        {
            return await _repo.CountPublishedAsync();
        }
    }
}
