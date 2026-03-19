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

  group('2. Users', () {
    test('2.1 GET users/me — parent', () async {
      final me = await parentApi.get('users/me');
      expect(me, isNotNull);
      expect(me['email'], 'parent@kidbank.by');
      expect(me['role'], 'Parent');
      expect(me['firstName'], isA<String>());
      expect(me['lastName'], isA<String>());
      expect(me['familyId'], isA<String>());
    });

    test('2.2 GET users/me — kid', () async {
      final me = await kidApi.get('users/me');
      expect(me, isNotNull);
      expect(me['email'], 'kid1@kidbank.by');
      expect(me['role'], 'Kid');
    });

    test('2.3 PUT users/me — update profile', () async {
      await parentApi.put('users/me', body: {
        'firstName': 'Александр',
        'lastName': 'Иванов',
      });

      final me = await parentApi.get('users/me');
      expect(me['firstName'], 'Александр');
      expect(me['lastName'], 'Иванов');
    });

    test('2.4 GET users/{id} — parent views kid', () async {
      final kidMe = await kidApi.get('users/me');
      final kidId = kidMe['id'];
      final user = await parentApi.get('users/$kidId');
      expect(user, isNotNull);
      expect(user['id'], kidId);
      expect(user['role'], 'Kid');
    });

    test('2.5 Kid cannot access users/{id}', () async {
      final parentMe = await parentApi.get('users/me');
      expect(
        () => kidApi.get('users/${parentMe['id']}'),
        throwsA(isA<ApiException>().having((e) => e.statusCode, 'status', 403)),
      );
    });
  });
}
