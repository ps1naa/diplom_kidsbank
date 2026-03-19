import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:provider/provider.dart';
import 'core/api_service.dart';
import 'core/theme.dart';
import 'providers/auth_provider.dart';
import 'providers/theme_provider.dart';
import 'core/router.dart';
import 'screens/auth/login_screen.dart';
import 'screens/parent/parent_shell.dart';
import 'screens/kid/kid_shell.dart';

void main() {
  WidgetsFlutterBinding.ensureInitialized();
  SystemChrome.setPreferredOrientations([DeviceOrientation.portraitUp]);

  final apiService = ApiService();

  runApp(
    MultiProvider(
      providers: [
        Provider<ApiService>.value(value: apiService),
        ChangeNotifierProvider(create: (_) => AuthProvider(apiService)),
        ChangeNotifierProvider(create: (_) => ThemeProvider()),
      ],
      child: const KidBankApp(),
    ),
  );
}

class KidBankApp extends StatefulWidget {
  const KidBankApp({super.key});
  @override
  State<KidBankApp> createState() => _KidBankAppState();
}

class _KidBankAppState extends State<KidBankApp> {
  @override
  void initState() {
    super.initState();
    Future.microtask(() => context.read<AuthProvider>().checkAuth());
  }

  @override
  Widget build(BuildContext context) {
    return Consumer<ThemeProvider>(
      builder: (_, themeProv, __) {
        final isDark = themeProv.mode == ThemeMode.dark ||
            (themeProv.mode == ThemeMode.system &&
                WidgetsBinding.instance.platformDispatcher.platformBrightness == Brightness.dark);
        SystemChrome.setSystemUIOverlayStyle(SystemUiOverlayStyle(
          statusBarColor: Colors.transparent,
          statusBarIconBrightness: isDark ? Brightness.light : Brightness.dark,
          systemNavigationBarColor: isDark ? AppColors.surfaceDark : AppColors.surfaceLight,
          systemNavigationBarIconBrightness: isDark ? Brightness.light : Brightness.dark,
        ));
        return MaterialApp(
          title: 'KidBank',
          debugShowCheckedModeBanner: false,
          theme: AppTheme.lightTheme,
          darkTheme: AppTheme.darkTheme,
          themeMode: themeProv.mode,
          onGenerateRoute: AppRouter.generate,
          home: Consumer<AuthProvider>(
            builder: (_, auth, __) {
              switch (auth.status) {
                case AuthStatus.unknown:
                  return const _SplashScreen();
                case AuthStatus.unauthenticated:
                  return const LoginScreen();
                case AuthStatus.authenticated:
                  return auth.isParent ? const ParentShell() : const KidShell();
              }
            },
          ),
        );
      },
    );
  }
}

class _SplashScreen extends StatelessWidget {
  const _SplashScreen();
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Center(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Container(
              width: 80, height: 80,
              decoration: BoxDecoration(
                gradient: const LinearGradient(
                  colors: [AppColors.primary, AppColors.cardGradient2],
                ),
                borderRadius: BorderRadius.circular(20),
              ),
              child: const Icon(Icons.account_balance_wallet, color: Colors.white, size: 40),
            ),
            const SizedBox(height: 24),
            Text('KidBank', style: TextStyle(
              fontSize: 28, fontWeight: FontWeight.bold,
              color: AppColors.primary,
            )),
            const SizedBox(height: 16),
            const CircularProgressIndicator(color: AppColors.primary),
          ],
        ),
      ),
    );
  }
}
