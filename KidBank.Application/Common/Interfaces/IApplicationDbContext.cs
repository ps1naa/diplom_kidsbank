using KidBank.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Family> Families { get; }
    DbSet<FamilyInvitation> FamilyInvitations { get; }
    DbSet<Account> Accounts { get; }
    DbSet<Transaction> Transactions { get; }
    DbSet<VirtualCard> VirtualCards { get; }
    DbSet<WishlistGoal> WishlistGoals { get; }
    DbSet<MoneyRequest> MoneyRequests { get; }
    DbSet<TaskAssignment> TaskAssignments { get; }
    DbSet<SpendingLimit> SpendingLimits { get; }
    DbSet<SpendingCategory> SpendingCategories { get; }
    DbSet<EducationModule> EducationModules { get; }
    DbSet<Quiz> Quizzes { get; }
    DbSet<EducationProgress> EducationProgresses { get; }
    DbSet<AchievementDefinition> AchievementDefinitions { get; }
    DbSet<AchievementProgress> AchievementProgresses { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<ChatMessage> ChatMessages { get; }
    DbSet<RecurringAllowance> RecurringAllowances { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<CategoryBlock> CategoryBlocks { get; }

    DbSet<TEntity> Set<TEntity>() where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
