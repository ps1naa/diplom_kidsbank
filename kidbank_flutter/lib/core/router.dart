import 'package:flutter/material.dart';
import '../screens/auth/login_screen.dart';
import '../screens/auth/register_parent_screen.dart';
import '../screens/auth/register_kid_screen.dart';
import '../screens/parent/parent_shell.dart';
import '../screens/kid/kid_shell.dart';
import '../screens/common/subscription_screen.dart';
import '../screens/common/settings_screen.dart';

class AppRouter {
  static const login = '/login';
  static const registerParent = '/register/parent';
  static const registerKid = '/register/kid';
  static const parentHome = '/parent';
  static const kidHome = '/kid';
  static const subscription = '/subscription';
  static const settings = '/settings';

  static Route<dynamic> generate(RouteSettings settings) {
    switch (settings.name) {
      case login:
        return _slide(const LoginScreen());
      case registerParent:
        return _slide(const RegisterParentScreen());
      case registerKid:
        return _slide(const RegisterKidScreen());
      case parentHome:
        return _slide(const ParentShell());
      case kidHome:
        return _slide(const KidShell());
      case subscription:
        return _slide(const SubscriptionScreen());
      case AppRouter.settings:
        return _slide(const SettingsScreen());
      default:
        return _slide(const LoginScreen());
    }
  }

  static PageRouteBuilder _slide(Widget page) {
    return PageRouteBuilder(
      pageBuilder: (_, __, ___) => page,
      transitionsBuilder: (_, animation, __, child) {
        return FadeTransition(
          opacity: CurvedAnimation(parent: animation, curve: Curves.easeOutCubic),
          child: SlideTransition(
            position: Tween<Offset>(begin: const Offset(0.1, 0), end: Offset.zero)
                .animate(CurvedAnimation(parent: animation, curve: Curves.easeOutCubic)),
            child: child,
          ),
        );
      },
    );
  }
}
