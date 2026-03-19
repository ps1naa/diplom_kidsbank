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

  group('16. Notifications', () {
    test('16.1 GET notifications', () async {
      final resp = await kidApi.get('notifications?limit=50');
      expect(resp, isNotNull);
    });

    test('16.2 POST notifications/{id}/read', () async {
      final resp = await kidApi.get('notifications?limit=50');
      final items = (resp is List) ? resp : (resp['items'] ?? []) as List;
      if (items.isNotEmpty) {
        final id = items.first['id'];
        await kidApi.post('notifications/$id/read');
      }
    });
  });

  group('17. Settings', () {
    test('17.1 GET settings/client', () async {
      final resp = await kidApi.get('settings/client');
      expect(resp, isNotNull);
    });

    test('17.2 PUT settings/client', () async {
      await kidApi.put('settings/client', body: {
        'key': 'theme',
        'value': 'dark',
      });

      await kidApi.put('settings/client', body: {
        'key': 'theme',
        'value': 'light',
      });
    });
  });

  group('18. Activity Feed', () {
    test('18.1 GET activity/feed', () async {
      final resp = await kidApi.get('activity/feed?pageNumber=1&pageSize=20');
      expect(resp, isNotNull);
    });

    test('18.2 GET activity/feed — parent', () async {
      final resp = await parentApi.get('activity/feed?pageNumber=1&pageSize=20');
      expect(resp, isNotNull);
    });
  });

  group('19. Analytics (parent only)', () {
    test('19.1 GET analytics/kid/{kidId}/summary', () async {
      final resp = await parentApi.get('analytics/kid/$kidId/summary');
      expect(resp, isNotNull);
    });

    test('19.2 GET analytics/kid/{kidId}/monthly', () async {
      final resp = await parentApi.get('analytics/kid/$kidId/monthly?months=6');
      expect(resp, isNotNull);
    });

    test('19.3 GET analytics/family/overview', () async {
      final resp = await parentApi.get('analytics/family/overview');
      expect(resp, isNotNull);
    });

    test('19.4 Kid cannot access analytics', () async {
      expect(
        () => kidApi.get('analytics/kid/$kidId/summary'),
        throwsA(isA<ApiException>().having((e) => e.statusCode, 'status', 403)),
      );
    });
  });

  group('20. Leaderboard', () {
    test('20.1 GET leaderboard/family', () async {
      final resp = await kidApi.get('leaderboard/family');
      expect(resp, isNotNull);
    });

    test('20.2 GET leaderboard/family — parent', () async {
      final resp = await parentApi.get('leaderboard/family');
      expect(resp, isNotNull);
    });
  });

  group('21. Reports (parent only)', () {
    test('21.1 GET reports/kid/{kidId} — monthly', () async {
      final resp = await parentApi.get('reports/kid/$kidId?period=monthly');
      expect(resp, isNotNull);
    });

    test('21.2 GET reports/kid/{kidId} — weekly', () async {
      final resp = await parentApi.get('reports/kid/$kidId?period=weekly');
      expect(resp, isNotNull);
    });

    test('21.3 Kid cannot access reports', () async {
      expect(
        () => kidApi.get('reports/kid/$kidId?period=monthly'),
        throwsA(isA<ApiException>().having((e) => e.statusCode, 'status', 403)),
      );
    });
  });
}
