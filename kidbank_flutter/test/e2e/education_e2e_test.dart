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

  group('14. Education', () {
    late String lessonId;

    test('14.1 GET education/lessons', () async {
      final lessons = await kidApi.get('education/lessons') as List;
      expect(lessons, isNotEmpty);
      expect(lessons.first['title'], isA<String>());
      lessonId = lessons.first['id'];
    });

    test('14.2 GET education/lessons/{id}', () async {
      final lesson = await kidApi.get('education/lessons/$lessonId');
      expect(lesson, isNotNull);
      expect(lesson['title'], isA<String>());
      expect(lesson['content'], isA<String>());
    });

    test('14.3 GET education/progress', () async {
      final resp = await kidApi.get('education/progress');
      expect(resp, isNotNull);
    });

    test('14.4 GET education/missions', () async {
      final missions = await kidApi.get('education/missions') as List;
      expect(missions, isNotEmpty);
      expect(missions.first['title'], isA<String>());
    });

    test('14.5 GET education/missions/{id}', () async {
      final missions = await kidApi.get('education/missions') as List;
      final missionId = missions.first['id'];
      final mission = await kidApi.get('education/missions/$missionId');
      expect(mission, isNotNull);
      expect(mission['title'], isA<String>());
    });

    test('14.6 POST education/quiz/submit', () async {
      final lesson = await kidApi.get('education/lessons/$lessonId');
      if (lesson['quizzes'] != null && (lesson['quizzes'] as List).isNotEmpty) {
        final quiz = (lesson['quizzes'] as List).first;
        final resp = await kidApi.post('education/quiz/submit', body: {
          'quizId': quiz['id'],
          'selectedOptionIndex': 0,
        });
        expect(resp, isNotNull);
      }
    });

    test('14.7 POST education/streak/update', () async {
      await kidApi.post('education/streak/update');
    });

    test('14.8 POST education/xp/add — parent adds XP', () async {
      await parentApi.post('education/xp/add', body: {
        'userId': kidId,
        'amount': 5,
      });
    });

    test('14.9 POST education/achievement/unlock', () async {
      final achievements = await kidApi.get('achievements') as List;
      if (achievements.isNotEmpty) {
        final code = achievements.first['code'];
        try {
          await kidApi.post('education/achievement/unlock', body: {
            'achievementCode': code,
          });
        } on ApiException catch (e) {
          // 409 = already unlocked, which is fine
          expect(e.statusCode, anyOf(200, 201, 409));
        }
      }
    });
  });

  group('15. Achievements', () {
    test('15.1 GET achievements — all definitions', () async {
      final list = await kidApi.get('achievements') as List;
      expect(list, isNotEmpty);
      expect(list.first['code'], isA<String>());
      expect(list.first['title'], isA<String>());
    });

    test('15.2 GET achievements/my — kid progress', () async {
      final list = await kidApi.get('achievements/my') as List;
      expect(list, isA<List>());
    });
  });
}
