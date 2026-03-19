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

  group('10. Categories', () {
    test('10.1 GET categories — parent', () async {
      final cats = await parentApi.get('categories') as List;
      expect(cats, isNotEmpty);
      expect(cats.first['name'], isA<String>());
      expect(cats.first['id'], isA<String>());
    });

    test('10.2 GET categories — kid', () async {
      final cats = await kidApi.get('categories') as List;
      expect(cats, isNotEmpty);
    });

    test('10.3 POST categories/block — block category for kid', () async {
      final cats = await parentApi.get('categories') as List;
      final catId = cats.first['id'];

      await parentApi.post('categories/block', body: {
        'categoryId': catId,
        'kidId': kidId,
        'isBlocked': true,
      });

      await parentApi.post('categories/block', body: {
        'categoryId': catId,
        'kidId': kidId,
        'isBlocked': false,
      });
    });
  });

  group('11. Spending Limits', () {
    late String limitId;

    test('11.1 POST spendinglimits — create limit', () async {
      final resp = await parentApi.post('spendinglimits', body: {
        'kidId': kidId,
        'limitAmount': 200,
        'period': 'Monthly',
      });
      expect(resp, isNotNull);
      limitId = resp['id'];
    });

    test('11.2 PUT spendinglimits/{id} — update limit', () async {
      await parentApi.put('spendinglimits/$limitId', body: {
        'limitAmount': 250,
      });
    });

    test('11.3 GET spendinglimits/kid/{kidId}', () async {
      final limits = await parentApi.get('spendinglimits/kid/$kidId') as List;
      expect(limits, isNotEmpty);
      expect(limits.any((l) => l['id'] == limitId), isTrue);
    });
  });
}
