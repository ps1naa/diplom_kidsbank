import 'package:flutter/material.dart';
import '../core/api_service.dart';

enum AuthStatus { unknown, authenticated, unauthenticated }
enum UserRole { parent, kid }

class AuthProvider extends ChangeNotifier {
  final ApiService _api;
  AuthStatus _status = AuthStatus.unknown;
  UserRole? _role;
  String? _userId;
  String? _firstName;
  String? _lastName;
  String? _email;
  String? _familyId;

  AuthProvider(this._api) {
    _api.onForceLogout = _onForceLogout;
  }

  AuthStatus get status => _status;
  UserRole? get role => _role;
  String? get email => _email;
  String? get userId => _userId;
  String? get firstName => _firstName;
  String? get lastName => _lastName;
  String? get familyId => _familyId;
  String? get fullName =>
      _firstName != null ? '$_firstName${_lastName != null ? ' $_lastName' : ''}' : null;
  bool get isParent => _role == UserRole.parent;
  bool get isKid => _role == UserRole.kid;

  Future<void> _onForceLogout() async {
    _clearUserData();
    _status = AuthStatus.unauthenticated;
    notifyListeners();
  }

  Future<void> checkAuth() async {
    final token = await _api.accessToken;
    if (token == null) {
      _status = AuthStatus.unauthenticated;
      notifyListeners();
      return;
    }

    // Try loading cached user data first for fast startup
    final cached = await _api.loadUserData();
    if (cached['id'] != null) {
      _userId = cached['id'];
      _firstName = cached['firstName'];
      _lastName = cached['lastName'];
      _email = cached['email'];
      _role = cached['role'] == 'Parent' ? UserRole.parent : UserRole.kid;
    }

    try {
      final user = await _api.get('users/me');
      _setUser(user);
      await _api.saveUserData(user);
      _status = AuthStatus.authenticated;
    } on ApiException catch (e) {
      if (e.statusCode == 401) {
        // Token refresh already attempted inside ApiService
        _status = AuthStatus.unauthenticated;
        await _api.clearTokens();
      } else if (cached['id'] != null) {
        // Network error but we have cached data — allow offline start
        _status = AuthStatus.authenticated;
      } else {
        _status = AuthStatus.unauthenticated;
        await _api.clearTokens();
      }
    } catch (_) {
      if (cached['id'] != null) {
        _status = AuthStatus.authenticated;
      } else {
        _status = AuthStatus.unauthenticated;
      }
    }
    notifyListeners();
  }

  Future<void> login(String email, String password) async {
    final data = await _api.post('auth/login', body: {
      'email': email,
      'password': password,
    }, auth: false);
    await _api.saveTokens(data['accessToken'], data['refreshToken']);

    _userId = data['userId'];
    _email = data['email'];
    _firstName = data['firstName'];
    _lastName = data['lastName'];
    _familyId = data['familyId'];
    _role = data['role'] == 'Parent' ? UserRole.parent : UserRole.kid;

    await _api.saveUserData({
      'id': _userId,
      'email': _email,
      'firstName': _firstName,
      'lastName': _lastName,
      'role': data['role'],
    });

    _status = AuthStatus.authenticated;
    notifyListeners();
  }

  Future<void> registerParent({
    required String email,
    required String password,
    required String firstName,
    required String lastName,
    required String dateOfBirth,
    required String familyName,
  }) async {
    final data = await _api.post('auth/register/parent', body: {
      'email': email,
      'password': password,
      'firstName': firstName,
      'lastName': lastName,
      'dateOfBirth': dateOfBirth,
      'familyName': familyName,
    }, auth: false);
    await _api.saveTokens(data['accessToken'], data['refreshToken']);
    _userId = data['userId'];
    _role = UserRole.parent;
    _firstName = firstName;
    _lastName = lastName;
    _email = email;
    _familyId = data['familyId'];
    await _api.saveUserData({
      'id': _userId, 'email': email, 'firstName': firstName,
      'lastName': lastName, 'role': 'Parent',
    });
    _status = AuthStatus.authenticated;
    notifyListeners();
  }

  Future<void> registerKid({
    required String invitationToken,
    required String email,
    required String password,
    required String firstName,
    required String lastName,
    required String dateOfBirth,
  }) async {
    final data = await _api.post('auth/register/kid', body: {
      'invitationToken': invitationToken,
      'email': email,
      'password': password,
      'firstName': firstName,
      'lastName': lastName,
      'dateOfBirth': dateOfBirth,
    }, auth: false);
    await _api.saveTokens(data['accessToken'], data['refreshToken']);
    _userId = data['userId'];
    _role = UserRole.kid;
    _firstName = firstName;
    _lastName = lastName;
    _email = email;
    _familyId = data['familyId'];
    await _api.saveUserData({
      'id': _userId, 'email': email, 'firstName': firstName,
      'lastName': lastName, 'role': 'Kid',
    });
    _status = AuthStatus.authenticated;
    notifyListeners();
  }

  Future<void> logout() async {
    try {
      final rt = await _api.refreshToken;
      if (rt != null) {
        await _api.post('auth/logout', body: {'refreshToken': rt});
      }
    } catch (_) {}
    await _api.clearTokens();
    _clearUserData();
    _status = AuthStatus.unauthenticated;
    notifyListeners();
  }

  Future<void> revokeAllSessions() async {
    await _api.post('auth/revoke-all');
    await _api.clearTokens();
    _clearUserData();
    _status = AuthStatus.unauthenticated;
    notifyListeners();
  }

  void _setUser(Map<String, dynamic> user) {
    _userId = user['id'];
    _firstName = user['firstName'];
    _lastName = user['lastName'];
    _email = user['email'];
    _familyId = user['familyId'];
    _role = user['role'] == 'Parent' ? UserRole.parent : UserRole.kid;
  }

  void _clearUserData() {
    _role = null;
    _userId = null;
    _firstName = null;
    _lastName = null;
    _email = null;
    _familyId = null;
  }
}
