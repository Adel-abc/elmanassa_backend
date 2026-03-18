using elmanassa.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace elmanassa.ApplicationDbContext
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected AppDbContext()
        {
        }

        #region Core Entities
        public DbSet<User> Users { get; set; }
        #endregion

        #region Course & Content
        public DbSet<Course> Courses { get; set; }
        public DbSet<CurriculumSection> CurriculumSections { get; set; }
        public DbSet<CurriculumLecture> CurriculumLectures { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Level> Levels { get; set; }
        public DbSet<Lecture> Lectures { get; set; }
        public DbSet<Review> Reviews { get; set; }
        #endregion

        #region Student Features
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<LectureProgress> LectureProgress { get; set; }
        #endregion

        #region Order & Payment
        public DbSet<Order> Orders { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        #endregion

        #region Interactive Features
        public DbSet<LiveStream> LiveStreams { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<AiConversation> AiConversations { get; set; }
        public DbSet<AiMessage> AiMessages { get; set; }
        #endregion

        #region Content & Messaging
        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<Testimonial> Testimonials { get; set; }
        #endregion

        #region Legacy (Keep for intermediate migration)
        public DbSet<Student> Students { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<AiLearningActivity> AiLearningActivities { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===== User Configuration =====
            modelBuilder.Entity<User>().Property(u => u.Id).ValueGeneratedNever();
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<User>().HasIndex(u => u.Role);

            // ===== Course Configuration =====
            modelBuilder.Entity<Course>()
                .HasOne(c => c.Instructor)
                .WithMany()
                .HasForeignKey(c => c.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Course>().HasIndex(c => c.Category);
            modelBuilder.Entity<Course>().HasIndex(c => c.InstructorId);
            modelBuilder.Entity<Course>().HasIndex(c => c.Status);

            // ===== Curriculum Configuration =====
            modelBuilder.Entity<CurriculumSection>()
                .HasOne(cs => cs.Course)
                .WithMany(c => c.CurriculumSections)
                .HasForeignKey(cs => cs.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<CurriculumSection>().HasIndex(cs => cs.CourseId);

            modelBuilder.Entity<CurriculumLecture>()
                .HasOne(cl => cl.Section)
                .WithMany(cs => cs.Lectures)
                .HasForeignKey(cl => cl.SectionId)
                .OnDelete(DeleteBehavior.Cascade);

            // ===== Subject/Level/Lecture Configuration =====
            modelBuilder.Entity<Subject>()
                .HasOne(s => s.Teacher)
                .WithMany()
                .HasForeignKey(s => s.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Subject>().HasIndex(s => s.TeacherId);

            modelBuilder.Entity<Level>()
                .HasOne(l => l.Subject)
                .WithMany(s => s.Levels)
                .HasForeignKey(l => l.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Lecture>()
                .HasOne(l => l.Level)
                .WithMany(lv => lv.Lectures)
                .HasForeignKey(l => l.LevelId)
                .OnDelete(DeleteBehavior.Cascade);

            // ===== Enrollment Configuration =====
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Subject)
                .WithMany()
                .HasForeignKey(e => e.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Enrollment>().HasIndex(e => e.UserId);

            // ===== LectureProgress Configuration =====
            modelBuilder.Entity<LectureProgress>()
                .HasOne(lp => lp.User)
                .WithMany()
                .HasForeignKey(lp => lp.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<LectureProgress>()
                .HasOne(lp => lp.Lecture)
                .WithMany()
                .HasForeignKey(lp => lp.LectureId)
                .OnDelete(DeleteBehavior.Cascade);

            // ===== Review Configuration =====
            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Course)
                .WithMany()
                .HasForeignKey(r => r.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // ===== Order & Coupon Configuration =====
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Course)
                .WithMany()
                .HasForeignKey(o => o.CourseId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Coupon)
                .WithMany()
                .HasForeignKey(o => o.CouponId)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Order>().HasIndex(o => o.UserId);

            modelBuilder.Entity<Coupon>().HasIndex(c => c.Code).IsUnique();

            // ===== LiveStream & Chat Configuration =====
            modelBuilder.Entity<LiveStream>()
                .HasOne(ls => ls.Instructor)
                .WithMany()
                .HasForeignKey(ls => ls.InstructorId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<LiveStream>()
                .HasMany(ls => ls.ChatMessages)
                .WithOne(cm => cm.Stream)
                .HasForeignKey(cm => cm.StreamId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChatMessage>()
                .HasOne(cm => cm.User)
                .WithMany()
                .HasForeignKey(cm => cm.UserId)
                .OnDelete(DeleteBehavior.Restrict); // prevent multiple cascade paths from User -> ChatMessages via LiveStream

            // ===== AI Configuration =====
            modelBuilder.Entity<AiConversation>()
                .HasOne(ac => ac.User)
                .WithMany()
                .HasForeignKey(ac => ac.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<AiConversation>()
                .HasMany(ac => ac.Messages)
                .WithOne(am => am.Conversation)
                .HasForeignKey(am => am.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            // ===== Blog & Content Configuration =====
            modelBuilder.Entity<BlogPost>()
                .HasOne(bp => bp.Author)
                .WithMany()
                .HasForeignKey(bp => bp.AuthorId)
                .OnDelete(DeleteBehavior.SetNull);

            // Legacy entity configurations
            modelBuilder.Entity<Student>().Property(s => s.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Student>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Teacher>().Property(t => t.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Teacher>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AiLearningActivity>().Property(a => a.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<AiLearningActivity>()
                .HasOne(a => a.Student)
                .WithMany(s => s.AiLearningActivities)
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
