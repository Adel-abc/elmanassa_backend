using elmanassa.ApplicationDbContext;
using elmanassa.Models;
using Microsoft.EntityFrameworkCore;

namespace elmanassa.Repositories
{
    public class CourseRepository : ICourseRepository
    {
        private readonly AppDbContext _context;

        public CourseRepository(AppDbContext context)
        {
            _context = context;
        }

        public IQueryable<Course> QueryPublished()
        {
            return _context.Courses.Where(c => c.Status == "published");
        }

        public async Task<Course?> GetByIdAsync(int id)
        {
            return await _context.Courses.FirstOrDefaultAsync(c => c.Id == id && c.Status == "published");
        }

        public async Task<int> CountPublishedAsync()
        {
            return await _context.Courses.Where(c => c.Status == "published").CountAsync();
        }

        public async Task<List<Review>> GetReviewsAsync(int courseId, int page = 1, int perPage = 10)
        {
            return await _context.Reviews
                .Where(r => r.CourseId == courseId)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .ToListAsync();
        }

        public async Task AddReviewAsync(Review review)
        {
            await _context.Reviews.AddAsync(review);
        }

        public async Task AddCourseAsync(Course course)
        {
            await _context.Courses.AddAsync(course);
        }

        public async Task<Enrollment?> GetEnrollmentAsync(Guid userId, int courseId)
        {
            return await _context.Enrollments.FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}