@Tags(['e2e'])
import 'package:flutter_test/flutter_test.dart';
import '../helpers/e2e_helpers.dart';

void main() {
  late dynamic parentApi;

  setUpAll(() async {
    parentApi = await loginAsParent();
  });

  group('4. Families', () {
    test('4.1 GET families/dashboard', () async {
      final resp = await parentApi.get('families/dashboard');
      expect(resp, isNotNull);
      expect(resp['familyName'], isA<String>());
    });

    test('4.2 GET families/kids', () async {
      final kids = await parentApi.get('families/kids') as List;
      expect(kids, isNotEmpty);
      expect(kids.first['firstName'], isA<String>());
      expect(kids.first['id'], isA<String>());
      expect(kids.first['totalXp'], isA<num>());
    });
  });
}
