@Tags(['e2e'])
import 'package:flutter_test/flutter_test.dart';
import 'package:kidbank_flutter/core/api_service.dart';
import '../helpers/e2e_helpers.dart';

void main() {
  late ApiService parentApi;
  late ApiService kidApi;
  late String kidId;

  setUpAll(() async {
    parentApi = await loginAsParent();
    kidApi = await loginAsKid1();
    final kidMe = await kidApi.get('users/me');
    kidId = kidMe['id'];
  });

  group('7. Goals', () {
    late String goalId;

    test('7.1 POST goals — kid creates goal', () async {
      final resp = await kidApi.post('goals', body: {
        'title': 'E2E Цель',
        'targetAmount': 500,
        'description': 'Тестовая цель',
        'currency': 'BYN',
      });
      expect(resp, isNotNull);
      expect(resp['id'], isA<String>());
      expect(resp['title'], 'E2E Цель');
      goalId = resp['id'];
    });

    test('7.2 GET goals/my — kid sees own goals', () async {
      final goals = await kidApi.get('goals/my?includeCompleted=true') as List;
      expect(goals.any((g) => g['id'] == goalId), isTrue);
    });

    test('7.3 PUT goals/{id} — kid updates goal', () async {
      await kidApi.put('goals/$goalId', body: {
        'title': 'E2E Цель (обн.)',
        'targetAmount': 600,
      });
      final goals = await kidApi.get('goals/my') as List;
      final goal = goals.firstWhere((g) => g['id'] == goalId);
      expect(goal['title'], 'E2E Цель (обн.)');
      expect(goal['targetAmount'], 600);
    });

    test('7.4 POST goals/{id}/deposit — kid deposits to goal', () async {
      await kidApi.post('goals/$goalId/deposit', body: {'amount': 10});
      final goals = await kidApi.get('goals/my') as List;
      final goal = goals.firstWhere((g) => g['id'] == goalId);
      expect(goal['currentAmount'], greaterThanOrEqualTo(10));
    });

    test('7.5 GET goals/kid/{kidId} — parent views kid goals', () async {
      final goals = await parentApi.get('goals/kid/$kidId?includeCompleted=true') as List;
      expect(goals.any((g) => g['id'] == goalId), isTrue);
    });

    test('7.6 Parent cannot deposit to kid goal directly (403)', () async {
      expect(
        () => parentApi.post('goals/$goalId/deposit', body: {'amount': 50}),
        throwsA(isA<ApiException>().having((e) => e.statusCode, 'status', 403)),
      );
    });

    test('7.7 DELETE goals/{id} — kid deletes goal', () async {
      await kidApi.delete('goals/$goalId');
      final goals = await kidApi.get('goals/my') as List;
      expect(goals.any((g) => g['id'] == goalId), isFalse);
    });
  });
}
