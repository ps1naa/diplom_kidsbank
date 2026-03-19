import 'dart:math';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:integration_test/integration_test.dart';
import 'package:provider/provider.dart';
import 'package:kidbank_flutter/core/api_service.dart';
import 'package:kidbank_flutter/core/theme.dart';
import 'package:kidbank_flutter/core/router.dart';
import 'package:kidbank_flutter/providers/auth_provider.dart';
import 'package:kidbank_flutter/providers/theme_provider.dart';
import 'package:kidbank_flutter/screens/auth/login_screen.dart';
import 'package:kidbank_flutter/screens/parent/parent_shell.dart';
import 'package:kidbank_flutter/screens/kid/kid_shell.dart';

void main() {
  IntegrationTestWidgetsFlutterBinding.ensureInitialized();

  Widget buildApp() {
    final api = ApiService();
    final auth = AuthProvider(api);
    return MultiProvider(
      providers: [
        Provider<ApiService>.value(value: api),
        ChangeNotifierProvider<AuthProvider>.value(value: auth),
        ChangeNotifierProvider(create: (_) => ThemeProvider()),
      ],
      child: Consumer<ThemeProvider>(
        builder: (_, themeProv, __) => MaterialApp(
          title: 'KidBank Test',
          debugShowCheckedModeBanner: false,
          theme: AppTheme.lightTheme,
          darkTheme: AppTheme.darkTheme,
          themeMode: themeProv.mode,
          onGenerateRoute: AppRouter.generate,
          home: Consumer<AuthProvider>(
            builder: (_, auth, __) {
              switch (auth.status) {
                case AuthStatus.unknown:
                case AuthStatus.unauthenticated:
                  return const LoginScreen();
                case AuthStatus.authenticated:
                  return auth.isParent
                      ? const ParentShell()
                      : const KidShell();
              }
            },
          ),
        ),
      ),
    );
  }

  Future<void> settle(WidgetTester t, {int ms = 5000}) async {
    await t.pumpAndSettle(const Duration(milliseconds: 100),
        EnginePhase.sendSemanticsUpdate, Duration(milliseconds: ms));
  }

  Future<void> wait(WidgetTester t, int seconds) async {
    await t.pump(Duration(seconds: seconds));
    await settle(t, ms: seconds * 2000);
  }

  Future<void> tapByText(WidgetTester t, String text) async {
    final f = find.text(text);
    if (f.evaluate().isEmpty) return;
    await t.ensureVisible(f.first);
    await t.pumpAndSettle();
    await t.tap(f.first, warnIfMissed: false);
    await t.pumpAndSettle();
  }

  Future<void> tapButton(WidgetTester t, String text) async {
    final f = find.widgetWithText(ElevatedButton, text);
    if (f.evaluate().isEmpty) {
      final fb = find.widgetWithText(TextButton, text);
      if (fb.evaluate().isEmpty) return;
      await t.ensureVisible(fb.first);
      await t.pumpAndSettle();
      await t.tap(fb.first, warnIfMissed: false);
      await t.pumpAndSettle();
      return;
    }
    await t.ensureVisible(f.first);
    await t.pumpAndSettle();
    await t.tap(f.first, warnIfMissed: false);
    await t.pumpAndSettle();
  }

  Future<void> enterField(WidgetTester t, String label, String text) async {
    // Try TextFormField first, then TextField
    var f = find.widgetWithText(TextFormField, label);
    if (f.evaluate().isEmpty) {
      f = find.widgetWithText(TextField, label);
    }
    if (f.evaluate().isEmpty) return;
    await t.ensureVisible(f.first);
    await t.pumpAndSettle();
    await t.enterText(f.first, text);
    await t.pumpAndSettle();
  }

  Future<void> goBack(WidgetTester t) async {
    final back = find.byType(BackButton);
    if (back.evaluate().isNotEmpty) {
      await t.tap(back.first, warnIfMissed: false);
      await t.pumpAndSettle();
      return;
    }
    final nav = find.byTooltip('Back');
    if (nav.evaluate().isNotEmpty) {
      await t.tap(nav.first, warnIfMissed: false);
      await t.pumpAndSettle();
    }
  }

  Future<void> loginAs(WidgetTester t, String email, String password) async {
    await enterField(t, 'Email', email);
    await enterField(t, 'Пароль', password);
    await tapButton(t, 'Войти');
    await wait(t, 5);
  }

  Future<void> dismissOverlay(WidgetTester t) async {
    await t.tapAt(const Offset(10, 10));
    await settle(t);
  }

  // ============================================================
  // S1: Login screen validation
  // ============================================================
  testWidgets('S1: Login screen validation', (t) async {
    await t.pumpWidget(buildApp());
    await settle(t);

    expect(find.text('KidBank'), findsOneWidget);
    expect(find.text('Войти'), findsOneWidget);

    await tapButton(t, 'Войти');
    expect(find.textContaining('email'), findsWidgets);

    await enterField(t, 'Email', 'bad');
    await enterField(t, 'Пароль', '12');
    await tapButton(t, 'Войти');

    final vis = find.byIcon(Icons.visibility_off);
    if (vis.evaluate().isNotEmpty) {
      await t.tap(vis.first);
      await t.pumpAndSettle();
      expect(find.byIcon(Icons.visibility), findsOneWidget);
    }
  });

  // ============================================================
  // S2: Registration navigation
  // ============================================================
  testWidgets('S2: Registration navigation', (t) async {
    await t.pumpWidget(buildApp());
    await settle(t);

    await tapByText(t, 'Регистрация');
    expect(find.text('Выберите тип аккаунта'), findsOneWidget);

    await tapByText(t, 'Родитель');
    await settle(t);
    expect(find.text('Зарегистрироваться как родитель'), findsOneWidget);
    await goBack(t);

    await tapByText(t, 'Регистрация');
    await tapByText(t, 'Ребёнок');
    await settle(t);
    expect(find.text('Зарегистрироваться как ребёнок'), findsOneWidget);
    await goBack(t);
  });

  // ============================================================
  // S3: Register parent validation
  // ============================================================
  testWidgets('S3: Register parent validation', (t) async {
    await t.pumpWidget(buildApp());
    await settle(t);

    await tapByText(t, 'Регистрация');
    await tapByText(t, 'Родитель');
    await settle(t);

    final btn = find.widgetWithText(ElevatedButton, 'Создать аккаунт');
    await t.ensureVisible(btn);
    await t.pumpAndSettle();
    await t.tap(btn, warnIfMissed: false);
    await t.pumpAndSettle();
    expect(find.text('Обязательное'), findsWidgets);

    await enterField(t, 'Имя', 'Тест');
    await enterField(t, 'Фамилия', 'Тестов');
    await enterField(t, 'Email', 'test@test.com');
    await enterField(t, 'Пароль', 'Password123!');
    await enterField(t, 'Название семьи', 'Тестовая');

    await t.ensureVisible(btn);
    await t.pumpAndSettle();
    await t.tap(btn, warnIfMissed: false);
    await t.pumpAndSettle();
    final e1 = find.text('Необходимо принять все соглашения');
    final e2 = find.text('Укажите дату рождения');
    expect(e1.evaluate().isNotEmpty || e2.evaluate().isNotEmpty, isTrue);
  });

  // ============================================================
  // S4: Wrong credentials
  // ============================================================
  testWidgets('S4: Wrong credentials error', (t) async {
    await t.pumpWidget(buildApp());
    await settle(t);

    await loginAs(t, 'parent@kidbank.by', 'WrongPass!');
    expect(find.byType(LoginScreen), findsOneWidget);
  });

  // ============================================================
  // S5: Parent login + dashboard
  // ============================================================
  testWidgets('S5: Parent login + dashboard', (t) async {
    await t.pumpWidget(buildApp());
    await settle(t);
    await loginAs(t, 'parent@kidbank.by', 'Parent123!');

    expect(find.byType(ParentShell), findsOneWidget);
    expect(find.textContaining('Привет'), findsOneWidget);
    expect(find.text('Общий баланс'), findsOneWidget);
  });

  // ============================================================
  // S6: Parent dashboard — top up + quick actions
  // ============================================================
  testWidgets('S6: Parent dashboard actions', (t) async {
    await t.pumpWidget(buildApp());
    await settle(t);
    await loginAs(t, 'parent@kidbank.by', 'Parent123!');

    // Top up
    await tapByText(t, 'Пополнить');
    await settle(t);
    final amtField = find.widgetWithText(TextField, 'Сумма (BYN)');
    if (amtField.evaluate().isNotEmpty) {
      await t.enterText(amtField.first, '5');
      await t.pumpAndSettle();
      final topupBtn = find.widgetWithText(ElevatedButton, 'Пополнить');
      if (topupBtn.evaluate().isNotEmpty) {
        await t.tap(topupBtn.first, warnIfMissed: false);
        await wait(t, 3);
      }
    }
    await dismissOverlay(t);

    // Chat
    await tapByText(t, 'Чат');
    await wait(t, 2);
    if (find.text('Семейный чат').evaluate().isNotEmpty) {
      await goBack(t);
      await wait(t, 1);
    }

    // Pro
    await tapByText(t, 'Pro');
    await wait(t, 2);
    if (find.textContaining('Pro').evaluate().isNotEmpty &&
        find.byType(ParentShell).evaluate().isEmpty) {
      await goBack(t);
      await wait(t, 1);
    }
  });

  // ============================================================
  // S7: Parent — Kids tab
  // ============================================================
  testWidgets('S7: Parent kids tab', (t) async {
    await t.pumpWidget(buildApp());
    await settle(t);
    await loginAs(t, 'parent@kidbank.by', 'Parent123!');

    await tapByText(t, 'Дети');
    await wait(t, 3);
    expect(find.text('Мои дети'), findsOneWidget);

    final kidCards = find.textContaining('Уровень');
    expect(kidCards.evaluate().isNotEmpty, isTrue);

    // Tap on a kid card
    final chevrons = find.byIcon(Icons.chevron_right);
    if (chevrons.evaluate().isNotEmpty) {
      await t.tap(chevrons.first, warnIfMissed: false);
      await wait(t, 3);
      await goBack(t);
      await wait(t, 1);
    }
  });

  // ============================================================
  // S8: Parent — Cards tab (create + actions)
  // ============================================================
  testWidgets('S8: Parent cards tab', (t) async {
    await t.pumpWidget(buildApp());
    await settle(t);
    await loginAs(t, 'parent@kidbank.by', 'Parent123!');

    await tapByText(t, 'Карты');
    await wait(t, 3);
    expect(find.text('Мои карты'), findsOneWidget);

    // Create card via FAB
    final fab = find.byType(FloatingActionButton);
    if (fab.evaluate().isNotEmpty) {
      await t.tap(fab.first, warnIfMissed: false);
      await settle(t);
      final createBtn = find.text('Создать');
      if (createBtn.evaluate().isNotEmpty) {
        await t.tap(createBtn.first, warnIfMissed: false);
        await wait(t, 3);
      } else {
        await dismissOverlay(t);
      }
    }

    // Check we see some card content (masked number or "Нет карт")
    final hasCards = find.textContaining('••••').evaluate().isNotEmpty;
    final emptyState = find.text('Нет карт').evaluate().isNotEmpty;
    expect(hasCards || emptyState, isTrue);

    // If there are cards, try the actions
    if (hasCards) {
      final cardNum = find.textContaining('••••');
      await t.tap(cardNum.first, warnIfMissed: false);
      await settle(t);
      // Check for action buttons
      final freeze = find.text('Заморозить');
      final unfreeze = find.text('Разморозить');
      if (freeze.evaluate().isNotEmpty || unfreeze.evaluate().isNotEmpty) {
        await dismissOverlay(t);
      }
    }
  });

  // ============================================================
  // S9: Parent — Tasks tab (view tabs, create task)
  // ============================================================
  testWidgets('S9: Parent tasks tab', (t) async {
    await t.pumpWidget(buildApp());
    await settle(t);
    await loginAs(t, 'parent@kidbank.by', 'Parent123!');

    await tapByText(t, 'Задания');
    await wait(t, 3);

    // Check tabs
    await tapByText(t, 'На проверку');
    await settle(t);
    await tapByText(t, 'Все задания');
    await settle(t);
    await tapByText(t, 'Шаблоны');
    await settle(t);
    await tapByText(t, 'Все задания');
    await settle(t);

    // Create task
    final fab = find.byType(FloatingActionButton);
    if (fab.evaluate().isNotEmpty) {
      await t.tap(fab.first, warnIfMissed: false);
      await settle(t);

      final newTask = find.text('Новое задание');
      if (newTask.evaluate().isNotEmpty) {
        await t.tap(newTask.first, warnIfMissed: false);
        await settle(t);
        await wait(t, 2);

        // Fill fields (TextField, not TextFormField)
        await enterField(t, 'Название', 'UI тест задание');
        await enterField(t, 'Описание', 'Описание теста');
        await enterField(t, 'Награда (BYN)', '5');

        // Select kid from dropdown
        final dropdown = find.byType(DropdownButtonFormField<String>);
        if (dropdown.evaluate().isNotEmpty) {
          await t.tap(dropdown.first, warnIfMissed: false);
          await settle(t);
          // Tap first popup item
          final popupItems = find.byType(DropdownMenuItem<String>);
          if (popupItems.evaluate().isNotEmpty) {
            await t.tap(popupItems.last, warnIfMissed: false);
            await settle(t);
          }
        }

        // Create button should be enabled now
        final createBtn = find.widgetWithText(ElevatedButton, 'Создать');
        if (createBtn.evaluate().isNotEmpty) {
          await t.tap(createBtn.first, warnIfMissed: false);
          await wait(t, 3);
        }
      } else {
        await dismissOverlay(t);
      }
    }
  });

  // ============================================================
  // S10: Parent — Profile + menu navigation
  // ============================================================
  testWidgets('S10: Parent profile + menus', (t) async {
    await t.pumpWidget(buildApp());
    await settle(t);
    await loginAs(t, 'parent@kidbank.by', 'Parent123!');

    await tapByText(t, 'Профиль');
    await wait(t, 3);
    expect(find.text('Родитель'), findsOneWidget);

    // Activity
    await tapByText(t, 'Активность');
    await wait(t, 2);
    if (find.text('Активность').evaluate().isNotEmpty) {
      await goBack(t);
      await wait(t, 1);
    }

    // Notifications
    await tapByText(t, 'Уведомления');
    await wait(t, 2);
    if (find.text('Уведомления').evaluate().isNotEmpty) {
      await goBack(t);
      await wait(t, 1);
    }

    // Settings
    final settings = find.text('Настройки');
    if (settings.evaluate().isNotEmpty) {
      await t.ensureVisible(settings.first);
      await t.pumpAndSettle();
      await t.tap(settings.first, warnIfMissed: false);
      await wait(t, 2);
      await goBack(t);
      await wait(t, 1);
    }

    // Chat
    final chat = find.text('Семейный чат');
    if (chat.evaluate().isNotEmpty) {
      await t.ensureVisible(chat.first);
      await t.pumpAndSettle();
      await t.tap(chat.first, warnIfMissed: false);
      await wait(t, 2);
      await goBack(t);
      await wait(t, 1);
    }

    // Money Requests
    final mr = find.text('Запросы денег');
    if (mr.evaluate().isNotEmpty) {
      await t.ensureVisible(mr.first);
      await t.pumpAndSettle();
      await t.tap(mr.first, warnIfMissed: false);
      await wait(t, 2);
      await goBack(t);
      await wait(t, 1);
    }

    // Allowance
    final al = find.text('Авто-пополнение');
    if (al.evaluate().isNotEmpty) {
      await t.ensureVisible(al.first);
      await t.pumpAndSettle();
      await t.tap(al.first, warnIfMissed: false);
      await wait(t, 2);
      await goBack(t);
      await wait(t, 1);
    }

    // Subscription
    final sub = find.text('Подписка Pro');
    if (sub.evaluate().isNotEmpty) {
      await t.ensureVisible(sub.first);
      await t.pumpAndSettle();
      await t.tap(sub.first, warnIfMissed: false);
      await wait(t, 2);
      await goBack(t);
      await wait(t, 1);
    }
  });

  // ============================================================
  // S11: Parent chat — send message
  // ============================================================
  testWidgets('S11: Parent chat', (t) async {
    await t.pumpWidget(buildApp());
    await settle(t);
    await loginAs(t, 'parent@kidbank.by', 'Parent123!');

    await tapByText(t, 'Чат');
    await wait(t, 3);
    expect(find.text('Семейный чат'), findsOneWidget);

    final msgField = find.widgetWithText(TextField, 'Сообщение...');
    if (msgField.evaluate().isNotEmpty) {
      await t.enterText(msgField.first, 'Тест сообщение от родителя');
      await t.pumpAndSettle();
      final send = find.byIcon(Icons.send);
      if (send.evaluate().isNotEmpty) {
        await t.tap(send.first, warnIfMissed: false);
        await wait(t, 3);
      }
    }
    await goBack(t);
  });

  // ============================================================
  // S12: Parent — Notifications
  // ============================================================
  testWidgets('S12: Parent notifications', (t) async {
    await t.pumpWidget(buildApp());
    await settle(t);
    await loginAs(t, 'parent@kidbank.by', 'Parent123!');

    final bell = find.byIcon(Icons.notifications_outlined);
    if (bell.evaluate().isNotEmpty) {
      await t.tap(bell.first, warnIfMissed: false);
      await wait(t, 3);
      expect(find.text('Уведомления'), findsOneWidget);

      final markAll = find.text('Прочитать все');
      if (markAll.evaluate().isNotEmpty) {
        await t.tap(markAll.first, warnIfMissed: false);
        await wait(t, 2);
      }
      await goBack(t);
    }
  });

  // ============================================================
  // S13: Full parent registration (new user)
  // ============================================================
  testWidgets('S13: Register new parent', (t) async {
    final uid = Random().nextInt(99999).toString().padLeft(5, '0');
    await t.pumpWidget(buildApp());
    await settle(t);

    await tapByText(t, 'Регистрация');
    await tapByText(t, 'Родитель');
    await settle(t);

    await enterField(t, 'Имя', 'UIТест');
    await enterField(t, 'Фамилия', 'Родитель');
    await enterField(t, 'Email', 'ui_reg_$uid@test.com');
    await enterField(t, 'Пароль', 'Password123!');

    final dateField = find.widgetWithText(TextFormField, 'Дата рождения');
    if (dateField.evaluate().isNotEmpty) {
      await t.ensureVisible(dateField.first);
      await t.pumpAndSettle();
      await t.tap(dateField.first, warnIfMissed: false);
      await t.pumpAndSettle();
      final ok = find.text('OK');
      if (ok.evaluate().isNotEmpty) {
        await t.tap(ok.first);
        await t.pumpAndSettle();
      }
    }

    await enterField(t, 'Название семьи', 'UIСемья_$uid');

    final checks = find.byType(Checkbox);
    for (var i = 0; i < checks.evaluate().length; i++) {
      await t.ensureVisible(checks.at(i));
      await t.pumpAndSettle();
      await t.tap(checks.at(i), warnIfMissed: false);
      await t.pumpAndSettle();
    }

    final createBtn = find.widgetWithText(ElevatedButton, 'Создать аккаунт');
    await t.ensureVisible(createBtn);
    await t.pumpAndSettle();
    await t.tap(createBtn, warnIfMissed: false);
    await wait(t, 8);

    expect(find.byType(ParentShell), findsOneWidget);
  });

  // ============================================================
  // S14: Kid login + dashboard
  // ============================================================
  testWidgets('S14: Kid login + dashboard', (t) async {
    await t.pumpWidget(buildApp());
    await settle(t);
    await loginAs(t, 'kid1@kidbank.by', 'Kid12345!');

    expect(find.byType(KidShell), findsOneWidget);
    expect(find.textContaining('Привет'), findsOneWidget);
    expect(find.text('Мой баланс'), findsOneWidget);
  });

  // ============================================================
  // S15: Kid dashboard — quick actions
  // ============================================================
  testWidgets('S15: Kid dashboard actions', (t) async {
    await t.pumpWidget(buildApp());
    await settle(t);
    await loginAs(t, 'kid1@kidbank.by', 'Kid12345!');

    // Cards
    await tapByText(t, 'Карты');
    await wait(t, 2);
    if (find.text('Мои карты').evaluate().isNotEmpty) {
      await goBack(t);
      await wait(t, 1);
    }

    // Savings
    await tapByText(t, 'Копилка');
    await wait(t, 2);
    if (find.text('Копилка').evaluate().isNotEmpty) {
      await goBack(t);
      await wait(t, 1);
    }

    // Chat
    await tapByText(t, 'Чат');
    await wait(t, 2);
    if (find.text('Семейный чат').evaluate().isNotEmpty) {
      await goBack(t);
      await wait(t, 1);
    }

    // Goals
    await tapByText(t, 'Цели');
    await wait(t, 2);
    if (find.textContaining('цели').evaluate().isNotEmpty) {
      await goBack(t);
      await wait(t, 1);
    }

    // Leaderboard
    await tapByText(t, 'Рейтинг');
    await wait(t, 2);
    if (find.text('Семейный рейтинг').evaluate().isNotEmpty) {
      await goBack(t);
      await wait(t, 1);
    }

    // Categories
    await tapByText(t, 'Категории');
    await wait(t, 2);
    if (find.text('Категории расходов').evaluate().isNotEmpty) {
      await goBack(t);
      await wait(t, 1);
    }
  });

  // ============================================================
  // S16: Kid — Tasks tab
  // ============================================================
  testWidgets('S16: Kid tasks tab', (t) async {
    await t.pumpWidget(buildApp());
    await settle(t);
    await loginAs(t, 'kid1@kidbank.by', 'Kid12345!');

    await tapByText(t, 'Задания');
    await wait(t, 3);

    await tapByText(t, 'Активные');
    await settle(t);
    await tapByText(t, 'На проверке');
    await settle(t);
    await tapByText(t, 'Завершённые');
    await settle(t);

    await tapByText(t, 'Активные');
    await settle(t);
    final complete = find.text('Отметить выполненным');
    if (complete.evaluate().isNotEmpty) {
      await t.ensureVisible(complete.first);
      await t.pumpAndSettle();
      await t.tap(complete.first, warnIfMissed: false);
      await wait(t, 3);
    }
  });

  // ============================================================
  // S17: Kid — Education tab
  // ============================================================
  testWidgets('S17: Kid education tab', (t) async {
    await t.pumpWidget(buildApp());
    await settle(t);
    await loginAs(t, 'kid1@kidbank.by', 'Kid12345!');

    await tapByText(t, 'Учёба');
    await wait(t, 3);

    await tapByText(t, 'Квизы');
    await settle(t);
    await tapByText(t, 'Прогресс');
    await settle(t);
    await tapByText(t, 'Достижения');
    await settle(t);
    await tapByText(t, 'Квизы');
    await settle(t);
  });

  // ============================================================
  // S18: Kid — Profile + menu navigation
  // ============================================================
  testWidgets('S18: Kid profile + menus', (t) async {
    await t.pumpWidget(buildApp());
    await settle(t);
    await loginAs(t, 'kid1@kidbank.by', 'Kid12345!');

    await tapByText(t, 'Профиль');
    await wait(t, 3);

    // Cards
    await tapByText(t, 'Мои карты');
    await wait(t, 2);
    await goBack(t);
    await wait(t, 1);

    // Savings
    final sav = find.text('Копилка');
    if (sav.evaluate().isNotEmpty) {
      await t.ensureVisible(sav.first);
      await t.pumpAndSettle();
      await t.tap(sav.first, warnIfMissed: false);
      await wait(t, 2);
      await goBack(t);
      await wait(t, 1);
    }

    // Money Requests
    final mr = find.text('Запросы денег');
    if (mr.evaluate().isNotEmpty) {
      await t.ensureVisible(mr.first);
      await t.pumpAndSettle();
      await t.tap(mr.first, warnIfMissed: false);
      await wait(t, 2);
      await goBack(t);
      await wait(t, 1);
    }

    // Leaderboard
    final lb = find.text('Рейтинг');
    if (lb.evaluate().isNotEmpty) {
      await t.ensureVisible(lb.first);
      await t.pumpAndSettle();
      await t.tap(lb.first, warnIfMissed: false);
      await wait(t, 2);
      await goBack(t);
      await wait(t, 1);
    }

    // Chat
    final chat = find.text('Семейный чат');
    if (chat.evaluate().isNotEmpty) {
      await t.ensureVisible(chat.first);
      await t.pumpAndSettle();
      await t.tap(chat.first, warnIfMissed: false);
      await wait(t, 2);
      await goBack(t);
      await wait(t, 1);
    }

    // Notifications
    final notif = find.text('Уведомления');
    if (notif.evaluate().isNotEmpty) {
      await t.ensureVisible(notif.first);
      await t.pumpAndSettle();
      await t.tap(notif.first, warnIfMissed: false);
      await wait(t, 2);
      await goBack(t);
      await wait(t, 1);
    }

    // Settings
    final settings = find.text('Настройки');
    if (settings.evaluate().isNotEmpty) {
      await t.ensureVisible(settings.first);
      await t.pumpAndSettle();
      await t.tap(settings.first, warnIfMissed: false);
      await wait(t, 2);

      // Toggle themes
      await tapByText(t, 'Тёмная тема');
      await settle(t);
      await tapByText(t, 'Светлая тема');
      await settle(t);

      await goBack(t);
      await wait(t, 1);
    }
  });

  // ============================================================
  // S19: Kid — Goals CRUD
  // ============================================================
  testWidgets('S19: Kid goals CRUD', (t) async {
    await t.pumpWidget(buildApp());
    await settle(t);
    await loginAs(t, 'kid1@kidbank.by', 'Kid12345!');

    await tapByText(t, 'Цели');
    await wait(t, 3);

    // Create goal
    final fab = find.byType(FloatingActionButton);
    if (fab.evaluate().isNotEmpty) {
      await t.tap(fab.first, warnIfMissed: false);
      await settle(t);

      await enterField(t, 'Название', 'UI Тест Цель');
      await enterField(t, 'Целевая сумма (BYN)', '100');
      await tapButton(t, 'Создать цель');
      await wait(t, 3);
    }

    // Deposit
    final deposit = find.text('Пополнить');
    if (deposit.evaluate().isNotEmpty) {
      await t.ensureVisible(deposit.first);
      await t.pumpAndSettle();
      await t.tap(deposit.first, warnIfMissed: false);
      await settle(t);

      final amtField = find.widgetWithText(TextField, 'Сумма (BYN)');
      if (amtField.evaluate().isNotEmpty) {
        await t.enterText(amtField.first, '1');
        await t.pumpAndSettle();
        final btn = find.widgetWithText(ElevatedButton, 'Пополнить');
        if (btn.evaluate().isNotEmpty) {
          await t.tap(btn.first, warnIfMissed: false);
          await wait(t, 3);
        }
      }
    }

    await goBack(t);
  });

  // ============================================================
  // S20: Kid — Savings
  // ============================================================
  testWidgets('S20: Kid savings', (t) async {
    await t.pumpWidget(buildApp());
    await settle(t);
    await loginAs(t, 'kid1@kidbank.by', 'Kid12345!');

    await tapByText(t, 'Копилка');
    await wait(t, 3);

    // Create savings
    final fab = find.byType(FloatingActionButton);
    if (fab.evaluate().isNotEmpty) {
      await t.tap(fab.first, warnIfMissed: false);
      await settle(t);

      final nameField = find.widgetWithText(TextField, 'Название');
      if (nameField.evaluate().isNotEmpty) {
        await t.enterText(nameField.first, 'UI Тест Копилка');
        await t.pumpAndSettle();
        await tapButton(t, 'Создать');
        await wait(t, 3);
      }
    }

    // Deposit
    final deposit = find.text('Пополнить');
    if (deposit.evaluate().isNotEmpty) {
      await t.ensureVisible(deposit.first);
      await t.pumpAndSettle();
      await t.tap(deposit.first, warnIfMissed: false);
      await settle(t);

      final amtField = find.widgetWithText(TextField, 'Сумма (BYN)');
      if (amtField.evaluate().isNotEmpty) {
        await t.enterText(amtField.first, '1');
        await t.pumpAndSettle();
        final btn = find.widgetWithText(ElevatedButton, 'Пополнить');
        if (btn.evaluate().isNotEmpty) {
          await t.tap(btn.first, warnIfMissed: false);
          await wait(t, 3);
        }
      }
    }

    await goBack(t);
  });

  // ============================================================
  // S21: Kid — Money Request
  // ============================================================
  testWidgets('S21: Kid money request', (t) async {
    await t.pumpWidget(buildApp());
    await settle(t);
    await loginAs(t, 'kid1@kidbank.by', 'Kid12345!');

    await tapByText(t, 'Запросить');
    await wait(t, 3);
    expect(find.text('Мои запросы'), findsOneWidget);

    final fab = find.byType(FloatingActionButton);
    if (fab.evaluate().isNotEmpty) {
      await t.tap(fab.first, warnIfMissed: false);
      await settle(t);

      await enterField(t, 'Сумма (BYN)', '10');
      await enterField(t, 'Причина', 'UI тест запрос');
      await tapButton(t, 'Отправить запрос');
      await wait(t, 3);
    }

    await goBack(t);
  });

  // ============================================================
  // S22: Kid — Cards
  // ============================================================
  testWidgets('S22: Kid cards', (t) async {
    await t.pumpWidget(buildApp());
    await settle(t);
    await loginAs(t, 'kid1@kidbank.by', 'Kid12345!');

    await tapByText(t, 'Карты');
    await wait(t, 3);
    expect(find.text('Мои карты'), findsOneWidget);

    final fab = find.byType(FloatingActionButton);
    if (fab.evaluate().isNotEmpty) {
      await t.tap(fab.first, warnIfMissed: false);
      await settle(t);
      final create = find.text('Создать');
      if (create.evaluate().isNotEmpty) {
        await t.tap(create.first, warnIfMissed: false);
        await wait(t, 3);
      } else {
        await dismissOverlay(t);
      }
    }

    // Tap on card for actions
    final cardNum = find.textContaining('••••');
    if (cardNum.evaluate().isNotEmpty) {
      await t.tap(cardNum.first, warnIfMissed: false);
      await settle(t);

      final showData = find.text('Показать данные карты');
      if (showData.evaluate().isNotEmpty) {
        await t.tap(showData.first, warnIfMissed: false);
        await settle(t);
        final close = find.text('Закрыть');
        if (close.evaluate().isNotEmpty) {
          await t.tap(close.first, warnIfMissed: false);
          await settle(t);
        }
      } else {
        await dismissOverlay(t);
      }
    }

    await goBack(t);
  });

  // ============================================================
  // S23: Kid — Chat
  // ============================================================
  testWidgets('S23: Kid chat', (t) async {
    await t.pumpWidget(buildApp());
    await settle(t);
    await loginAs(t, 'kid1@kidbank.by', 'Kid12345!');

    await tapByText(t, 'Чат');
    await wait(t, 3);
    expect(find.text('Семейный чат'), findsOneWidget);

    final msgField = find.widgetWithText(TextField, 'Сообщение...');
    if (msgField.evaluate().isNotEmpty) {
      await t.enterText(msgField.first, 'Привет от ребёнка (UI тест)');
      await t.pumpAndSettle();
      final send = find.byIcon(Icons.send);
      if (send.evaluate().isNotEmpty) {
        await t.tap(send.first, warnIfMissed: false);
        await wait(t, 3);
      }
    }

    await goBack(t);
  });

  // ============================================================
  // S24: Kid — Categories
  // ============================================================
  testWidgets('S24: Kid categories', (t) async {
    await t.pumpWidget(buildApp());
    await settle(t);
    await loginAs(t, 'kid1@kidbank.by', 'Kid12345!');

    await tapByText(t, 'Категории');
    await wait(t, 3);
    expect(find.text('Категории расходов'), findsOneWidget);
    await goBack(t);
  });

  // ============================================================
  // S25: Parent — Allowance setup
  // ============================================================
  testWidgets('S25: Parent allowance', (t) async {
    await t.pumpWidget(buildApp());
    await settle(t);
    await loginAs(t, 'parent@kidbank.by', 'Parent123!');

    await tapByText(t, 'Профиль');
    await wait(t, 2);

    final al = find.text('Авто-пополнение');
    if (al.evaluate().isNotEmpty) {
      await t.ensureVisible(al.first);
      await t.pumpAndSettle();
      await t.tap(al.first, warnIfMissed: false);
      await wait(t, 3);

      final config = find.text('Настроить');
      if (config.evaluate().isNotEmpty) {
        await t.tap(config.first, warnIfMissed: false);
        await settle(t);
        await enterField(t, 'Сумма (BYN)', '10');
        await tapButton(t, 'Сохранить');
        await wait(t, 3);
      }

      await goBack(t);
    }
  });

  // ============================================================
  // S26: Parent — Money Requests
  // ============================================================
  testWidgets('S26: Parent money requests', (t) async {
    await t.pumpWidget(buildApp());
    await settle(t);
    await loginAs(t, 'parent@kidbank.by', 'Parent123!');

    await tapByText(t, 'Профиль');
    await wait(t, 2);

    final mr = find.text('Запросы денег');
    if (mr.evaluate().isNotEmpty) {
      await t.ensureVisible(mr.first);
      await t.pumpAndSettle();
      await t.tap(mr.first, warnIfMissed: false);
      await wait(t, 3);

      final approve = find.text('Одобрить');
      if (approve.evaluate().isNotEmpty) {
        await t.tap(approve.first, warnIfMissed: false);
        await settle(t);
        final confirm = find.widgetWithText(TextButton, 'Одобрить');
        if (confirm.evaluate().isNotEmpty) {
          await t.tap(confirm.last, warnIfMissed: false);
          await wait(t, 3);
        }
      }

      await goBack(t);
    }
  });

  // ============================================================
  // S27: Subscription screen
  // ============================================================
  testWidgets('S27: Subscription', (t) async {
    await t.pumpWidget(buildApp());
    await settle(t);
    await loginAs(t, 'parent@kidbank.by', 'Parent123!');

    await tapByText(t, 'Pro');
    await wait(t, 2);

    final subBtn = find.widgetWithText(ElevatedButton, 'Оформить подписку');
    if (subBtn.evaluate().isNotEmpty) {
      await t.tap(subBtn.first, warnIfMissed: false);
      await settle(t);
    }

    await goBack(t);
  });

  // ============================================================
  // S28: Logout/Login cycle
  // ============================================================
  testWidgets('S28: Logout cycle', (t) async {
    await t.pumpWidget(buildApp());
    await settle(t);
    await loginAs(t, 'parent@kidbank.by', 'Parent123!');

    expect(find.byType(ParentShell), findsOneWidget);

    // Go to profile and logout
    await tapByText(t, 'Профиль');
    await wait(t, 2);
    final logout = find.text('Выйти');
    expect(logout.evaluate().isNotEmpty, isTrue, reason: 'Logout button should be visible');
    await t.ensureVisible(logout.first);
    await t.pumpAndSettle(const Duration(seconds: 10));
    await t.tap(logout.first, warnIfMissed: false);
    await t.pumpAndSettle(const Duration(seconds: 15));

    // Wait for async logout to complete and UI to rebuild
    for (var i = 0; i < 10; i++) {
      await t.pump(const Duration(seconds: 1));
      await settle(t, ms: 3000);
      if (find.text('Войти').evaluate().isNotEmpty) break;
    }

    expect(find.text('Войти'), findsOneWidget);

    // Login as kid
    await loginAs(t, 'kid1@kidbank.by', 'Kid12345!');
    expect(find.byType(KidShell), findsOneWidget);

    // Logout kid
    await tapByText(t, 'Профиль');
    await wait(t, 2);
    final logout2 = find.text('Выйти');
    expect(logout2.evaluate().isNotEmpty, isTrue);
    await t.ensureVisible(logout2.first);
    await t.pumpAndSettle(const Duration(seconds: 10));
    await t.tap(logout2.first, warnIfMissed: false);
    await t.pumpAndSettle(const Duration(seconds: 15));

    for (var i = 0; i < 10; i++) {
      await t.pump(const Duration(seconds: 1));
      await settle(t, ms: 3000);
      if (find.text('Войти').evaluate().isNotEmpty) break;
    }

    expect(find.text('Войти'), findsOneWidget);
  });
}
