import 'package:flutter/material.dart';
import 'parent_dashboard_screen.dart';
import 'parent_kids_screen.dart';
import 'parent_tasks_screen.dart';
import 'parent_cards_screen.dart';
import 'parent_profile_screen.dart';

class ParentShell extends StatefulWidget {
  const ParentShell({super.key});
  @override
  State<ParentShell> createState() => _ParentShellState();
}

class _ParentShellState extends State<ParentShell> {
  int _index = 0;
  final _screens = const [
    ParentDashboardScreen(),
    ParentKidsScreen(),
    ParentCardsScreen(),
    ParentTasksScreen(),
    ParentProfileScreen(),
  ];

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: IndexedStack(index: _index, children: _screens),
      bottomNavigationBar: Container(
        decoration: BoxDecoration(
          color: Theme.of(context).colorScheme.surface,
          boxShadow: [BoxShadow(color: Colors.black.withValues(alpha: 0.15), blurRadius: 20, offset: const Offset(0, -5))],
        ),
        child: BottomNavigationBar(
          currentIndex: _index,
          onTap: (i) => setState(() => _index = i),
          items: const [
            BottomNavigationBarItem(icon: Icon(Icons.dashboard_rounded), label: 'Главная'),
            BottomNavigationBarItem(icon: Icon(Icons.family_restroom), label: 'Дети'),
            BottomNavigationBarItem(icon: Icon(Icons.credit_card), label: 'Карты'),
            BottomNavigationBarItem(icon: Icon(Icons.assignment_rounded), label: 'Задания'),
            BottomNavigationBarItem(icon: Icon(Icons.person_rounded), label: 'Профиль'),
          ],
        ),
      ),
    );
  }
}
