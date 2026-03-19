@Tags(['e2e'])
import 'package:flutter_test/flutter_test.dart';
import 'package:kidbank_flutter/core/api_service.dart';
import '../helpers/e2e_helpers.dart';

void main() {
  late ApiService kidApi;
  late ApiService approverApi;

  setUpAll(() async {
    kidApi = await loginAsKid1();

    final probe = await kidApi.post('moneyrequests', body: {
      'amount': 1,
      'currency': 'BYN',
      'reason': 'probe to find parent',
    });
    final parentId = probe['parentId'] as String;

    final parentApi1 = await loginAsParent();
    final me1 = await parentApi1.get('users/me');
    if (me1['id'] == parentId) {
      approverApi = parentApi1;
    } else {
      approverApi = await loginAs('mama@kidbank.by', 'Parent123!');
    }
  });

  group('8. Money Requests', () {
    late String requestId;

    test('8.1 POST moneyrequests — kid creates request', () async {
      final resp = await kidApi.post('moneyrequests', body: {
        'amount': 30,
        'currency': 'BYN',
        'reason': 'E2E тестовый запрос',
      });
      expect(resp, isNotNull);
      expect(resp['id'], isA<String>());
      expect(resp['parentId'], isA<String>());
      requestId = resp['id'];
    });

    test('8.2 GET moneyrequests/my — kid sees own requests', () async {
      final list = await kidApi.get('moneyrequests/my') as List;
      expect(list.any((r) => r['id'] == requestId), isTrue);
    });

    test('8.3 GET moneyrequests/pending — correct parent sees pending', () async {
      final list = await approverApi.get('moneyrequests/pending') as List;
      expect(list.any((r) => r['id'] == requestId), isTrue);
    });

    test('8.4 POST moneyrequests/{id}/approve', () async {
      await approverApi.post('moneyrequests/$requestId/approve', body: {
        'note': 'Одобрено E2E',
      });

      final list = await kidApi.get('moneyrequests/my') as List;
      final req = list.firstWhere((r) => r['id'] == requestId);
      expect(req['status'], 'Approved');
    });

    test('8.5 Create and reject money request', () async {
      final resp = await kidApi.post('moneyrequests', body: {
        'amount': 500,
        'currency': 'BYN',
        'reason': 'Отклонить E2E',
      });
      final id = resp['id'];

      await approverApi.post('moneyrequests/$id/reject', body: {
        'note': 'Слишком много',
      });

      final list = await kidApi.get('moneyrequests/my') as List;
      final req = list.firstWhere((r) => r['id'] == id);
      expect(req['status'], 'Rejected');
    });
  });
}
