using elmanassa.Models;

namespace elmanassa.Repositories
{
    public interface ICourseRepository
    {
        IQueryable<Course> QueryPublished();
        Task<Course?> GetByIdAsync(int id);
        Task<int> CountPublishedAsync();
        Task<List<Review>> GetReviewsAsync(int courseId, int page = 1, int perPage = 10);
        Task AddReviewAsync(Review review);
        Task AddCourseAsync(Course course);
        Task<Enrollment?> GetEnrollmentAsync(Guid userId, int courseId);
        Task SaveChangesAsync();
    }
}