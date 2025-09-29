using Microsoft.EntityFrameworkCore;
using NexiumCode.Models;

namespace NexiumCode.Context
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Progress> Progresses { get; set; }
        public DbSet<Certificate> Certificates { get; set; }
        public DbSet<ForumThread> ForumThreads { get; set; }
        public DbSet<ForumReply> ForumReplies { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<QuizQuestion> Quizzes { get; set; }
        public DbSet<PracticeTask> PracticeTasks { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ForumThread>().HasQueryFilter(t => !t.IsDeleted);
        }
    }
}
