@Tags(['e2e'])
import 'dart:math';
import 'package:flutter_test/flutter_test.dart';
import 'package:kidbank_flutter/core/api_service.dart';
import '../helpers/e2e_helpers.dart';

void main() {
  final uid = Random().nextInt(99999).toString().padLeft(5, '0');
  final parentEmail = 'e2e_parent_$uid@test.com';
  final kidEmail = 'e2e_kid_$uid@test.com';
  const password = 'Password123!';

  late String parentAccessToken;
  late String parentRefreshToken;
  late String parentUserId;
  late String kidUserId;
  late String invitationToken;

  group('1. Auth — Registration & Login flow', () {
    test('1.1 Register parent', () async {
      final api = createRealApiService();
      final resp = await api.post('auth/register/parent', body: {
        'email': parentEmail,
        'password': password,
        'firstName': 'E2EТест',
        'lastName': 'Родитель',
        'dateOfBirth': '1990-01-15',
        'familyName': 'E2EСемья_$uid',
      }, auth: false);

      expect(resp, isNotNull);
      expect(resp['accessToken'], isA<String>());
      expect(resp['refreshToken'], isA<String>());
      expect(resp['userId'], isA<String>());
      expect(resp['role'], 'Parent');
      expect(resp['email'], parentEmail);

      parentAccessToken = resp['accessToken'];
      parentRefreshToken = resp['refreshToken'];
      parentUserId = resp['userId'];
    });

    test('1.2 Generate kid invitation', () async {
      final api = createRealApiService();
      await api.saveTokens(parentAccessToken, parentRefreshToken);
      final resp = await api.post('families/invite', body: {
        'email': kidEmail,
        'role': 'Kid',
      });

      expect(resp, isNotNull);
      expect(resp['token'], isA<String>());
      invitationToken = resp['token'];
    });

    test('1.3 Register kid with invitation', () async {
      final api = createRealApiService();
      final resp = await api.post('auth/register/kid', body: {
        'invitationToken': invitationToken,
        'email': kidEmail,
        'password': password,
        'firstName': 'E2EТест',
        'lastName': 'Ребёнок',
        'dateOfBirth': '2013-06-20',
      }, auth: false);

      expect(resp, isNotNull);
      expect(resp['accessToken'], isA<String>());
      expect(resp['role'], 'Kid');
      kidUserId = resp['userId'];
    });

    test('1.4 Login as parent', () async {
      final api = createRealApiService();
      final resp = await api.post('auth/login', body: {
        'email': parentEmail,
        'password': password,
      }, auth: false);

      expect(resp, isNotNull);
      expect(resp['userId'], parentUserId);
      expect(resp['role'], 'Parent');
      parentAccessToken = resp['accessToken'];
      parentRefreshToken = resp['refreshToken'];
    });

    test('1.5 Login as kid', () async {
      final api = createRealApiService();
      final resp = await api.post('auth/login', body: {
        'email': kidEmail,
        'password': password,
      }, auth: false);

      expect(resp, isNotNull);
      expect(resp['userId'], kidUserId);
      expect(resp['role'], 'Kid');
    });

    test('1.6 Refresh token', () async {
      final api = createRealApiService();
      final resp = await api.post('auth/refresh', body: {
        'accessToken': parentAccessToken,
        'refreshToken': parentRefreshToken,
      }, auth: false);

      expect(resp, isNotNull);
      expect(resp['accessToken'], isA<String>());
      expect(resp['refreshToken'], isA<String>());
      parentAccessToken = resp['accessToken'];
      parentRefreshToken = resp['refreshToken'];
    });

    test('1.7 Login with wrong password returns 401', () async {
      final api = createRealApiService();
      expect(
        () => api.post('auth/login', body: {
          'email': parentEmail,
          'password': 'WrongPassword!',
        }, auth: false),
        throwsA(isA<ApiException>().having((e) => e.statusCode, 'status', 401)),
      );
    });

    test('1.8 Register duplicate email returns 409', () async {
      final api = createRealApiService();
      expect(
        () => api.post('auth/register/parent', body: {
          'email': parentEmail,
          'password': password,
          'firstName': 'Dup',
          'lastName': 'Dup',
          'dateOfBirth': '1990-01-01',
          'familyName': 'Dup',
        }, auth: false),
        throwsA(isA<ApiException>().having((e) => e.statusCode, 'status', 409)),
      );
    });

    test('1.9 Logout', () async {
      final api = createRealApiService();
      await api.saveTokens(parentAccessToken, parentRefreshToken);
      await api.post('auth/logout', body: {
        'refreshToken': parentRefreshToken,
      });

      final loginResp = await api.post('auth/login', body: {
        'email': parentEmail,
        'password': password,
      }, auth: false);
      parentAccessToken = loginResp['accessToken'];
      parentRefreshToken = loginResp['refreshToken'];
    });
  });
}
