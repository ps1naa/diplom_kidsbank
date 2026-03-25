import 'package:flutter/material.dart';
import '../../core/theme.dart';
import 'kid_dashboard_screen.dart';
import 'kid_tasks_screen.dart';
import 'kid_education_screen.dart';
import 'kid_profile_screen.dart';

class KidShell extends StatefulWidget {
  const KidShell({super.key});
  @override
  State<KidShell> createState() => _KidShellState();
}

class _KidShellState extends State<KidShell> {
  int _index = 0;
  int _refreshKey = 0;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: IndexedStack(index: _index, children: [
        KidDashboardScreen(key: ValueKey('dash_$_refreshKey')),
        KidTasksScreen(key: ValueKey('tasks_$_refreshKey')),
        KidEducationScreen(key: ValueKey('edu_$_refreshKey')),
        KidProfileScreen(key: ValueKey('profile_$_refreshKey')),
      ]),
      bottomNavigationBar: Container(
        decoration: BoxDecoration(
          color: Theme.of(context).colorScheme.surface,
          boxShadow: [BoxShadow(color: Colors.black.withValues(alpha: 0.2), blurRadius: 20, offset: const Offset(0, -5))],
        ),
        child: BottomNavigationBar(
          currentIndex: _index,
          onTap: (i) => setState(() {
            if (i != _index) _refreshKey++;
            _index = i;
          }),
          selectedItemColor: AppColors.kidGradient1,
          items: const [
            BottomNavigationBarItem(icon: Icon(Icons.home_rounded), label: 'Главная'),
            BottomNavigationBarItem(icon: Icon(Icons.assignment_rounded), label: 'Задания'),
            BottomNavigationBarItem(icon: Icon(Icons.school_rounded), label: 'Учёба'),
            BottomNavigationBarItem(icon: Icon(Icons.person_rounded), label: 'Профиль'),
          ],
        ),
      ),
    );
  }
}
