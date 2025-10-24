using System.ComponentModel.DataAnnotations;

namespace NexiumCode.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public int Rating { get; set; } = 200;

        public int Level { get; set; } = 1;
        public int CurrentXP { get; set; } = 0;
        public int TotalXP { get; set; } = 0;

        public int TheoryMasterProgress { get; set; } = 0;
        public int PracticeProProgress { get; set; } = 0;
        public int QuizChampionProgress { get; set; } = 0;
        public int CommunityStarProgress { get; set; } = 0;

        public int TheoryMasterRank { get; set; } = 0;
        public int PracticeProRank { get; set; } = 0;
        public int QuizChampionRank { get; set; } = 0;
        public int CommunityStarRank { get; set; } = 0;

        public int CurrentStreak { get; set; } = 0;
        public DateTimeOffset? LastActivityDate { get; set; }

        public string Achievements { get; set; } = "[]";

        public string AvatarUrl { get; set; } = "/images/avatars/default-avatar.png";
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public ICollection<Progress> Progresses { get; set; }
        public ICollection<Certificate> Certificates { get; set; }
        public ICollection<ForumThread> ForumThreads { get; set; }
        public ICollection<ForumReply> ForumReplies { get; set; }
    }
}
