using NexiumCode.Models;
using NexiumCode.Repositories;
using System.Text.Json;

namespace NexiumCode.Services
{
    public class XPService : IXPService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<XPService> _logger;

        public XPService(IUserRepository userRepository, ILogger<XPService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task AddXP(int userId, int xp, string skillBranch, string reason)
        {
            var user = await _userRepository.GetById(userId);
            if (user == null) return;

            _logger.LogInformation($"Adding {xp} XP to user {userId} for {reason}");

            user.CurrentXP += xp;
            user.TotalXP += xp;

            // Обновляем прогресс навыков
            await UpdateSkillProgress(user, skillBranch, xp);

            await CheckLevelUp(user);

            await _userRepository.Update(user);
            await _userRepository.SaveChanges();
        }

        public async Task AddELO(int userId, int elo, string reason)
        {
            var user = await _userRepository.GetById(userId);
            if (user == null) return;

            _logger.LogInformation($"Adding {elo} ELO to user {userId} for {reason}");

            user.Rating += elo;
            if (user.Rating < 0) user.Rating = 0;

            if (user.Rating >= 2000)
            {
                await AddAchievement(userId, "Master - Reached 2000 ELO");
            }
            else if (user.Rating >= 1700)
            {
                await AddAchievement(userId, "Expert - Reached 1700 ELO");
            }

            await _userRepository.Update(user);
            await _userRepository.SaveChanges();
        }

        public async Task UpdateStreak(int userId)
        {
            var user = await _userRepository.GetById(userId);
            if (user == null) return;

            var today = DateTimeOffset.UtcNow.Date;
            var lastActivity = user.LastActivityDate?.Date;

            if (lastActivity == null || lastActivity == today)
            {
                user.LastActivityDate = DateTimeOffset.UtcNow;
            }
            else if (lastActivity == today.AddDays(-1))
            {
                user.CurrentStreak++;
                user.LastActivityDate = DateTimeOffset.UtcNow;

                if (user.CurrentStreak == 7)
                {
                    await AddXP(userId, 50, "all", "7 day streak bonus");
                    await AddELO(userId, 20, "7 day streak bonus");
                    await AddAchievement(userId, "Week Warrior - 7 day streak");
                }
                else if (user.CurrentStreak == 14)
                {
                    await AddXP(userId, 100, "all", "14 day streak bonus");
                    await AddELO(userId, 50, "14 day streak bonus");
                    await AddAchievement(userId, "Fortnight Fighter - 14 day streak");
                }
                else if (user.CurrentStreak == 30)
                {
                    await AddXP(userId, 300, "all", "30 day streak bonus");
                    await AddELO(userId, 100, "30 day streak bonus");
                    await AddAchievement(userId, "Month Master - 30 day streak");
                }
            }
            else
            {
                user.CurrentStreak = 1;
                user.LastActivityDate = DateTimeOffset.UtcNow;
            }

            await _userRepository.Update(user);
            await _userRepository.SaveChanges();
        }

        public async Task AddAchievement(int userId, string achievement)
        {
            var user = await _userRepository.GetById(userId);
            if (user == null) return;

            var achievements = JsonSerializer.Deserialize<List<string>>(user.Achievements) ?? [];

            if (!achievements.Contains(achievement))
            {
                achievements.Add(achievement);
                user.Achievements = JsonSerializer.Serialize(achievements);

                _logger.LogInformation($"Achievement unlocked for user {userId}: {achievement}");

                await _userRepository.Update(user);
                await _userRepository.SaveChanges();
            }
        }

        public int CalculateXPForNextLevel(int currentLevel)
        {
            return (int)(100 * currentLevel * 1.15);
        }

        private async Task UpdateSkillProgress(User user, string skillBranch, int xp)
        {
            int progressIncrease = CalculateProgressIncrease(xp, skillBranch);

            switch (skillBranch.ToLower())
            {
                case "theory":
                    user.TheoryMasterProgress = Math.Min(user.TheoryMasterProgress + progressIncrease, 100);
                    await CheckRankUp(user, "theory");
                    break;

                case "practice":
                    user.PracticeProProgress = Math.Min(user.PracticeProProgress + progressIncrease, 100);
                    await CheckRankUp(user, "practice");
                    break;

                case "quiz":
                    user.QuizChampionProgress = Math.Min(user.QuizChampionProgress + progressIncrease, 100);
                    await CheckRankUp(user, "quiz");
                    break;

                case "community":
                    user.CommunityStarProgress = Math.Min(user.CommunityStarProgress + progressIncrease, 100);
                    await CheckRankUp(user, "community");
                    break;

                case "all":
                    int bonus = Math.Max(progressIncrease / 4, 1);

                    user.TheoryMasterProgress = Math.Min(user.TheoryMasterProgress + bonus, 100);
                    user.PracticeProProgress = Math.Min(user.PracticeProProgress + bonus, 100);
                    user.QuizChampionProgress = Math.Min(user.QuizChampionProgress + bonus, 100);
                    user.CommunityStarProgress = Math.Min(user.CommunityStarProgress + bonus, 100);

                    await CheckRankUp(user, "theory");
                    await CheckRankUp(user, "practice");
                    await CheckRankUp(user, "quiz");
                    await CheckRankUp(user, "community");
                    break;
            }
        }

        private int CalculateProgressIncrease(int xp, string skillBranch)
        {
            int baseIncrease = xp / 3;

            switch (skillBranch.ToLower())
            {
                case "practice":
                    return baseIncrease + 5;
                case "theory":
                    return baseIncrease + 3;
                case "quiz":
                    return baseIncrease + 2;
                default:
                    return baseIncrease;
            }
        }

        private async Task CheckRankUp(User user, string skillType)
        {
            int currentProgress = 0;
            int requiredProgress = 100;

            switch (skillType.ToLower())
            {
                case "theory":
                    currentProgress = user.TheoryMasterProgress;
                    if (currentProgress >= requiredProgress)
                    {
                        user.TheoryMasterRank++;
                        user.TheoryMasterProgress = 0; // Сброс прогресса при повышении ранга
                        _logger.LogInformation($"User {user.Id} reached Theory Master Rank {user.TheoryMasterRank}!");
                        await AddELO(user.Id, 30, $"Theory Master Rank {user.TheoryMasterRank}");
                        await AddAchievement(user.Id, $"Theory Master Rank {user.TheoryMasterRank}");
                    }
                    break;

                case "practice":
                    currentProgress = user.PracticeProProgress;
                    if (currentProgress >= requiredProgress)
                    {
                        user.PracticeProRank++;
                        user.PracticeProProgress = 0;
                        _logger.LogInformation($"User {user.Id} reached Practice Pro Rank {user.PracticeProRank}!");
                        await AddELO(user.Id, 30, $"Practice Pro Rank {user.PracticeProRank}");
                        await AddAchievement(user.Id, $"Practice Pro Rank {user.PracticeProRank}");
                    }
                    break;

                case "quiz":
                    currentProgress = user.QuizChampionProgress;
                    if (currentProgress >= requiredProgress)
                    {
                        user.QuizChampionRank++;
                        user.QuizChampionProgress = 0;
                        _logger.LogInformation($"User {user.Id} reached Quiz Champion Rank {user.QuizChampionRank}!");
                        await AddELO(user.Id, 30, $"Quiz Champion Rank {user.QuizChampionRank}");
                        await AddAchievement(user.Id, $"Quiz Champion Rank {user.QuizChampionRank}");
                    }
                    break;

                case "community":
                    currentProgress = user.CommunityStarProgress;
                    if (currentProgress >= requiredProgress)
                    {
                        user.CommunityStarRank++;
                        user.CommunityStarProgress = 0;
                        _logger.LogInformation($"User {user.Id} reached Community Star Rank {user.CommunityStarRank}!");
                        await AddELO(user.Id, 30, $"Community Star Rank {user.CommunityStarRank}");
                        await AddAchievement(user.Id, $"Community Star Rank {user.CommunityStarRank}");
                    }
                    break;
            }
        }

        private async Task CheckLevelUp(User user)
        {
            int xpNeeded = CalculateXPForNextLevel(user.Level);

            while (user.CurrentXP >= xpNeeded)
            {
                user.CurrentXP -= xpNeeded;
                user.Level++;

                _logger.LogInformation($"User {user.Id} leveled up to level {user.Level}!");

                await AddELO(user.Id, 10, $"Level up to {user.Level}");

                if (user.Level % 10 == 0)
                {
                    await AddAchievement(user.Id, $"⭐ Level {user.Level} Reached");
                }

                xpNeeded = CalculateXPForNextLevel(user.Level);
            }
        }

        public async Task UpdateSkillProgress(int userId, string skillType, int progressIncrement)
        {
            var user = await _userRepository.GetById(userId);
            if (user == null) return;

            switch (skillType.ToLower())
            {
                case "theory":
                    user.TheoryMasterProgress = Math.Min(user.TheoryMasterProgress + progressIncrement, 100);
                    break;
                case "practice":
                    user.PracticeProProgress = Math.Min(user.PracticeProProgress + progressIncrement, 100);
                    break;
                case "quiz":
                    user.QuizChampionProgress = Math.Min(user.QuizChampionProgress + progressIncrement, 100);
                    break;
                case "community":
                    user.CommunityStarProgress = Math.Min(user.CommunityStarProgress + progressIncrement, 100);
                    break;
            }

            await CheckRankUp(user, skillType);
            await _userRepository.Update(user);
            await _userRepository.SaveChanges();
        }
    }
}