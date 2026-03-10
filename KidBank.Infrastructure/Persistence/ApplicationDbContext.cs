using KidBank.Application.Common.Interfaces;
using KidBank.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Family> Families => Set<Family>();
    public DbSet<FamilyInvitation> FamilyInvitations => Set<FamilyInvitation>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<VirtualCard> VirtualCards => Set<VirtualCard>();
    public DbSet<WishlistGoal> WishlistGoals => Set<WishlistGoal>();
    public DbSet<MoneyRequest> MoneyRequests => Set<MoneyRequest>();
    public DbSet<TaskAssignment> TaskAssignments => Set<TaskAssignment>();
    public DbSet<SpendingLimit> SpendingLimits => Set<SpendingLimit>();
    public DbSet<SpendingCategory> SpendingCategories => Set<SpendingCategory>();
    public DbSet<EducationModule> EducationModules => Set<EducationModule>();
    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<EducationProgress> EducationProgresses => Set<EducationProgress>();
    public DbSet<AchievementDefinition> AchievementDefinitions => Set<AchievementDefinition>();
    public DbSet<AchievementProgress> AchievementProgresses => Set<AchievementProgress>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<RecurringAllowance> RecurringAllowances => Set<RecurringAllowance>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<CategoryBlock> CategoryBlocks => Set<CategoryBlock>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
