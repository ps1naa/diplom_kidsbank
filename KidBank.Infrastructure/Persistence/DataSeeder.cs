using KidBank.Domain.Entities;
using KidBank.Domain.Enums;
using KidBank.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace KidBank.Infrastructure.Persistence;

public static class DataSeeder
{
    public static async Task SeedReferenceDataAsync(ApplicationDbContext context)
    {
        if (await context.EducationMissions.AnyAsync()) return;

        SeedAchievements(context);
        SeedMissionsAndModules(context);

        await context.SaveChangesAsync();
    }

    public static async Task SeedTestDataAsync(
        ApplicationDbContext context,
        Func<string, string> hashPassword,
        Func<string, string> computeEmailHash)
    {
        if (await context.Users.AnyAsync()) return;

        var family = FamilyService.Create("Ивановы");
        var family2 = FamilyService.Create("Петровы");
        context.Families.AddRange(family, family2);

        var parent = UserService.CreateParent(
            "parent@kidbank.by", hashPassword("Parent123!"),
            "Александр", "Иванов", new DateTime(1985, 3, 15, 0, 0, 0, DateTimeKind.Utc),
            family.Id, computeEmailHash("parent@kidbank.by"));
        parent.TotalXp = 500;
        parent.CurrentStreak = 12;
        parent.LastActivityDate = DateTime.UtcNow;

        var parent2 = UserService.CreateParent(
            "mama@kidbank.by", hashPassword("Parent123!"),
            "Елена", "Иванова", new DateTime(1987, 7, 22, 0, 0, 0, DateTimeKind.Utc),
            family.Id, computeEmailHash("mama@kidbank.by"));
        parent2.TotalXp = 300;
        parent2.CurrentStreak = 8;
        parent2.LastActivityDate = DateTime.UtcNow;

        var kid1 = UserService.CreateKid(
            "kid1@kidbank.by", hashPassword("Kid12345!"),
            "Максим", "Иванов", new DateTime(2012, 5, 10, 0, 0, 0, DateTimeKind.Utc),
            family.Id, computeEmailHash("kid1@kidbank.by"));
        kid1.TotalXp = 1250;
        kid1.CurrentStreak = 15;
        kid1.LastActivityDate = DateTime.UtcNow;

        var kid2 = UserService.CreateKid(
            "kid2@kidbank.by", hashPassword("Kid12345!"),
            "Алиса", "Иванова", new DateTime(2014, 9, 3, 0, 0, 0, DateTimeKind.Utc),
            family.Id, computeEmailHash("kid2@kidbank.by"));
        kid2.TotalXp = 820;
        kid2.CurrentStreak = 7;
        kid2.LastActivityDate = DateTime.UtcNow;

        var parentPetrov = UserService.CreateParent(
            "petrov@kidbank.by", hashPassword("Parent123!"),
            "Дмитрий", "Петров", new DateTime(1982, 11, 5, 0, 0, 0, DateTimeKind.Utc),
            family2.Id, computeEmailHash("petrov@kidbank.by"));

        var kidPetrov = UserService.CreateKid(
            "misha@kidbank.by", hashPassword("Kid12345!"),
            "Михаил", "Петров", new DateTime(2013, 1, 20, 0, 0, 0, DateTimeKind.Utc),
            family2.Id, computeEmailHash("misha@kidbank.by"));
        kidPetrov.TotalXp = 450;
        kidPetrov.CurrentStreak = 3;

        context.Users.AddRange(parent, parent2, kid1, kid2, parentPetrov, kidPetrov);

        var parentAcc = AccountService.CreateMain(parent.Id);
        parentAcc.Balance = 5000m;
        var parentSavings = AccountService.CreateSavings(parent.Id, "Накопления на отпуск");
        parentSavings.Balance = 2500m;
        var parent2Acc = AccountService.CreateMain(parent2.Id);
        parent2Acc.Balance = 3000m;

        var kid1Acc = AccountService.CreateMain(kid1.Id);
        kid1Acc.Balance = 350m;
        var kid1Savings = AccountService.CreateSavings(kid1.Id, "На велосипед");
        kid1Savings.Balance = 120m;

        var kid2Acc = AccountService.CreateMain(kid2.Id);
        kid2Acc.Balance = 180m;

        var parentPetrovAcc = AccountService.CreateMain(parentPetrov.Id);
        parentPetrovAcc.Balance = 4000m;
        var kidPetrovAcc = AccountService.CreateMain(kidPetrov.Id);
        kidPetrovAcc.Balance = 200m;

        context.Accounts.AddRange(parentAcc, parentSavings, parent2Acc, kid1Acc, kid1Savings, kid2Acc, parentPetrovAcc, kidPetrovAcc);

        var tx1 = TransactionService.CreateDeposit(parentAcc.Id, 5000m, "BYN", "Пополнение счёта");
        var tx2 = TransactionService.CreateTransfer(parentAcc.Id, kid1Acc.Id, 200m, "BYN", "Карманные деньги за неделю");
        var tx3 = TransactionService.CreateTransfer(parentAcc.Id, kid2Acc.Id, 100m, "BYN", "Карманные деньги");
        var tx4 = TransactionService.CreateDeposit(parent2Acc.Id, 3000m, "BYN", "Зарплата");
        var tx5 = TransactionService.CreateTransfer(parent2Acc.Id, kid1Acc.Id, 50m, "BYN", "За хорошие оценки");
        var tx6 = TransactionService.CreateGoalDeposit(kid1Acc.Id, 80m, "BYN", Guid.NewGuid());
        var tx7 = TransactionService.CreateDeposit(parentPetrovAcc.Id, 4000m, "BYN", "Пополнение");
        var tx8 = TransactionService.CreateTransfer(parentPetrovAcc.Id, kidPetrovAcc.Id, 150m, "BYN", "На обед");
        context.Transactions.AddRange(tx1, tx2, tx3, tx4, tx5, tx6, tx7, tx8);

        var goal1 = GoalService.Create(kid1.Id, "Игровая приставка", 1200m, "BYN", "PlayStation 5", targetDate: DateTime.UtcNow.AddMonths(3));
        goal1.CurrentAmount = 350m;
        var goal2 = GoalService.Create(kid1.Id, "Наушники AirPods", 400m, "BYN", "Беспроводные наушники", targetDate: DateTime.UtcNow.AddMonths(1));
        goal2.CurrentAmount = 400m;
        goal2.Status = GoalStatus.Completed;
        var goal3 = GoalService.Create(kid2.Id, "Кукла Barbie", 150m, "BYN", targetDate: DateTime.UtcNow.AddDays(30));
        goal3.CurrentAmount = 60m;
        var goal4 = GoalService.Create(kid2.Id, "Книга о Гарри Поттере", 50m, "BYN", targetDate: DateTime.UtcNow.AddDays(14));
        goal4.CurrentAmount = 50m;
        goal4.Status = GoalStatus.Completed;
        var goal5 = GoalService.Create(kidPetrov.Id, "Футбольный мяч", 80m, "BYN");
        goal5.CurrentAmount = 30m;
        context.WishlistGoals.AddRange(goal1, goal2, goal3, goal4, goal5);

        var task1 = TaskService.Create(kid1.Id, parent.Id, "Убрать комнату", 10m, "BYN", "Пропылесосить и протереть пыль", DateTime.UtcNow.AddDays(1));
        var task2 = TaskService.Create(kid1.Id, parent.Id, "Помыть посуду", 5m, "BYN", dueDate: DateTime.UtcNow.AddDays(1));
        task2.Status = TaskAssignmentStatus.Completed;
        task2.CompletedAt = DateTime.UtcNow.AddHours(-2);
        var task3 = TaskService.Create(kid1.Id, parent.Id, "Сделать уроки", 15m, "BYN", "Математика и русский язык");
        task3.Status = TaskAssignmentStatus.Approved;
        task3.CompletedAt = DateTime.UtcNow.AddDays(-1);
        var task4 = TaskService.Create(kid2.Id, parent.Id, "Полить цветы", 3m, "BYN");
        var task5 = TaskService.Create(kid2.Id, parent2.Id, "Прочитать книгу", 20m, "BYN", "Прочитать 30 страниц");
        task5.Status = TaskAssignmentStatus.Completed;
        task5.CompletedAt = DateTime.UtcNow.AddHours(-5);
        var task6 = TaskService.Create(kid1.Id, parent.Id, "Вынести мусор", 5m, "BYN");
        task6.Status = TaskAssignmentStatus.Rejected;
        var task7 = TaskService.Create(kidPetrov.Id, parentPetrov.Id, "Помочь с уборкой", 15m, "BYN");
        context.TaskAssignments.AddRange(task1, task2, task3, task4, task5, task6, task7);

        var tpl1 = TaskTemplateService.Create(parent.Id, "Уборка комнаты", 10m, "BYN", "Пропылесосить и протереть пыль");
        var tpl2 = TaskTemplateService.Create(parent.Id, "Учёба 1 час", 15m, "BYN", "Подготовиться к контрольной");
        var tpl3 = TaskTemplateService.Create(parent.Id, "Помыть посуду", 5m, "BYN");
        var tpl4 = TaskTemplateService.Create(parent.Id, "Выгулять собаку", 8m, "BYN");
        var tpl5 = TaskTemplateService.Create(parentPetrov.Id, "Прибраться в комнате", 12m, "BYN");
        context.TaskTemplates.AddRange(tpl1, tpl2, tpl3, tpl4, tpl5);

        var req1 = MoneyRequestService.Create(kid1.Id, parent.Id, 100m, "BYN", "На подарок другу");
        var req2 = MoneyRequestService.Create(kid1.Id, parent.Id, 50m, "BYN", "На мороженое");
        req2.Status = MoneyRequestStatus.Approved;
        req2.ResponseNote = "Хорошо, заслужил!";
        req2.RespondedAt = DateTime.UtcNow.AddDays(-2);
        var req3 = MoneyRequestService.Create(kid2.Id, parent.Id, 30m, "BYN", "На книжку");
        req3.Status = MoneyRequestStatus.Approved;
        req3.ResponseNote = "Молодец, что читаешь!";
        req3.RespondedAt = DateTime.UtcNow.AddDays(-1);
        var req4 = MoneyRequestService.Create(kid2.Id, parent.Id, 200m, "BYN", "На планшет");
        req4.Status = MoneyRequestStatus.Rejected;
        req4.ResponseNote = "Слишком дорого, давай копить";
        req4.RespondedAt = DateTime.UtcNow.AddDays(-3);
        var req5 = MoneyRequestService.Create(kidPetrov.Id, parentPetrov.Id, 50m, "BYN", "На обед в школе");
        context.MoneyRequests.AddRange(req1, req2, req3, req4, req5);

        var card1 = CardService.Create(kid1Acc.Id, "MAXIM IVANOV");
        var card2 = CardService.Create(kid2Acc.Id, "ALISA IVANOVA");
        var card3 = CardService.Create(kidPetrovAcc.Id, "MIKHAIL PETROV");
        context.VirtualCards.AddRange(card1, card2, card3);

        var cat1 = SpendingCategoryService.CreateSystem(family.Id, "Еда", "restaurant", "#4CAF50");
        var cat2 = SpendingCategoryService.CreateSystem(family.Id, "Транспорт", "directions_bus", "#2196F3");
        var cat3 = SpendingCategoryService.CreateSystem(family.Id, "Развлечения", "sports_esports", "#FF9800");
        var cat4 = SpendingCategoryService.CreateSystem(family.Id, "Одежда", "checkroom", "#9C27B0");
        var cat5 = SpendingCategoryService.CreateSystem(family.Id, "Образование", "school", "#00BCD4");
        var cat6 = SpendingCategoryService.Create(family.Id, "Подарки", true, "card_giftcard", "#E91E63");
        var cat7 = SpendingCategoryService.CreateSystem(family2.Id, "Еда", "restaurant", "#4CAF50");
        var cat8 = SpendingCategoryService.CreateSystem(family2.Id, "Развлечения", "sports_esports", "#FF9800");
        context.SpendingCategories.AddRange(cat1, cat2, cat3, cat4, cat5, cat6, cat7, cat8);

        var limit1 = SpendingLimitHelper.Create(kid1.Id, parent.Id, 100m, SpendingLimitPeriod.Daily);
        var limit2 = SpendingLimitHelper.Create(kid1.Id, parent.Id, 500m, SpendingLimitPeriod.Weekly);
        var limit3 = SpendingLimitHelper.Create(kid2.Id, parent.Id, 50m, SpendingLimitPeriod.Daily);
        var limit4 = SpendingLimitHelper.Create(kid2.Id, parent.Id, 300m, SpendingLimitPeriod.Monthly);
        var limit5 = SpendingLimitHelper.Create(kidPetrov.Id, parentPetrov.Id, 80m, SpendingLimitPeriod.Daily);
        context.SpendingLimits.AddRange(limit1, limit2, limit3, limit4, limit5);

        var allow1 = AllowanceService.Create(parent.Id, kid1.Id, 50m, "BYN", "Weekly", 1);
        var allow2 = AllowanceService.Create(parent.Id, kid2.Id, 30m, "BYN", "Weekly", 1);
        var allow3 = AllowanceService.Create(parent2.Id, kid1.Id, 25m, "BYN", "Monthly", dayOfMonth: 1);
        var allow4 = AllowanceService.Create(parentPetrov.Id, kidPetrov.Id, 40m, "BYN", "Weekly", 5);
        context.RecurringAllowances.AddRange(allow1, allow2, allow3, allow4);

        var msg1 = ChatMessageService.CreateFamilyMessage(family.Id, parent.Id, "Всем привет! Как дела в школе?");
        msg1.CreatedAt = DateTime.UtcNow.AddHours(-5);
        var msg2 = ChatMessageService.CreateFamilyMessage(family.Id, kid1.Id, "Привет, пап! Всё хорошо, получил 9 по математике!");
        msg2.CreatedAt = DateTime.UtcNow.AddHours(-4);
        var msg3 = ChatMessageService.CreateFamilyMessage(family.Id, kid2.Id, "А я нарисовала рисунок! 🎨");
        msg3.CreatedAt = DateTime.UtcNow.AddHours(-3);
        var msg4 = ChatMessageService.CreateFamilyMessage(family.Id, parent2.Id, "Молодцы! Алиса, покажешь рисунок?");
        msg4.CreatedAt = DateTime.UtcNow.AddHours(-2);
        var msg5 = ChatMessageService.CreateFamilyMessage(family.Id, kid1.Id, "Мам, можно пойти к Пете после школы?");
        msg5.CreatedAt = DateTime.UtcNow.AddHours(-1);
        var msg6 = ChatMessageService.CreateFamilyMessage(family.Id, parent.Id, "Да, но вернись к 18:00");
        var msg7 = ChatMessageService.CreateFamilyMessage(family2.Id, parentPetrov.Id, "Миша, не забудь про уроки!");
        msg7.CreatedAt = DateTime.UtcNow.AddHours(-2);
        var msg8 = ChatMessageService.CreateFamilyMessage(family2.Id, kidPetrov.Id, "Хорошо, папа! 📚");
        msg8.CreatedAt = DateTime.UtcNow.AddHours(-1);
        context.ChatMessages.AddRange(msg1, msg2, msg3, msg4, msg5, msg6, msg7, msg8);

        var n1 = NotificationService.Create(kid1.Id, "Task", "Новое задание!", "Родитель назначил задание: Убрать комнату");
        var n2 = NotificationService.Create(kid1.Id, "Achievement", "Новое достижение!", "Ты получил «Первое задание»");
        var n3 = NotificationService.Create(parent.Id, "MoneyRequest", "Запрос денег", "Максим просит 100 BYN на подарок другу");
        var n4 = NotificationService.Create(kid2.Id, "Transfer", "Пополнение!", "Мама перевела 100 BYN");
        var n5 = NotificationService.Create(kid1.Id, "Task", "Задание одобрено!", "Задание «Сделать уроки» одобрено, +15 BYN");
        n5.IsRead = true;
        var n6 = NotificationService.Create(parent.Id, "Task", "Задание выполнено", "Алиса отметила задание «Прочитать книгу» как выполненное");
        var n7 = NotificationService.Create(kidPetrov.Id, "Task", "Новое задание!", "Папа назначил задание: Помочь с уборкой");
        context.Notifications.AddRange(n1, n2, n3, n4, n5, n6, n7);

        var block1 = CategoryBlockService.Create(kid2.Id, cat3.Id, parent.Id);
        context.CategoryBlocks.Add(block1);

        await context.SaveChangesAsync();

        await SeedProgressDataAsync(context, kid1.Id, kid2.Id, kidPetrov.Id);
    }

