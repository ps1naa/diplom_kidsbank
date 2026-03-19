import 'dart:async';
import 'dart:convert';
import 'dart:io';
import 'package:flutter/foundation.dart';
import 'package:http/http.dart' as http;
import 'constants.dart';
import 'token_store.dart';

class ApiException implements Exception {
  final int statusCode;
  final String message;
  final String? code;
  ApiException(this.statusCode, this.message, {this.code});
  @override
  String toString() => message;
}

class NetworkException implements Exception {
  final String message;
  NetworkException(this.message);
  @override
  String toString() => message;
}

typedef LogoutCallback = Future<void> Function();

class ApiService {
  final TokenStore _store;
  final http.Client _client;
  bool _isRefreshing = false;
  final List<Completer<void>> _refreshQueue = [];
  LogoutCallback? onForceLogout;

  ApiService({http.Client? client, TokenStore? store})
      : _client = client ?? http.Client(),
        _store = store ?? SecureTokenStore();

  String get _baseUrl {
    if (kIsWeb) return 'http://localhost:5000/api/v1';
    if (Platform.isAndroid) return 'http://10.0.2.2:5000/api/v1';
    return 'http://localhost:5000/api/v1';
  }

  Future<Map<String, String>> _headers({bool auth = true}) async {
    final h = <String, String>{'Content-Type': 'application/json'};
    if (auth) {
      final token = await _store.read('access_token');
      if (token != null) h['Authorization'] = 'Bearer $token';
    }
    return h;
  }

  Future<dynamic> get(String path, {bool auth = true}) =>
      _request('GET', path, auth: auth);

  Future<dynamic> post(String path,
          {Map<String, dynamic>? body, bool auth = true}) =>
      _request('POST', path, body: body, auth: auth);

  Future<dynamic> put(String path,
          {Map<String, dynamic>? body, bool auth = true}) =>
      _request('PUT', path, body: body, auth: auth);

  Future<dynamic> delete(String path, {bool auth = true}) =>
      _request('DELETE', path, auth: auth);

  Future<dynamic> _request(String method, String path,
      {Map<String, dynamic>? body,
      bool auth = true,
      bool isRetry = false}) async {
    try {
      final uri = Uri.parse('$_baseUrl/$path');
      final headers = await _headers(auth: auth);
      final encoded = body != null ? jsonEncode(body) : null;

      http.Response resp;
      switch (method) {
        case 'GET':
          resp = await _client
              .get(uri, headers: headers)
              .timeout(ApiConstants.timeout);
        case 'POST':
          resp = await _client
              .post(uri, headers: headers, body: encoded)
              .timeout(ApiConstants.timeout);
        case 'PUT':
          resp = await _client
              .put(uri, headers: headers, body: encoded)
              .timeout(ApiConstants.timeout);
        case 'DELETE':
          resp = await _client
              .delete(uri, headers: headers)
              .timeout(ApiConstants.timeout);
        default:
          throw UnsupportedError('HTTP method $method');
      }

      if (resp.statusCode == 401 && auth && !isRetry) {
        final refreshed = await _tryRefreshToken();
        if (refreshed) {
          return _request(method, path,
              body: body, auth: auth, isRetry: true);
        }
        await _forceLogout();
        throw ApiException(401, 'Сессия истекла. Войдите заново.',
            code: 'SESSION_EXPIRED');
      }

      return _handle(resp);
    } on SocketException {
      throw NetworkException('Нет подключения к интернету');
    } on TimeoutException {
      throw NetworkException('Превышено время ожидания. Попробуйте позже.');
    } on http.ClientException {
      throw NetworkException('Ошибка соединения с сервером');
    }
  }

  dynamic _handle(http.Response resp) {
    if (resp.statusCode >= 200 && resp.statusCode < 300) {
      if (resp.body.isEmpty) return null;
      return jsonDecode(resp.body);
    }

    String msg;
    String? code;
    try {
      final body = jsonDecode(resp.body);
      msg = body['message'] ?? body['title'] ?? '';
      code = body['code'];
    } catch (_) {
      msg = '';
    }

    switch (resp.statusCode) {
      case 400:
        throw ApiException(400, msg.isNotEmpty ? msg : 'Некорректный запрос',
            code: code ?? 'VALIDATION_ERROR');
      case 401:
        throw ApiException(401, msg.isNotEmpty ? msg : 'Не авторизован',
            code: code ?? 'UNAUTHORIZED');
      case 403:
        throw ApiException(403, msg.isNotEmpty ? msg : 'Доступ запрещён',
            code: code ?? 'FORBIDDEN');
      case 404:
        throw ApiException(404, msg.isNotEmpty ? msg : 'Не найдено',
            code: code ?? 'NOT_FOUND');
      case 409:
        throw ApiException(409, msg.isNotEmpty ? msg : 'Конфликт данных',
            code: code ?? 'CONFLICT');
      default:
        throw ApiException(
            resp.statusCode,
            msg.isNotEmpty
                ? msg
                : 'Ошибка сервера (${resp.statusCode})',
            code: code ?? 'INTERNAL_ERROR');
    }
  }

  Future<bool> _tryRefreshToken() async {
    if (_isRefreshing) {
      final completer = Completer<void>();
      _refreshQueue.add(completer);
      await completer.future;
      final newToken = await _store.read('access_token');
      return newToken != null;
    }

    _isRefreshing = true;
    try {
      final at = await _store.read('access_token');
      final rt = await _store.read('refresh_token');
      if (at == null || rt == null) return false;

      final resp = await _client
          .post(
            Uri.parse('$_baseUrl/auth/refresh'),
            headers: {'Content-Type': 'application/json'},
            body: jsonEncode({'accessToken': at, 'refreshToken': rt}),
          )
          .timeout(const Duration(seconds: 10));

      if (resp.statusCode == 200) {
        final data = jsonDecode(resp.body);
        await saveTokens(data['accessToken'], data['refreshToken']);
        for (final c in _refreshQueue) {
          c.complete();
        }
        _refreshQueue.clear();
        return true;
      }
      for (final c in _refreshQueue) {
        c.complete();
      }
      _refreshQueue.clear();
      return false;
    } catch (_) {
      for (final c in _refreshQueue) {
        c.complete();
      }
      _refreshQueue.clear();
      return false;
    } finally {
      _isRefreshing = false;
    }
  }

  Future<void> _forceLogout() async {
    await clearTokens();
    onForceLogout?.call();
  }

  Future<void> saveTokens(String accessToken, String refreshToken) async {
    await _store.write('access_token', accessToken);
    await _store.write('refresh_token', refreshToken);
  }

  Future<void> saveUserData(Map<String, dynamic> userData) async {
    await _store.write('user_id', userData['id']?.toString() ?? '');
    await _store.write('user_role', userData['role']?.toString() ?? '');
    await _store.write(
        'user_first_name', userData['firstName']?.toString() ?? '');
    await _store.write(
        'user_last_name', userData['lastName']?.toString() ?? '');
    await _store.write('user_email', userData['email']?.toString() ?? '');
  }

  Future<Map<String, String?>> loadUserData() async {
    return {
      'id': await _store.read('user_id'),
      'role': await _store.read('user_role'),
      'firstName': await _store.read('user_first_name'),
      'lastName': await _store.read('user_last_name'),
      'email': await _store.read('user_email'),
    };
  }

  Future<void> clearTokens() async {
    await _store.deleteAll();
  }

  Future<String?> get accessToken => _store.read('access_token');
  Future<String?> get refreshToken => _store.read('refresh_token');
}
