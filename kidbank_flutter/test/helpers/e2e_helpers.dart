import 'package:kidbank_flutter/core/token_store.dart';
import 'package:kidbank_flutter/core/api_service.dart';

class InMemoryTokenStore implements TokenStore {
  final Map<String, String> _data = {};

  @override
  Future<String?> read(String key) async => _data[key];

  @override
  Future<void> write(String key, String value) async => _data[key] = value;

  @override
  Future<void> deleteAll() async => _data.clear();
}

ApiService createRealApiService() {
  return ApiService(store: InMemoryTokenStore());
}

Future<ApiService> loginAs(String email, String password) async {
  final api = createRealApiService();
  final resp = await api.post('auth/login', body: {
    'email': email,
    'password': password,
  }, auth: false);
  await api.saveTokens(resp['accessToken'], resp['refreshToken']);
  await api.saveUserData(resp);
  return api;
}

Future<ApiService> loginAsParent() => loginAs('parent@kidbank.by', 'Parent123!');
Future<ApiService> loginAsKid1() => loginAs('kid1@kidbank.by', 'Kid12345!');
Future<ApiService> loginAsKid2() => loginAs('kid2@kidbank.by', 'Kid12345!');