    private static async Task SeedProgressDataAsync(ApplicationDbContext context, Guid kid1Id, Guid kid2Id, Guid kidPetrovId)
    {
        var defs = await context.AchievementDefinitions.ToListAsync();
        var modules = await context.EducationModules.ToListAsync();

        if (defs.Count > 0)
        {
            var byCode = defs.ToDictionary(d => d.Code);

            void AddAP(Guid userId, string code, bool unlocked, int progress = 0)
            {
                if (!byCode.TryGetValue(code, out var def)) return;
                context.AchievementProgresses.Add(new AchievementProgress
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    AchievementDefinitionId = def.Id,
                    IsUnlocked = unlocked,
                    CurrentProgress = unlocked ? 1 : progress,
                    UnlockedAt = unlocked ? DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 30)) : null,
                    CreatedAt = DateTime.UtcNow
                });
            }

            AddAP(kid1Id, "FIRST_SAVINGS", true);
            AddAP(kid1Id, "FIRST_GOAL", true);
            AddAP(kid1Id, "GOAL_REACHED", true);
            AddAP(kid1Id, "TASK_FIRST", true);
            AddAP(kid1Id, "TASK_10", false, 3);
            AddAP(kid1Id, "QUIZ_FIRST", true);
            AddAP(kid1Id, "MODULE_FIRST", true);
            AddAP(kid1Id, "CHAT_FIRST", true);
            AddAP(kid1Id, "MONEY_REQUEST_FIRST", true);
            AddAP(kid1Id, "CARD_FIRST", true);
            AddAP(kid1Id, "STREAK_7", true);
            AddAP(kid1Id, "XP_1000", true);

            AddAP(kid2Id, "FIRST_GOAL", true);
            AddAP(kid2Id, "GOAL_REACHED", true);
            AddAP(kid2Id, "TASK_FIRST", true);
            AddAP(kid2Id, "CHAT_FIRST", true);
            AddAP(kid2Id, "QUIZ_FIRST", true);

            AddAP(kidPetrovId, "TASK_FIRST", false, 0);
            AddAP(kidPetrovId, "CHAT_FIRST", true);
        }

        if (modules.Count > 0)
        {
            var completedCount = Math.Min(5, modules.Count);
            for (var i = 0; i < completedCount; i++)
            {
                context.EducationProgresses.Add(new EducationProgress
                {
                    Id = Guid.NewGuid(),
                    UserId = kid1Id,
                    ModuleId = modules[i].Id,
                    IsCompleted = true,
                    QuizzesCompleted = 2,
                    QuizzesTotal = 2,
                    TotalXpEarned = modules[i].XpReward,
                    StartedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(5, 20)),
                    CompletedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 5))
                });
            }

            var kid2Count = Math.Min(2, modules.Count);
            for (var i = 0; i < kid2Count; i++)
            {
                context.EducationProgresses.Add(new EducationProgress
                {
                    Id = Guid.NewGuid(),
                    UserId = kid2Id,
                    ModuleId = modules[i].Id,
                    IsCompleted = true,
                    QuizzesCompleted = 2,
                    QuizzesTotal = 3,
                    TotalXpEarned = modules[i].XpReward,
                    StartedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(3, 10)),
                    CompletedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 3))
                });
            }
        }

        await context.SaveChangesAsync();
    }

    private static void SeedAchievements(ApplicationDbContext context)
    {
        if (context.AchievementDefinitions.Any()) return;

        var achievements = new[]
        {
            Create("first_savings", "Первый вклад", "Положи деньги на накопительный счёт", "savings", 50, "{\"type\":\"savings_deposit\",\"count\":1}"),
            Create("saver_10", "Копилка 10", "Сделай 10 пополнений целей", "savings", 100, "{\"type\":\"goal_deposit\",\"count\":10}"),
            Create("saver_50", "Мастер накоплений", "Сделай 50 пополнений целей", "savings", 300, "{\"type\":\"goal_deposit\",\"count\":50}"),
            Create("first_goal", "Первая мечта", "Создай свою первую цель", "goals", 50, "{\"type\":\"goal_create\",\"count\":1}"),
            Create("goal_reached", "Мечта сбылась!", "Накопи на свою первую цель", "goals", 200, "{\"type\":\"goal_complete\",\"count\":1}"),
            Create("goals_3", "Целеустремлённый", "Достигни 3 цели", "goals", 500, "{\"type\":\"goal_complete\",\"count\":3}"),
            Create("task_first", "Первое задание", "Выполни своё первое задание", "tasks", 50, "{\"type\":\"task_complete\",\"count\":1}"),
            Create("task_10", "Трудяга", "Выполни 10 заданий", "tasks", 150, "{\"type\":\"task_complete\",\"count\":10}"),
            Create("task_50", "Мастер на все руки", "Выполни 50 заданий", "tasks", 500, "{\"type\":\"task_complete\",\"count\":50}"),
            Create("task_100", "Легенда труда", "Выполни 100 заданий", "tasks", 1000, "{\"type\":\"task_complete\",\"count\":100}"),
            Create("quiz_first", "Первый квиз", "Пройди свой первый квиз", "education", 30, "{\"type\":\"quiz_complete\",\"count\":1}"),
            Create("quiz_10", "Знайка", "Пройди 10 квизов", "education", 100, "{\"type\":\"quiz_complete\",\"count\":10}"),
            Create("quiz_perfect", "Отличник", "Ответь правильно на 10 квизов подряд", "education", 200, "{\"type\":\"quiz_perfect_streak\",\"count\":10}"),
            Create("module_first", "Первый урок", "Заверши свой первый обучающий модуль", "education", 50, "{\"type\":\"module_complete\",\"count\":1}"),
            Create("module_10", "Вечный студент", "Заверши 10 модулей", "education", 300, "{\"type\":\"module_complete\",\"count\":10}"),
            Create("mission_first", "Первая миссия", "Заверши свою первую миссию", "education", 150, "{\"type\":\"mission_complete\",\"count\":1}"),
            Create("mission_all", "Выпускник", "Заверши все миссии", "education", 1000, "{\"type\":\"mission_complete_all\",\"count\":1}"),
            Create("streak_7", "Неделя подряд", "Заходи 7 дней подряд", "engagement", 100, "{\"type\":\"streak\",\"count\":7}"),
            Create("streak_30", "Месяц стабильности", "Заходи 30 дней подряд", "engagement", 500, "{\"type\":\"streak\",\"count\":30}"),
            Create("xp_1000", "Тысячник", "Набери 1000 XP", "engagement", 200, "{\"type\":\"total_xp\",\"count\":1000}"),
            Create("xp_5000", "Мастер опыта", "Набери 5000 XP", "engagement", 500, "{\"type\":\"total_xp\",\"count\":5000}"),
            Create("chat_first", "Первое сообщение", "Отправь сообщение в семейный чат", "social", 20, "{\"type\":\"chat_send\",\"count\":1}"),
            Create("money_request_first", "Первый запрос", "Попроси деньги у родителя", "finance", 30, "{\"type\":\"money_request\",\"count\":1}"),
            Create("card_first", "Моя карта", "Создай свою первую виртуальную карту", "finance", 50, "{\"type\":\"card_create\",\"count\":1}"),
        };

        foreach (var a in achievements)
            context.AchievementDefinitions.Add(a);

        static AchievementDefinition Create(string code, string title, string desc, string cat, int xp, string req)
        {
            return AchievementDefinitionService.Create(code, title, desc, cat, xp, requirementJson: req);
        }
    }

    private static void SeedMissionsAndModules(ApplicationDbContext context)
    {
        var mission1 = EducationMissionService.Create(
            "Что такое деньги?", "Знакомство с понятием денег, их историей и функциями", 1, 200, 6, 18);
        var mission2 = EducationMissionService.Create(
            "Копим правильно", "Учимся откладывать деньги и ставить финансовые цели", 2, 250, 6, 18);
        var mission3 = EducationMissionService.Create(
            "Умные траты", "Как тратить деньги с умом и не покупать лишнего", 3, 300, 8, 18);
        var mission4 = EducationMissionService.Create(
            "Семейный бюджет", "Понимаем, откуда берутся деньги в семье и куда уходят", 4, 300, 10, 18);
        var mission5 = EducationMissionService.Create(
            "Безопасность денег", "Как защитить свои деньги и не попасться мошенникам", 5, 350, 8, 18);

        context.EducationMissions.AddRange(mission1, mission2, mission3, mission4, mission5);

        AddModulesForMission1(context, mission1.Id);
        AddModulesForMission2(context, mission2.Id);
        AddModulesForMission3(context, mission3.Id);
        AddModulesForMission4(context, mission4.Id);
        AddModulesForMission5(context, mission5.Id);
    }

    private static void AddModulesForMission1(ApplicationDbContext ctx, Guid missionId)
    {
        var m1 = AddModule(ctx, missionId, "История денег", "Узнай, как люди обменивались товарами до появления денег и как появились первые монеты и купюры.",
            "Давным-давно люди не использовали деньги. Они менялись: рыбак давал рыбу, а фермер — зерно. Это называется бартер. Но что делать, если рыбаку не нужно зерно? Так появились деньги — сначала ракушки, потом металлические монеты, а затем бумажные купюры. Сегодня деньги бывают даже электронными — на карточке или в телефоне!",
            1, 30);
        AddQuiz(ctx, m1, "Как назывался обмен товарами без денег?", "[\"Бартер\",\"Кредит\",\"Инвестиция\",\"Налог\"]", 0, "Бартер — это обмен одного товара на другой без использования денег.", 1, 15);
        AddQuiz(ctx, m1, "Что использовали в качестве первых денег?", "[\"Ракушки\",\"Смартфоны\",\"Бумагу\",\"Пластик\"]", 0, "Ракушки каури были одними из первых денег в истории.", 2, 15);
        AddQuiz(ctx, m1, "Какие деньги бывают сегодня?", "[\"Только монеты\",\"Только купюры\",\"Монеты, купюры и электронные\",\"Только электронные\"]", 2, "Сегодня мы используем монеты, купюры и электронные деньги.", 3, 15);

        var m2 = AddModule(ctx, missionId, "Зачем нужны деньги?", "Разберёмся, какие функции выполняют деньги в нашей жизни.",
            "Деньги выполняют три главные функции: 1) Средство обмена — ты платишь деньгами за товары и услуги. 2) Мера стоимости — деньги помогают сравнивать цены. Яблоко стоит 2 рубля, а велосипед — 500. 3) Средство накопления — деньги можно откладывать на будущее. Именно благодаря деньгам мы можем покупать то, что нам нужно, когда нам нужно!",
            2, 30);
        AddQuiz(ctx, m2, "Сколько главных функций у денег?", "[\"Одна\",\"Две\",\"Три\",\"Четыре\"]", 2, "У денег три главные функции: средство обмена, мера стоимости и средство накопления.", 1, 15);
        AddQuiz(ctx, m2, "Что значит «мера стоимости»?", "[\"Деньги можно копить\",\"Деньги помогают сравнивать цены\",\"Деньги можно обменять\",\"Деньги красивые\"]", 1, "Мера стоимости позволяет нам сравнивать стоимость разных товаров.", 2, 15);

        var m3 = AddModule(ctx, missionId, "Виды денег", "Монеты, купюры, электронные деньги — в чём разница?",
            "Деньги бывают разными: наличные (монеты и купюры) — их можно потрогать и положить в кошелёк. Безналичные — деньги на банковской карте или в мобильном приложении. Криптовалюты — цифровые деньги вроде биткоина. В повседневной жизни мы чаще всего используем наличные и безналичные деньги. Банковская карта — это как электронный кошелёк!",
            3, 30);
        AddQuiz(ctx, m3, "Какие деньги нельзя потрогать?", "[\"Монеты\",\"Купюры\",\"Безналичные\",\"Все можно потрогать\"]", 2, "Безналичные деньги существуют только в электронном виде.", 1, 15);
        AddQuiz(ctx, m3, "Что такое банковская карта?", "[\"Игрушка\",\"Электронный кошелёк\",\"Бумажные деньги\",\"Документ\"]", 1, "Банковская карта — это электронный кошелёк, связанный с вашим счётом в банке.", 2, 15);
    }

    private static void AddModulesForMission2(ApplicationDbContext ctx, Guid missionId)
    {
        var m1 = AddModule(ctx, missionId, "Зачем копить?", "Почему важно откладывать деньги и как это поможет в будущем.",
            "Копить — значит откладывать часть денег на будущее. Зачем? Чтобы купить что-то большое (велосипед, игровую приставку), чтобы иметь запас на непредвиденные расходы, чтобы чувствовать себя уверенно. Правило: старайся откладывать хотя бы 10% от любых полученных денег. Если тебе дали 100 рублей, отложи 10 — и ты удивишься, как быстро накопится!",
            1, 35);
        AddQuiz(ctx, m1, "Какой процент от дохода рекомендуется откладывать?", "[\"1%\",\"5%\",\"10%\",\"50%\"]", 2, "Рекомендуется откладывать минимум 10% от любого дохода.", 1, 15);
        AddQuiz(ctx, m1, "Зачем нужна «подушка безопасности»?", "[\"Для сна\",\"На непредвиденные расходы\",\"Для красоты\",\"Для игр\"]", 1, "Подушка безопасности — это деньги на случай непредвиденных расходов.", 2, 15);

        var m2 = AddModule(ctx, missionId, "Ставим финансовую цель", "Как правильно поставить цель и составить план накоплений.",
            "Чтобы копить эффективно, нужна конкретная цель. Вместо 'хочу много денег' лучше: 'хочу накопить 500 рублей на наушники за 2 месяца'. Формула: Сумма цели / Количество недель = Сколько откладывать в неделю. Пример: 500 / 8 недель = 63 рубля в неделю. Это вполне реально! Записывай свой прогресс — так мотивация не пропадёт.",
            2, 35);
        AddQuiz(ctx, m2, "Какая цель лучше?", "[\"Хочу много денег\",\"Хочу накопить 500 руб. на наушники за 2 месяца\",\"Хочу быть богатым\",\"Хочу всё купить\"]", 1, "Конкретная цель с суммой и сроком помогает составить план.", 1, 15);
        AddQuiz(ctx, m2, "Если цель 800 руб. за 4 недели, сколько откладывать в неделю?", "[\"100 руб.\",\"200 руб.\",\"400 руб.\",\"800 руб.\"]", 1, "800 / 4 = 200 рублей в неделю.", 2, 15);

        var m3 = AddModule(ctx, missionId, "Копилка и счёт", "Куда лучше складывать деньги — в копилку или на счёт?",
            "Копилка дома — хороший старт для маленьких сумм. Но деньги там не растут. Банковский счёт (или счёт в приложении) безопаснее: деньги не потеряются. Накопительный счёт — деньги лежат и ещё приносят проценты! В нашем приложении ты можешь создать цель и копить прямо на неё, видя прогресс каждый день.",
            3, 35);
        AddQuiz(ctx, m3, "Где деньги в большей безопасности?", "[\"Под подушкой\",\"В копилке\",\"На банковском счёте\",\"В кармане\"]", 2, "Банковский счёт — самое безопасное место для хранения денег.", 1, 15);
        AddQuiz(ctx, m3, "Что такое накопительный счёт?", "[\"Счёт, где деньги уменьшаются\",\"Счёт с процентами на остаток\",\"Счёт для оплаты покупок\",\"Счёт для игр\"]", 1, "Накопительный счёт начисляет проценты на ваши деньги.", 2, 15);
    }

    private static void AddModulesForMission3(ApplicationDbContext ctx, Guid missionId)
    {
        var m1 = AddModule(ctx, missionId, "Нужды и желания", "Учимся отличать то, что нам нужно, от того, что хочется.",
            "Нужды — это то, без чего нельзя обойтись: еда, одежда, жильё, лекарства. Желания — то, что приятно иметь, но можно обойтись: новая игрушка, сладости, модные кроссовки. Прежде чем купить что-то, спроси себя: 'Мне это НУЖНО или я просто ХОЧУ?' Если хочешь — подожди 24 часа. Если через день всё ещё хочется — может, стоит купить.",
            1, 40);
        AddQuiz(ctx, m1, "Что из этого — нужда?", "[\"Новая игрушка\",\"Еда\",\"Модные кроссовки\",\"Видеоигра\"]", 1, "Еда — базовая потребность, без которой нельзя обойтись.", 1, 15);
        AddQuiz(ctx, m1, "Что делать, если хочется купить что-то прямо сейчас?", "[\"Сразу купить\",\"Подождать 24 часа\",\"Попросить у друга\",\"Забыть про это\"]", 1, "Правило 24 часов помогает избежать импульсивных покупок.", 2, 15);

        var m2 = AddModule(ctx, missionId, "Сравниваем цены", "Как найти лучшую цену и не переплатить.",
            "Один и тот же товар может стоить по-разному в разных магазинах. Сравни цены перед покупкой! Лайфхаки: посмотри цену за единицу (за 1 кг, за 1 штуку), проверь наличие скидок и акций, не ведись на красивую упаковку — внутри может быть то же самое, что дешевле. Экономить — не значит быть жадным. Это значит быть умным покупателем!",
            2, 40);
        AddQuiz(ctx, m2, "Как правильно сравнивать цены?", "[\"По красоте упаковки\",\"По цене за единицу\",\"По размеру магазина\",\"По рекламе\"]", 1, "Цена за единицу (кг, шт) помогает объективно сравнить товары.", 1, 15);

        var m3 = AddModule(ctx, missionId, "Импульсивные покупки", "Почему мы покупаем ненужное и как этого избежать.",
            "Импульсивная покупка — когда ты покупаешь что-то спонтанно, без планирования. Магазины специально расставляют товары так, чтобы ты захотел их купить. Чипсы у кассы, яркие витрины — всё это ловушки! Как бороться: составляй список покупок заранее, бери с собой только нужную сумму, спроси себя 'буду ли я этим пользоваться через неделю?'",
            3, 40);
        AddQuiz(ctx, m3, "Что такое импульсивная покупка?", "[\"Покупка по плану\",\"Покупка со скидкой\",\"Покупка спонтанно, без планирования\",\"Покупка в интернете\"]", 2, "Импульсивная покупка — спонтанная, незапланированная покупка.", 1, 15);
        AddQuiz(ctx, m3, "Как избежать импульсивных покупок?", "[\"Не ходить в магазин\",\"Составлять список покупок\",\"Покупать всё подряд\",\"Брать побольше денег\"]", 1, "Список покупок помогает не отвлекаться на ненужное.", 2, 15);
    }

    private static void AddModulesForMission4(ApplicationDbContext ctx, Guid missionId)
    {
        var m1 = AddModule(ctx, missionId, "Откуда берутся деньги?", "Узнаём, как зарабатывают деньги и какие бывают источники дохода.",
            "Деньги не берутся из ниоткуда. Родители получают зарплату за работу. Бывают и другие доходы: подработка, продажа вещей, проценты от сбережений. В семье все доходы складываются в общий бюджет. Даже ты можешь зарабатывать — выполняя задания и получая награды! Главное — понимать, что деньги нужно заработать, прежде чем потратить.",
            1, 35);
        AddQuiz(ctx, m1, "Что такое зарплата?", "[\"Деньги в копилке\",\"Оплата за работу\",\"Подарок от банка\",\"Деньги из банкомата\"]", 1, "Зарплата — это деньги, которые получают за работу.", 1, 15);

        var m2 = AddModule(ctx, missionId, "Куда уходят деньги?", "Разбираемся в расходах семьи.",
            "Расходы бывают обязательные и необязательные. Обязательные: жильё, еда, транспорт, образование, здоровье. Необязательные: развлечения, игрушки, кафе, подписки. Если записывать все расходы, можно увидеть, куда утекают деньги, и найти, где можно сэкономить. Попробуй записывать свои расходы хотя бы неделю!",
            2, 35);
        AddQuiz(ctx, m2, "Какой расход обязательный?", "[\"Новая игрушка\",\"Поход в кино\",\"Оплата за жильё\",\"Подписка на сервис\"]", 2, "Оплата за жильё — обязательный расход, без которого не обойтись.", 1, 15);

        var m3 = AddModule(ctx, missionId, "Составляем бюджет", "Простой план: доходы минус расходы.",
            "Бюджет — это план доходов и расходов. Формула простая: Доходы - Расходы = Остаток. Если остаток положительный — ты молодец, можешь сберегать! Если отрицательный — нужно сократить расходы. Попробуй правило 50/30/20: 50% — на нужды, 30% — на желания, 20% — на сбережения.",
            3, 35);
        AddQuiz(ctx, m3, "Что такое бюджет?", "[\"Много денег\",\"План доходов и расходов\",\"Банковский счёт\",\"Зарплата\"]", 1, "Бюджет — это план, сколько денег приходит и уходит.", 1, 15);
        AddQuiz(ctx, m3, "Сколько процентов рекомендуется сберегать по правилу 50/30/20?", "[\"10%\",\"20%\",\"30%\",\"50%\"]", 1, "По правилу 50/30/20 на сбережения идёт 20% дохода.", 2, 15);
    }

    private static void AddModulesForMission5(ApplicationDbContext ctx, Guid missionId)
    {
        var m1 = AddModule(ctx, missionId, "Пароли и PIN-коды", "Как защитить свои деньги с помощью надёжных паролей.",
            "PIN-код карты — это секрет! Никому не говори свой PIN, даже друзьям. Надёжный пароль: минимум 8 символов, буквы + цифры + спецсимволы. Плохой пароль: 123456, qwerty, дата рождения. Хороший пароль: K1dB@nk_2024! Не используй один пароль для всего.",
            1, 40);
        AddQuiz(ctx, m1, "Кому можно сказать PIN-код карты?", "[\"Лучшему другу\",\"Учителю\",\"Никому\",\"Продавцу\"]", 2, "PIN-код — это секрет, который нельзя сообщать никому.", 1, 15);
        AddQuiz(ctx, m1, "Какой пароль надёжный?", "[\"123456\",\"qwerty\",\"K1dB@nk_2024!\",\"password\"]", 2, "Надёжный пароль содержит буквы, цифры и спецсимволы.", 2, 15);

        var m2 = AddModule(ctx, missionId, "Мошенники в интернете", "Как распознать мошенников и не попасться на уловки.",
            "Мошенники могут: прислать сообщение 'Вы выиграли приз!', попросить перевести деньги за 'бесплатный' подарок, создать поддельный сайт банка. Правила безопасности: не переходи по подозрительным ссылкам, не вводи данные карты на незнакомых сайтах, если что-то кажется подозрительным — расскажи родителям.",
            2, 40);
        AddQuiz(ctx, m2, "Что делать, если пришло сообщение 'Вы выиграли миллион'?", "[\"Перейти по ссылке\",\"Отправить данные карты\",\"Рассказать родителям и не переходить\",\"Переслать друзьям\"]", 2, "Подозрительные сообщения — это мошенничество. Расскажи родителям!", 1, 15);

        var m3 = AddModule(ctx, missionId, "Безопасные покупки онлайн", "Как безопасно покупать в интернете.",
            "Покупки онлайн — удобно, но есть правила: покупай только на известных сайтах, проверяй адрес сайта (https:// и значок замка), не сохраняй данные карты в браузере, используй виртуальную карту для интернет-покупок.",
            3, 40);
        AddQuiz(ctx, m3, "Что означает значок замка в адресной строке?", "[\"Сайт заблокирован\",\"Соединение защищено\",\"Сайт медленный\",\"Это игра\"]", 1, "Замок означает, что соединение зашифровано и безопасно.", 1, 15);
        AddQuiz(ctx, m3, "Какую карту лучше использовать для покупок в интернете?", "[\"Основную карту родителей\",\"Виртуальную карту\",\"Любую найденную\",\"Карту друга\"]", 1, "Виртуальная карта безопаснее для интернет-покупок.", 2, 15);
    }

    private static EducationModule AddModule(ApplicationDbContext ctx, Guid missionId,
        string title, string description, string content, int order, int xp)
    {
        var module = EducationModuleService.Create(title, content, order, xp, description: description);
        module.MissionId = missionId;
        module.IsPublished = true;
        ctx.EducationModules.Add(module);
        return module;
    }

    private static void AddQuiz(ApplicationDbContext ctx, EducationModule module,
        string question, string optionsJson, int correctIndex, string explanation, int order, int xp)
    {
        var options = System.Text.Json.JsonSerializer.Deserialize<List<string>>(optionsJson)!;
        var quiz = QuizService.Create(module.Id, question, options, correctIndex, xp, order, explanation);
        ctx.Quizzes.Add(quiz);
    }
}
