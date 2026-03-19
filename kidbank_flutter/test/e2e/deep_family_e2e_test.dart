@Tags(['e2e'])
import 'package:flutter_test/flutter_test.dart';
import 'package:kidbank_flutter/core/api_service.dart';
import '../helpers/e2e_helpers.dart';

/// Глубокие интеграционные тесты: проверка взаимодействия внутри одной семьи.
/// Все операции — parent + kid1 + kid2 из одной семьи.
/// Тесты проверяют сквозные сценарии: задание → оплата → баланс → уровень → лидерборд.
void main() {
  late ApiService parentApi;
  late ApiService kid1Api;
  late ApiService kid2Api;
  late String parentId;
  late String kid1Id;
  late String kid2Id;
  late String kid1MainAccId;

  setUpAll(() async {
    parentApi = await loginAsParent();
    kid1Api = await loginAsKid1();
    kid2Api = await loginAsKid2();

    final parentMe = await parentApi.get('users/me');
    parentId = parentMe['id'];
    final kid1Me = await kid1Api.get('users/me');
    kid1Id = kid1Me['id'];
    final kid2Me = await kid2Api.get('users/me');
    kid2Id = kid2Me['id'];

    final kid1Accs = await kid1Api.get('accounts/my') as List;
    kid1MainAccId = (kid1Accs.firstWhere((a) => a['type'] == 'Main'))['id'];
  });

  group('DF1. Семья — участники и дашборд', () {
    test('DF1.1 Родитель видит обоих детей', () async {
      final kids = await parentApi.get('families/kids') as List;
      expect(kids.length, greaterThanOrEqualTo(2));
      final ids = kids.map((k) => k['id']).toList();
      expect(ids, contains(kid1Id));
      expect(ids, contains(kid2Id));
    });

    test('DF1.2 Dashboard содержит корректную информацию', () async {
      final dash = await parentApi.get('families/dashboard');
      expect(dash['familyId'], isA<String>());
      expect(dash['familyName'], isA<String>());
      expect(dash['kids'], isA<List>());
      final kids = dash['kids'] as List;
      expect(kids.length, greaterThanOrEqualTo(2));

      for (final kid in kids) {
        expect(kid['kidId'], isA<String>());
        expect(kid['firstName'], isA<String>());
        expect(kid['mainAccountBalance'], isA<num>());
        expect(kid['totalXp'], isA<num>());
        expect(kid['currentStreak'], isA<num>());
      }
    });

    test('DF1.3 Уровень и XP ребёнка консистентны', () async {
      final me = await kid1Api.get('users/me');
      expect(me['totalXp'], isA<num>());
      expect(me['level'], isA<num>());
      expect(me['currentStreak'], isA<num>());
      final level = me['level'] as num;
      final xp = me['totalXp'] as num;
      expect(level, greaterThanOrEqualTo(1));
      expect(xp, greaterThanOrEqualTo(0));
    });

    test('DF1.4 Уровень ребёнка совпадает с dashboard', () async {
      final me = await kid1Api.get('users/me');
      final dash = await parentApi.get('families/dashboard');
      final kids = dash['kids'] as List;
      final kidSummary = kids.firstWhere((k) => k['kidId'] == kid1Id);

      expect(kidSummary['totalXp'], me['totalXp']);
    });
  });

  group('DF2. Полный цикл задания с проверкой баланса', () {
    late String taskId;
    late num kid1BalanceBefore;

    test('DF2.1 Запоминаем баланс ребёнка до задания', () async {
      final accs = await kid1Api.get('accounts/my') as List;
      kid1BalanceBefore = (accs.firstWhere((a) => a['type'] == 'Main'))['balance'] as num;
    });

    test('DF2.2 Родитель создаёт задание для kid1', () async {
      final resp = await parentApi.post('tasks', body: {
        'assignedToId': kid1Id,
        'title': 'DF Задание для проверки',
        'rewardAmount': 50,
        'description': 'Глубокий тест: проверяем награду',
      });
      taskId = resp['id'];
      expect(resp['assignedToId'], kid1Id);
      expect(resp['rewardAmount'], 50);
    });

    test('DF2.3 Kid1 видит задание', () async {
      final tasks = await kid1Api.get('tasks/my') as List;
      final task = tasks.firstWhere((t) => t['id'] == taskId);
      expect(task['status'], 'Pending');
    });

    test('DF2.4 Kid2 НЕ видит задание kid1', () async {
      final tasks = await kid2Api.get('tasks/my') as List;
      expect(tasks.any((t) => t['id'] == taskId), isFalse);
    });

    test('DF2.5 Kid1 выполняет задание', () async {
      await kid1Api.post('tasks/$taskId/complete', body: {});
      final tasks = await kid1Api.get('tasks/my') as List;
      final task = tasks.firstWhere((t) => t['id'] == taskId);
      expect(task['status'], 'Completed');
    });

    test('DF2.6 Родитель одобряет задание', () async {
      await parentApi.post('tasks/$taskId/approve');
      final tasks = await kid1Api.get('tasks/my') as List;
      final task = tasks.firstWhere((t) => t['id'] == taskId);
      expect(task['status'], 'Approved');
    });

    test('DF2.7 Баланс kid1 увеличился на сумму награды', () async {
      final accs = await kid1Api.get('accounts/my') as List;
      final kid1BalanceAfter = (accs.firstWhere((a) => a['type'] == 'Main'))['balance'] as num;
      expect(kid1BalanceAfter, kid1BalanceBefore + 50);
    });
  });

  group('DF3. Денежный запрос — полный цикл', () {
    late String requestId;
    late String approverParentId;

    test('DF3.1 Kid1 создаёт запрос денег', () async {
      final resp = await kid1Api.post('moneyrequests', body: {
        'amount': 100,
        'reason': 'DF тест запроса',
      });
      expect(resp['id'], isA<String>());
      expect(resp['parentId'], isA<String>());
      requestId = resp['id'];
      approverParentId = resp['parentId'];
    });

    test('DF3.2 Правильный родитель видит запрос в pending', () async {
      final me = await parentApi.get('users/me');
      late ApiService approver;
      if (me['id'] == approverParentId) {
        approver = parentApi;
      } else {
        approver = await loginAs('mama@kidbank.by', 'Parent123!');
      }

      final pending = await approver.get('moneyrequests/pending') as List;
      expect(pending.any((r) => r['id'] == requestId), isTrue);

      final req = pending.firstWhere((r) => r['id'] == requestId);
      expect(req['kidId'], kid1Id);
      expect(req['amount'], 100);
      expect(req['reason'], 'DF тест запроса');
      expect(req['status'], 'Pending');
    });

    test('DF3.3 Kid1 видит свой запрос', () async {
      final my = await kid1Api.get('moneyrequests/my') as List;
      expect(my.any((r) => r['id'] == requestId), isTrue);
    });
  });

  group('DF4. Копилка — перевод и возврат', () {
    late String savingsId;

    test('DF4.1 Kid1 создаёт копилку', () async {
      final resp = await kid1Api.post('accounts/savings', body: {
        'name': 'DF Копилка',
      });
      expect(resp, isNotNull);
      expect(resp['type'], 'Savings');
      savingsId = resp['id'];
    });

    test('DF4.2 Перевод из основного в копилку', () async {
      final accsBefore = await kid1Api.get('accounts/my') as List;
      final mainBefore = (accsBefore.firstWhere((a) => a['type'] == 'Main'))['balance'] as num;

      await kid1Api.post('accounts/transfer', body: {
        'sourceAccountId': kid1MainAccId,
        'destinationAccountId': savingsId,
        'amount': 20,
        'description': 'DF пополнение копилки',
      });

      final accsAfter = await kid1Api.get('accounts/my') as List;
      final mainAfter = (accsAfter.firstWhere((a) => a['type'] == 'Main'))['balance'] as num;
      final savingsAfter = (accsAfter.firstWhere((a) => a['id'] == savingsId))['balance'] as num;

      expect(mainAfter, mainBefore - 20);
      expect(savingsAfter, 20);
    });

    test('DF4.3 Перевод из копилки обратно в основной', () async {
      final accsBefore = await kid1Api.get('accounts/my') as List;
      final mainBefore = (accsBefore.firstWhere((a) => a['type'] == 'Main'))['balance'] as num;

      await kid1Api.post('accounts/transfer', body: {
        'sourceAccountId': savingsId,
        'destinationAccountId': kid1MainAccId,
        'amount': 10,
        'description': 'DF вывод из копилки',
      });

      final accsAfter = await kid1Api.get('accounts/my') as List;
      final mainAfter = (accsAfter.firstWhere((a) => a['type'] == 'Main'))['balance'] as num;
      final savingsAfter = (accsAfter.firstWhere((a) => a['id'] == savingsId))['balance'] as num;

      expect(mainAfter, mainBefore + 10);
      expect(savingsAfter, 10);
    });

    test('DF4.4 Транзакции копилки отражаются в истории', () async {
      final resp = await kid1Api.get('accounts/$savingsId/transactions?pageNumber=1&pageSize=10');
      final items = resp['items'] as List? ?? [];
      expect(items.length, greaterThanOrEqualTo(2));
    });
  });

  group('DF5. Цель — депозит и прогресс', () {
    late String goalId;

    test('DF5.1 Kid1 создаёт цель', () async {
      final resp = await kid1Api.post('goals', body: {
        'title': 'DF Тестовая цель',
        'targetAmount': 200,
        'description': 'Глубокий тест целей',
      });
      goalId = resp['id'];
      expect(resp['progressPercentage'], 0);
    });

    test('DF5.2 Kid1 депозитит в цель', () async {
      await kid1Api.post('goals/$goalId/deposit', body: {'amount': 50});
      final goals = await kid1Api.get('goals/my') as List;
      final goal = goals.firstWhere((g) => g['id'] == goalId);
      expect(goal['currentAmount'], 50);
      expect((goal['progressPercentage'] as num).toDouble(), 25.0);
    });

    test('DF5.3 Родитель видит цель ребёнка', () async {
      final goals = await parentApi.get('goals/kid/$kid1Id') as List;
      final goal = goals.firstWhere((g) => g['id'] == goalId);
      expect(goal['currentAmount'], 50);
      expect(goal['title'], 'DF Тестовая цель');
    });

    test('DF5.4 Удаление цели', () async {
      await kid1Api.delete('goals/$goalId');
      final goals = await kid1Api.get('goals/my') as List;
      expect(goals.any((g) => g['id'] == goalId), isFalse);
    });
  });

  group('DF6. Лидерборд — оба ребёнка', () {
    test('DF6.1 Лидерборд содержит обоих детей', () async {
      final resp = await kid1Api.get('leaderboard/family');
      expect(resp, isNotNull);
      final entries = resp['entries'] as List? ?? [];
      final ids = entries.map((e) => e['userId']).toList();
      expect(ids, contains(kid1Id));
      expect(ids, contains(kid2Id));
    });

    test('DF6.2 У каждого ребёнка в лидерборде есть XP и уровень', () async {
      final resp = await kid1Api.get('leaderboard/family');
      final entries = resp['entries'] as List;
      for (final entry in entries) {
        expect(entry['name'], isA<String>());
        expect(entry['totalXp'], isA<num>());
        expect(entry['level'], isA<num>());
        expect(entry['rank'], isA<num>());
      }
    });

    test('DF6.3 Родитель видит тот же лидерборд', () async {
      final resp = await parentApi.get('leaderboard/family');
      final entries = resp['entries'] as List;
      expect(entries.length, greaterThanOrEqualTo(2));
    });
  });

  group('DF7. Аналитика и отчёты — проверка полей DTO', () {
    test('DF7.1 KidSpendingSummary — корректные поля', () async {
      final resp = await parentApi.get('analytics/kid/$kid1Id/summary');
      expect(resp, isNotNull);
      expect(resp['kidId'], kid1Id);
      expect(resp['kidName'], isA<String>());
      expect(resp['totalBalance'], isA<num>());
      expect(resp['totalSpentThisMonth'], isA<num>());
      expect(resp['totalEarnedThisMonth'], isA<num>());
      expect(resp['tasksCompletedThisMonth'], isA<num>());
      expect(resp['taskRewardsThisMonth'], isA<num>());
      expect(resp['goalsCompleted'], isA<num>());
      expect(resp['activeGoals'], isA<num>());
      expect(resp['goalsSavings'], isA<num>());

      expect(resp.containsKey('totalSpent'), isFalse, reason: 'Правильное поле — totalSpentThisMonth');
      expect(resp.containsKey('totalEarned'), isFalse, reason: 'Правильное поле — totalEarnedThisMonth');
      expect(resp.containsKey('categoryBreakdown'), isFalse, reason: 'categoryBreakdown не существует в DTO');
    });

    test('DF7.2 MonthlyStats — корректные поля', () async {
      final resp = await parentApi.get('analytics/kid/$kid1Id/monthly?months=6') as List;
      expect(resp, isNotEmpty);
      final month = resp.first;
      expect(month['year'], isA<num>());
      expect(month['month'], isA<num>());
      expect(month['monthName'], isA<String>());
      expect(month['totalIncome'], isA<num>());
      expect(month['totalExpenses'], isA<num>());
      expect(month['netChange'], isA<num>());
      expect(month['transactionCount'], isA<num>());

      expect(month.containsKey('spent'), isFalse, reason: 'Правильное поле — totalExpenses');
    });

    test('DF7.3 FamilyAnalytics', () async {
      final resp = await parentApi.get('analytics/family/overview');
      expect(resp['totalKids'], isA<num>());
      expect(resp['totalKidsBalance'], isA<num>());
      expect(resp['kidsSummary'], isA<List>());
    });

    test('DF7.4 Kid Report', () async {
      final resp = await parentApi.get('reports/kid/$kid1Id?period=monthly');
      expect(resp, isNotNull);
      expect(resp['kidId'], kid1Id);
      expect(resp['kidName'], isA<String>());
      expect(resp['balance'], isA<Map>());
      expect(resp['goals'], isA<Map>());
      expect(resp['tasks'], isA<Map>());
    });

    test('DF7.5 Kid не может смотреть свою аналитику', () async {
      expect(
        () => kid1Api.get('analytics/kid/$kid1Id/summary'),
        throwsA(isA<ApiException>().having((e) => e.statusCode, 'status', 403)),
      );
    });
  });

  group('DF8. Уведомления и лента', () {
    test('DF8.1 Лента активности содержит события', () async {
      final resp = await kid1Api.get('activity/feed?pageNumber=1&pageSize=20');
      expect(resp, isNotNull);
    });

    test('DF8.2 Уведомления — корректная структура', () async {
      final resp = await kid1Api.get('notifications?limit=50');
      expect(resp, isNotNull);
      if (resp is List && resp.isNotEmpty) {
        final n = resp.first;
        expect(n['id'], isA<String>());
        expect(n['title'], isA<String>());
        expect(n['message'], isA<String>());
        expect(n['createdAt'], isA<String>());
        expect(n['isRead'], isA<bool>());
      }
    });
  });

  group('DF9. Достижения — проверка DTO', () {
    test('DF9.1 Все достижения имеют progress (не currentProgress)', () async {
      final list = await kid1Api.get('achievements/my') as List;
      for (final a in list) {
        expect(a.containsKey('progress'), isTrue, reason: 'Поле должно быть progress, не currentProgress');
        expect(a['progress'], isA<num>());
        expect(a['requiredProgress'], isA<num>());
        expect(a['title'], isA<String>());
        expect(a['code'], isA<String>());
        expect(a.containsKey('currentProgress'), isFalse, reason: 'currentProgress не существует в AchievementDto');
      }
    });
  });

  group('DF10. Межсемейная изоляция', () {
    test('DF10.1 Kid2 не может видеть цели kid1', () async {
      final resp = await kid1Api.post('goals', body: {
        'title': 'Приватная цель kid1',
        'targetAmount': 100,
      });
      final goalId = resp['id'];

      final kid2Goals = await kid2Api.get('goals/my') as List;
      expect(kid2Goals.any((g) => g['id'] == goalId), isFalse);

      await kid1Api.delete('goals/$goalId');
    });

    test('DF10.2 Kid2 не может выполнить задание kid1', () async {
      final resp = await parentApi.post('tasks', body: {
        'assignedToId': kid1Id,
        'title': 'Задание только для kid1',
        'rewardAmount': 5,
      });
      final taskId = resp['id'];

      expect(
        () => kid2Api.post('tasks/$taskId/complete', body: {}),
        throwsA(isA<ApiException>()),
      );

      await parentApi.delete('tasks/$taskId');
    });
  });
}
