@Tags(['e2e'])
import 'package:flutter_test/flutter_test.dart';
import 'package:kidbank_flutter/core/api_service.dart';
import '../helpers/e2e_helpers.dart';

void main() {
  late ApiService parentApi;
  late ApiService kidApi;
  late String parentId;
  late String kidId;

  setUpAll(() async {
    parentApi = await loginAsParent();
    kidApi = await loginAsKid1();
    final parentMe = await parentApi.get('users/me');
    parentId = parentMe['id'];
    final kidMe = await kidApi.get('users/me');
    kidId = kidMe['id'];
  });

  group('13. Chat', () {
    test('13.1 POST chat/send — kid sends family message', () async {
      final resp = await kidApi.post('chat/send', body: {
        'content': 'Семейное сообщение от ребёнка E2E',
      });
      expect(resp, isNotNull);
      expect(resp['id'], isA<String>());
      expect(resp['senderId'], kidId);
      expect(resp['senderName'], isA<String>());
      expect(resp['content'], 'Семейное сообщение от ребёнка E2E');
      expect(resp['createdAt'], isA<String>());
      expect(resp['recipientId'], isNull);
    });

    test('13.2 POST chat/send — kid sends direct message to parent', () async {
      final resp = await kidApi.post('chat/send', body: {
        'content': 'Личное сообщение для родителя E2E',
        'recipientId': parentId,
      });
      expect(resp, isNotNull);
      expect(resp['recipientId'], parentId);
      expect(resp['recipientName'], isA<String>());
    });

    test('13.3 POST chat/send — parent replies to kid', () async {
      final resp = await parentApi.post('chat/send', body: {
        'content': 'Ответ от родителя E2E',
        'recipientId': kidId,
      });
      expect(resp, isNotNull);
      expect(resp['senderId'], parentId);
      expect(resp['recipientId'], kidId);
    });

    test('13.4 GET chat/history — kid sees messages (paginated)', () async {
      final resp = await kidApi.get('chat/history?pageNumber=1&pageSize=50');
      expect(resp, isNotNull);
      final items = resp is List ? resp : (resp['items'] ?? []) as List;
      expect(items, isNotEmpty);

      final msg = items.first;
      expect(msg['senderId'], isA<String>());
      expect(msg['senderName'], isA<String>());
      expect(msg['content'], isA<String>());
      expect(msg['createdAt'], isA<String>());
    });

    test('13.5 GET chat/history — parent sees messages', () async {
      final resp = await parentApi.get('chat/history?pageNumber=1&pageSize=50');
      expect(resp, isNotNull);
      final items = resp is List ? resp : (resp['items'] ?? []) as List;
      expect(items, isNotEmpty);
    });

    test('13.6 Chat cross-read — parent sends, kid reads', () async {
      final uniqueMsg = 'CrossRead_${DateTime.now().millisecondsSinceEpoch}';
      await parentApi.post('chat/send', body: {'content': uniqueMsg});

      await Future.delayed(const Duration(milliseconds: 500));

      final resp = await kidApi.get('chat/history?pageNumber=1&pageSize=50');
      final items = resp is List ? resp : (resp['items'] ?? []) as List;
      final found = items.any((m) => m['content'] == uniqueMsg);
      expect(found, isTrue, reason: 'Ребёнок должен видеть семейное сообщение от родителя');
    });

    test('13.7 GET chat/history with withUserId — direct chat filter', () async {
      final resp = await kidApi.get('chat/history?pageNumber=1&pageSize=50&withUserId=$parentId');
      expect(resp, isNotNull);
      final items = resp is List ? resp : (resp['items'] ?? []) as List;
      for (final m in items) {
        final isDirectWithParent =
            (m['senderId'] == kidId && m['recipientId'] == parentId) ||
            (m['senderId'] == parentId && m['recipientId'] == kidId);
        expect(isDirectWithParent, isTrue, reason: 'Фильтр withUserId должен показывать только прямые сообщения');
      }
    });
  });
}
