namespace NexiumCode.Services
{
    public interface IXPService
    {
        Task AddXP(int userId, int xp, string skillBranch, string reason);
        Task AddELO(int userId, int elo, string reason);
        Task UpdateStreak(int userId);
        Task AddAchievement(int userId, string achievement);
        int CalculateXPForNextLevel(int currentLevel);

        Task UpdateSkillProgress(int userId, string skillType, int progressIncrement);
    }
}