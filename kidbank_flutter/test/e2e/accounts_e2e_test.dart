@Tags(['e2e'])
import 'package:flutter_test/flutter_test.dart';
import 'package:kidbank_flutter/core/api_service.dart';
import '../helpers/e2e_helpers.dart';

void main() {
  late ApiService parentApi;
  late ApiService kidApi;

  setUpAll(() async {
    parentApi = await loginAsParent();
    kidApi = await loginAsKid1();
  });

  group('3. Accounts', () {
    late String parentMainAccId;
    late String kidMainAccId;

    test('3.1 GET accounts/my — parent', () async {
      final accs = await parentApi.get('accounts/my') as List;
      expect(accs, isNotEmpty);
      final main = accs.firstWhere((a) => a['type'] == 'Main');
      expect(main['balance'], isA<num>());
      parentMainAccId = main['id'];
    });

    test('3.2 GET accounts/my — kid', () async {
      final accs = await kidApi.get('accounts/my') as List;
      expect(accs, isNotEmpty);
      final main = accs.firstWhere((a) => a['type'] == 'Main');
      kidMainAccId = main['id'];
    });

    test('3.3 GET accounts/kid/{kidId} — parent views kid accounts', () async {
      final kidMe = await kidApi.get('users/me');
      final accs = await parentApi.get('accounts/kid/${kidMe['id']}') as List;
      expect(accs, isNotEmpty);
      expect(accs.any((a) => a['type'] == 'Main'), isTrue);
    });

    test('3.4 POST accounts/topup — parent topup', () async {
      final before = await parentApi.get('accounts/my') as List;
      final balBefore = (before.firstWhere((a) => a['type'] == 'Main'))['balance'] as num;

      await parentApi.post('accounts/topup', body: {
        'amount': 100,
      });

      final after = await parentApi.get('accounts/my') as List;
      final balAfter = (after.firstWhere((a) => a['type'] == 'Main'))['balance'] as num;
      expect(balAfter, balBefore + 100);
    });

    test('3.5 POST accounts/deposit — parent deposits to kid', () async {
      final kidMe = await kidApi.get('users/me');
      final kidAccsBefore = await kidApi.get('accounts/my') as List;
      final kidBalBefore = (kidAccsBefore.firstWhere((a) => a['type'] == 'Main'))['balance'] as num;

      await parentApi.post('accounts/deposit', body: {
        'kidId': kidMe['id'],
        'amount': 25,
        'description': 'E2E deposit test',
      });

      final kidAccsAfter = await kidApi.get('accounts/my') as List;
      final kidBalAfter = (kidAccsAfter.firstWhere((a) => a['type'] == 'Main'))['balance'] as num;
      expect(kidBalAfter, kidBalBefore + 25);
    });

    test('3.6 POST accounts/savings — create savings account', () async {
      final resp = await parentApi.post('accounts/savings', body: {
        'name': 'E2E Savings',
      });
      expect(resp, isNotNull);
      expect(resp['type'], 'Savings');
    });

    test('3.7 POST accounts/transfer — transfer between accounts', () async {
      final accs = await parentApi.get('accounts/my') as List;
      final main = accs.firstWhere((a) => a['type'] == 'Main');
      final savingsList = accs.where((a) => a['type'] == 'Savings').toList();
      expect(savingsList, isNotEmpty);
      final savings = savingsList.last;
      final savingsBalBefore = (savings['balance'] as num);

      await parentApi.post('accounts/transfer', body: {
        'sourceAccountId': main['id'],
        'destinationAccountId': savings['id'],
        'amount': 10,
        'description': 'E2E transfer',
      });

      final accsAfter = await parentApi.get('accounts/my') as List;
      final savingsAfter = accsAfter.firstWhere((a) => a['id'] == savings['id']);
      expect(savingsAfter['balance'] as num, savingsBalBefore + 10);
    });

    test('3.8 GET accounts/{id}/transactions — view transactions', () async {
      final resp = await kidApi.get(
          'accounts/$kidMainAccId/transactions?pageNumber=1&pageSize=10');
      expect(resp, isNotNull);
      expect(resp['items'], isA<List>());
    });
  });
}
