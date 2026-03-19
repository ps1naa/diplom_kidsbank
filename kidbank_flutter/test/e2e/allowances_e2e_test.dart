@Tags(['e2e'])
import 'package:flutter_test/flutter_test.dart';
import 'package:kidbank_flutter/core/api_service.dart';
import '../helpers/e2e_helpers.dart';

void main() {
  late ApiService parentApi;
  late String kidId;

  setUpAll(() async {
    parentApi = await loginAsParent();
    final kidApi = await loginAsKid1();
    final kidMe = await kidApi.get('users/me');
    kidId = kidMe['id'];
  });

  group('12. Allowances', () {
    test('12.1 POST allowances — create recurring allowance', () async {
      await parentApi.post('allowances', body: {
        'kidId': kidId,
        'amount': 75,
        'frequency': 'Monthly',
      });
    });

    test('12.2 GET allowances/kid/{kidId}', () async {
      final resp = await parentApi.get('allowances/kid/$kidId');
      expect(resp, isNotNull);
    });
  });
}
