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

  group('5. Tasks', () {
    late String taskId;

    test('5.1 POST tasks — parent creates task for kid', () async {
      final resp = await parentApi.post('tasks', body: {
        'assignedToId': kidId,
        'title': 'E2E задание',
        'rewardAmount': 15,
        'currency': 'BYN',
        'description': 'Тестовое задание E2E',
      });
      expect(resp, isNotNull);
      expect(resp['id'], isA<String>());
      taskId = resp['id'];
    });

    test('5.2 GET tasks/my — kid sees assigned tasks', () async {
      final tasks = await kidApi.get('tasks/my') as List;
      expect(tasks, isNotEmpty);
      expect(tasks.any((t) => t['id'] == taskId), isTrue);
    });

    test('5.3 PUT tasks/{id} — parent updates task', () async {
      await parentApi.put('tasks/$taskId', body: {
        'title': 'E2E задание (обн.)',
        'rewardAmount': 20,
      });

      final tasks = await kidApi.get('tasks/my') as List;
      final updated = tasks.firstWhere((t) => t['id'] == taskId);
      expect(updated['title'], 'E2E задание (обн.)');
      expect(updated['rewardAmount'], 20);
    });

    test('5.4 POST tasks/{id}/complete — kid completes task', () async {
      await kidApi.post('tasks/$taskId/complete', body: {
        'proofUrl': 'https://example.com/proof.jpg',
      });

      final tasks = await kidApi.get('tasks/my') as List;
      final task = tasks.firstWhere((t) => t['id'] == taskId);
      expect(task['status'], 'Completed');
    });

    test('5.5 GET tasks/pending-approval — parent sees completed task', () async {
      final pending = await parentApi.get('tasks/pending-approval') as List;
      expect(pending.any((t) => t['id'] == taskId), isTrue);
    });

    test('5.6 POST tasks/{id}/approve — parent approves', () async {
      await parentApi.post('tasks/$taskId/approve');

      final tasks = await kidApi.get('tasks/my') as List;
      final task = tasks.firstWhere((t) => t['id'] == taskId);
      expect(task['status'], 'Approved');
    });

    test('5.7 Create and reject task', () async {
      final resp = await parentApi.post('tasks', body: {
        'assignedToId': kidId,
        'title': 'E2E отклонить',
        'rewardAmount': 5,
        'currency': 'BYN',
      });
      final id = resp['id'];

      await kidApi.post('tasks/$id/complete');
      await parentApi.post('tasks/$id/reject', body: {
        'reason': 'Не полностью выполнено',
      });

      final tasks = await kidApi.get('tasks/my') as List;
      final task = tasks.firstWhere((t) => t['id'] == id);
      expect(task['status'], 'Rejected');
    });

    test('5.8 DELETE tasks/{id} — delete pending task', () async {
      final resp = await parentApi.post('tasks', body: {
        'assignedToId': kidId,
        'title': 'E2E удалить',
        'rewardAmount': 1,
        'currency': 'BYN',
      });
      await parentApi.delete('tasks/${resp['id']}');

      final tasks = await kidApi.get('tasks/my') as List;
      expect(tasks.any((t) => t['id'] == resp['id']), isFalse);
    });
  });

  group('6. Task Templates', () {
    late String tplId;

    test('6.1 POST tasktemplates — create template', () async {
      final resp = await parentApi.post('tasktemplates', body: {
        'title': 'E2E шаблон',
        'rewardAmount': 10,
        'description': 'Шаблон задания E2E',
      });
      expect(resp, isNotNull);
      tplId = resp['id'];
    });

    test('6.2 GET tasktemplates/my', () async {
      final list = await parentApi.get('tasktemplates/my') as List;
      expect(list.any((t) => t['id'] == tplId), isTrue);
    });

    test('6.3 POST tasktemplates/{id}/assign — assign from template', () async {
      final resp = await parentApi.post('tasktemplates/$tplId/assign', body: {
        'assignedToId': kidId,
      });
      expect(resp, isNotNull);
      expect(resp['id'], isA<String>());
    });

    test('6.4 DELETE tasktemplates/{id}', () async {
      await parentApi.delete('tasktemplates/$tplId');
      final list = await parentApi.get('tasktemplates/my') as List;
      expect(list.any((t) => t['id'] == tplId), isFalse);
    });
  });
}
