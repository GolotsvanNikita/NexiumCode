using Microsoft.EntityFrameworkCore;
using NexiumCode.Models;

namespace NexiumCode.Context
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Progress> Progresses { get; set; }
        public DbSet<ProgressLesson> ProgressLessons { get; set; }
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
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

            modelBuilder.Entity<ForumReply>()
                .HasOne(fr => fr.Thread)
                .WithMany(ft => ft.Replies)
                .HasForeignKey(fr => fr.ThreadId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ForumReply>()
                .HasOne(fr => fr.User)
                .WithMany(u => u.ForumReplies)
                .HasForeignKey(fr => fr.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ForumThread>()
                .HasOne(ft => ft.User)
                .WithMany(u => u.ForumThreads)
                .HasForeignKey(ft => ft.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ProgressLesson>()
            .HasKey(pl => pl.Id);

            modelBuilder.Entity<ProgressLesson>()
                .HasOne(pl => pl.Progress)
                .WithMany(p => p.ProgressLessons)
                .HasForeignKey(pl => pl.ProgressId);
        }
    }
}
