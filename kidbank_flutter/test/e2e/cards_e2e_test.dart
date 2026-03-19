@Tags(['e2e'])
import 'package:flutter_test/flutter_test.dart';
import 'package:kidbank_flutter/core/api_service.dart';
import '../helpers/e2e_helpers.dart';

void main() {
  late ApiService parentApi;
  late ApiService kidApi;
  late String kidMainAccId;
  late String parentMainAccId;

  setUpAll(() async {
    parentApi = await loginAsParent();
    kidApi = await loginAsKid1();
    final kidAccs = await kidApi.get('accounts/my') as List;
    kidMainAccId = (kidAccs.firstWhere((a) => a['type'] == 'Main'))['id'];
    final parentAccs = await parentApi.get('accounts/my') as List;
    parentMainAccId = (parentAccs.firstWhere((a) => a['type'] == 'Main'))['id'];
  });

  group('9. Cards — Kid', () {
    late String cardId;

    test('9.1 POST cards — create virtual card (kid)', () async {
      final resp = await kidApi.post('cards', body: {
        'accountId': kidMainAccId,
        'cardName': 'E2E Kid Card',
      });
      expect(resp, isNotNull);
      expect(resp['id'], isA<String>());
      expect(resp['cardNumber'], isA<String>());
      expect(resp['cardHolderName'], isA<String>());
      expect(resp['expiryDate'], isA<String>());
      expect(resp['isActive'], isTrue);
      expect(resp['isFrozen'], isFalse);
      expect(resp['createdAt'], isA<String>());
      cardId = resp['id'];
    });

    test('9.2 GET cards/my — kid sees card with correct DTO fields', () async {
      final cards = await kidApi.get('cards/my') as List;
      expect(cards, isNotEmpty);
      final card = cards.firstWhere((c) => c['id'] == cardId);
      expect(card['cardNumber'], isA<String>());
      expect(card['cardHolderName'], isA<String>());
      expect(card['expiryDate'], isA<String>());
      expect(card['isActive'], isA<bool>());
      expect(card['isFrozen'], isA<bool>());

      expect(card.containsKey('last4Digits'), isFalse, reason: 'DTO не содержит last4Digits');
      expect(card.containsKey('cvv'), isFalse, reason: 'DTO не содержит cvv');
      expect(card.containsKey('cardholderName'), isFalse, reason: 'Правильное поле — cardHolderName');
    });

    test('9.3 POST cards/{id}/freeze — kid freezes card', () async {
      await kidApi.post('cards/$cardId/freeze');
      final cards = await kidApi.get('cards/my') as List;
      final card = cards.firstWhere((c) => c['id'] == cardId);
      expect(card['isFrozen'], isTrue);
    });

    test('9.4 POST cards/{id}/unfreeze — kid unfreezes card', () async {
      await kidApi.post('cards/$cardId/unfreeze');
      final cards = await kidApi.get('cards/my') as List;
      final card = cards.firstWhere((c) => c['id'] == cardId);
      expect(card['isFrozen'], isFalse);
    });

    test('9.5 POST cards without accountId — 400', () async {
      expect(
        () => kidApi.post('cards', body: {'currency': 'BYN'}),
        throwsA(isA<ApiException>().having((e) => e.statusCode, 'status', 400)),
      );
    });
  });

  group('9B. Cards — Parent', () {
    late String parentCardId;

    test('9B.1 POST cards — parent creates card', () async {
      final resp = await parentApi.post('cards', body: {
        'accountId': parentMainAccId,
        'cardName': 'E2E Parent Card',
      });
      expect(resp, isNotNull);
      expect(resp['id'], isA<String>());
      expect(resp['cardHolderName'], isA<String>());
      parentCardId = resp['id'];
    });

    test('9B.2 GET cards/my — parent sees own cards', () async {
      final cards = await parentApi.get('cards/my') as List;
      expect(cards.any((c) => c['id'] == parentCardId), isTrue);
    });

    test('9B.3 Freeze & unfreeze parent card', () async {
      await parentApi.post('cards/$parentCardId/freeze');
      var cards = await parentApi.get('cards/my') as List;
      expect(cards.firstWhere((c) => c['id'] == parentCardId)['isFrozen'], isTrue);

      await parentApi.post('cards/$parentCardId/unfreeze');
      cards = await parentApi.get('cards/my') as List;
      expect(cards.firstWhere((c) => c['id'] == parentCardId)['isFrozen'], isFalse);
    });
  });
}
