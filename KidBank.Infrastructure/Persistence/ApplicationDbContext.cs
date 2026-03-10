using KidBank.Application.Common.Interfaces;
using KidBank.Domain.Entities;
using KidBank.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly IDataEncryptor? _encryptor;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IDataEncryptor? encryptor = null) 
        : base(options)
    {
        _encryptor = encryptor;
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
    public DbSet<ClientSetting> ClientSettings => Set<ClientSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        if (_encryptor == null) return;

        var converter = new EncryptedStringConverter(_encryptor);
        var nullableConverter = new EncryptedNullableStringConverter(_encryptor);

        modelBuilder.Entity<User>(b =>
        {
            b.Property(u => u.Email).HasConversion(converter).HasMaxLength(1024);
            b.Property(u => u.PasswordHash).HasConversion(converter).HasMaxLength(1024);
            b.Property(u => u.FirstName).HasConversion(converter).HasMaxLength(512);
            b.Property(u => u.LastName).HasConversion(converter).HasMaxLength(512);
        });

        modelBuilder.Entity<VirtualCard>(b =>
        {
            b.Property(vc => vc.CardNumber).HasConversion(converter).HasMaxLength(512);
            b.Property(vc => vc.CardHolderName).HasConversion(converter).HasMaxLength(512);
            b.Property(vc => vc.Cvv).HasConversion(converter).HasMaxLength(512);
        });

        modelBuilder.Entity<ChatMessage>(b =>
        {
            b.Property(cm => cm.Content).HasConversion(converter).HasMaxLength(4000);
        });

        modelBuilder.Entity<AuditLog>(b =>
        {
            b.Property(al => al.UserEmail).HasConversion(nullableConverter).HasMaxLength(1024);
            b.Property(al => al.Exception).HasConversion(nullableConverter);
        });

        modelBuilder.Entity<Transaction>(b =>
        {
            b.Property(t => t.Description).HasConversion(nullableConverter).HasMaxLength(2000);
        });
    }
}
