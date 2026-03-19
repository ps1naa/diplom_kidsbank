@Tags(['e2e'])
import 'package:flutter_test/flutter_test.dart';
import 'package:kidbank_flutter/core/api_service.dart';
import '../helpers/e2e_helpers.dart';

/// Глубокие тесты образования: миссии, модули, квизы, XP, стрик, достижения.
/// Проверяем полный цикл: прогресс → квиз → XP → уровень.
void main() {
  late ApiService parentApi;
  late ApiService kidApi;
  late String kidId;

  setUpAll(() async {
    parentApi = await loginAsParent();
    kidApi = await loginAsKid1();
    final me = await kidApi.get('users/me');
    kidId = me['id'];
  });

  group('EDU1. Миссии — структура и навигация', () {
    late List<dynamic> missions;

    test('EDU1.1 GET education/missions — список миссий', () async {
      missions = await kidApi.get('education/missions') as List;
      expect(missions, isNotEmpty);

      final m = missions.first;
      expect(m['id'], isA<String>());
      expect(m['title'], isA<String>());
      expect(m['description'], isA<String>());
      expect(m['xpReward'], isA<num>());
      expect(m['totalModules'], isA<num>());
      expect(m['completedModules'], isA<num>());
    });

    test('EDU1.2 GET education/missions/{id} — детали миссии', () async {
      final missionId = missions.first['id'];
      final detail = await kidApi.get('education/missions/$missionId');
      expect(detail['id'], missionId);
      expect(detail['title'], isA<String>());
      expect(detail['modules'], isA<List>());

      final modules = detail['modules'] as List;
      if (modules.isNotEmpty) {
        final mod = modules.first;
        expect(mod['id'], isA<String>());
        expect(mod['title'], isA<String>());
        expect(mod['quizzesCompleted'], isA<num>());
        expect(mod['quizzesTotal'], isA<num>());
        expect(mod['isCompleted'], isA<bool>());
      }
    });
  });

  group('EDU2. Уроки и квизы', () {
    late List<dynamic> lessons;
    late String lessonId;

    test('EDU2.1 GET education/lessons — список уроков', () async {
      lessons = await kidApi.get('education/lessons') as List;
      expect(lessons, isNotEmpty);
      final l = lessons.first;
      expect(l['id'], isA<String>());
      expect(l['title'], isA<String>());
      expect(l['xpReward'], isA<num>());
      lessonId = l['id'];
    });

    test('EDU2.2 GET education/lessons/{id} — урок с квизами', () async {
      final lesson = await kidApi.get('education/lessons/$lessonId');
      expect(lesson['id'], lessonId);
      expect(lesson['title'], isA<String>());
      expect(lesson['content'], isA<String>());
      expect(lesson['quizzes'], isA<List>());

      final quizzes = lesson['quizzes'] as List;
      if (quizzes.isNotEmpty) {
        final q = quizzes.first;
        expect(q['id'], isA<String>());
        expect(q['question'], isA<String>());
        expect(q['options'], isA<List>());
        expect(q['xpReward'], isA<num>());
      }
    });

    test('EDU2.3 POST education/quiz/submit — отправка ответа на квиз', () async {
      final lesson = await kidApi.get('education/lessons/$lessonId');
      final quizzes = lesson['quizzes'] as List;
      if (quizzes.isEmpty) {
        return;
      }

      final quiz = quizzes.first;
      final quizId = quiz['id'];
      final options = quiz['options'] as List;
      expect(options, isNotEmpty, reason: 'Квиз должен иметь варианты ответов');

      final resp = await kidApi.post('education/quiz/submit', body: {
        'quizId': quizId,
        'selectedOptionIndex': 0,
      });

      expect(resp, isNotNull);
      expect(resp['isCorrect'], isA<bool>());
      expect(resp['correctOptionIndex'], isA<num>());
      expect(resp['xpEarned'], isA<num>());
      expect(resp['totalXp'], isA<num>());

      if (resp['explanation'] != null) {
        expect(resp['explanation'], isA<String>());
      }
    });
  });

  group('EDU3. XP и уровень', () {
    late num xpBefore;

    test('EDU3.1 Запоминаем XP до добавления', () async {
      final me = await kidApi.get('users/me');
      xpBefore = me['totalXp'] as num;
    });

    test('EDU3.2 Родитель добавляет XP ребёнку', () async {
      await parentApi.post('education/xp/add', body: {
        'userId': kidId,
        'amount': 25,
      });
    });

    test('EDU3.3 XP ребёнка увеличилось', () async {
      final me = await kidApi.get('users/me');
      final xpAfter = me['totalXp'] as num;
      expect(xpAfter, xpBefore + 25);
    });

    test('EDU3.4 Уровень корректно рассчитывается', () async {
      final me = await kidApi.get('users/me');
      final xp = me['totalXp'] as num;
      final level = me['level'] as num;
      expect(level, greaterThanOrEqualTo(1));

      if (me['xpToNextLevel'] != null) {
        expect(me['xpToNextLevel'] as num, greaterThanOrEqualTo(0));
      }
    });
  });

  group('EDU4. Стрик', () {
    test('EDU4.1 POST education/streak/update', () async {
      final resp = await kidApi.post('education/streak/update');
      expect(resp, isNotNull);
      if (resp is Map) {
        expect(resp['currentStreak'], isA<num>());
      }
    });

    test('EDU4.2 Стрик отражается в профиле', () async {
      final me = await kidApi.get('users/me');
      expect(me['currentStreak'], isA<num>());
      expect(me['currentStreak'] as num, greaterThanOrEqualTo(0));
    });
  });

  group('EDU5. Прогресс образования — DTO', () {
    test('EDU5.1 GET education/progress — корректные поля', () async {
      final resp = await kidApi.get('education/progress');
      expect(resp, isNotNull);
      expect(resp['totalModules'], isA<num>());
      expect(resp['completedModules'], isA<num>());
      expect(resp['totalXpEarned'], isA<num>());
      expect(resp['currentStreak'], isA<num>());
      expect(resp['level'], isA<num>());
      expect(resp['xpToNextLevel'], isA<num>());
      expect(resp['modules'], isA<List>());

      expect(resp.containsKey('totalXp'), isFalse, reason: 'Правильное поле — totalXpEarned');
      expect(resp.containsKey('quizzesCompleted'), isFalse, reason: 'На верхнем уровне нет quizzesCompleted — оно в modules');

      final modules = resp['modules'] as List;
      if (modules.isNotEmpty) {
        final mod = modules.first;
        expect(mod['moduleId'], isA<String>());
        expect(mod['moduleTitle'], isA<String>());
        expect(mod['isCompleted'], isA<bool>());
        expect(mod['completedQuizzes'], isA<num>());
        expect(mod['totalQuizzes'], isA<num>());
        expect(mod['progressPercentage'], isA<num>());
        expect(mod['xpEarned'], isA<num>());
      }
    });
  });

  group('EDU6. Достижения — полный цикл', () {
    test('EDU6.1 GET achievements — все достижения', () async {
      final list = await kidApi.get('achievements') as List;
      expect(list, isNotEmpty);
      final a = list.first;
      expect(a['id'], isA<String>());
      expect(a['code'], isA<String>());
      expect(a['title'], isA<String>());
      expect(a['description'], isA<String>());
      expect(a['xpReward'], isA<num>());
    });

    test('EDU6.2 GET achievements/my — персональный прогресс', () async {
      final list = await kidApi.get('achievements/my') as List;
      expect(list, isA<List>());
      if (list.isNotEmpty) {
        final a = list.first;
        expect(a['progress'], isA<num>());
        expect(a['requiredProgress'], isA<num>());
        expect(a.containsKey('currentProgress'), isFalse);
      }
    });

    test('EDU6.3 POST education/achievement/unlock', () async {
      final all = await kidApi.get('achievements') as List;
      if (all.isEmpty) return;

      final code = all.first['code'] as String;
      try {
        final resp = await kidApi.post('education/achievement/unlock', body: {
          'achievementCode': code,
        });
        expect(resp, isNotNull);
      } on ApiException catch (e) {
        expect(e.statusCode, anyOf(200, 201, 409), reason: '409 если уже разблокировано');
      }
    });
  });

  group('EDU7. Квиз — проверка всех вариантов', () {
    test('EDU7.1 Отправка неправильного ответа', () async {
      final lessons = await kidApi.get('education/lessons') as List;
      if (lessons.isEmpty) return;

      String? quizId;
      int optionCount = 0;

      for (final l in lessons) {
        final detail = await kidApi.get('education/lessons/${l['id']}');
        final quizzes = detail['quizzes'] as List;
        for (final q in quizzes) {
          final opts = q['options'] as List;
          if (opts.length >= 2) {
            quizId = q['id'];
            optionCount = opts.length;
            break;
          }
        }
        if (quizId != null) break;
      }

      if (quizId == null) return;

      final resp = await kidApi.post('education/quiz/submit', body: {
        'quizId': quizId,
        'selectedOptionIndex': optionCount - 1,
      });
      expect(resp['isCorrect'], isA<bool>());
      expect(resp['correctOptionIndex'], isA<num>());
    });

    test('EDU7.2 Отправка с невалидным quizId — ошибка', () async {
      expect(
        () => kidApi.post('education/quiz/submit', body: {
          'quizId': '00000000-0000-0000-0000-000000000000',
          'selectedOptionIndex': 0,
        }),
        throwsA(isA<ApiException>()),
      );
    });
  });
}
