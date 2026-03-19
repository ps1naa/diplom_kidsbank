import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/api_service.dart';
import '../../core/theme.dart';
import '../../providers/auth_provider.dart';
import '../../providers/theme_provider.dart';
import '../common/chat_screen.dart';
import '../common/notifications_screen.dart';
import '../common/activity_screen.dart';
import '../common/settings_screen.dart';
import '../common/subscription_screen.dart';
import 'parent_allowance_screen.dart';
import 'parent_money_requests_screen.dart';
import 'parent_spending_limits_screen.dart';

class ParentProfileScreen extends StatefulWidget {
  const ParentProfileScreen({super.key});
  @override
  State<ParentProfileScreen> createState() => _ParentProfileScreenState();
}

class _ParentProfileScreenState extends State<ParentProfileScreen> {
  List<dynamic>? _kids;
  int _notifCount = 0;

  @override
  void initState() { super.initState(); _load(); }

  Future<void> _load() async {
    final api = context.read<ApiService>();
    try {
      final results = await Future.wait([
        api.get('families/kids'),
        api.get('notifications?limit=50'),
      ]);
      setState(() {
        _kids = results[0] as List;
        final notifs = results[1] as List;
        _notifCount = notifs.where((n) => n['isRead'] != true).length;
      });
    } catch (_) {}
  }

  @override
  Widget build(BuildContext context) {
    final auth = context.watch<AuthProvider>();
    final theme = context.watch<ThemeProvider>();
    return Scaffold(
      appBar: AppBar(title: const Text('Профиль')),
      body: RefreshIndicator(
        onRefresh: _load,
        child: ListView(
          padding: const EdgeInsets.all(16),
          children: [
            Card(child: Padding(
              padding: const EdgeInsets.all(20),
              child: Row(children: [
                CircleAvatar(
                  radius: 32,
                  backgroundColor: AppColors.primary.withValues(alpha: 0.15),
                  child: Text(auth.firstName?[0] ?? 'P', style: const TextStyle(fontSize: 24, fontWeight: FontWeight.bold, color: AppColors.primary)),
                ),
                const SizedBox(width: 16),
                Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                  Text(auth.fullName ?? '', style: TextStyle(fontSize: 18, fontWeight: FontWeight.w600, color: context.textPrimary)),
                  const SizedBox(height: 4),
                  Text(auth.email ?? '', style: TextStyle(color: context.textSecondary, fontSize: 13)),
                  const SizedBox(height: 6),
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 3),
                    decoration: BoxDecoration(color: AppColors.primary.withValues(alpha: 0.15), borderRadius: BorderRadius.circular(8)),
                    child: const Text('Родитель', style: TextStyle(color: AppColors.primary, fontSize: 12, fontWeight: FontWeight.w600)),
                  ),
                ])),
              ]),
            )),
            const SizedBox(height: 8),
            Card(child: ListTile(
              leading: const Icon(Icons.dark_mode, color: AppColors.primary),
              title: const Text('Тёмная тема'),
              trailing: Switch(
                value: theme.isDark,
                onChanged: (_) => theme.toggle(),
                activeColor: AppColors.primary,
              ),
            )),
            const SizedBox(height: 16),
            Text('Семья', style: TextStyle(fontSize: 14, fontWeight: FontWeight.w600, color: context.textSecondary)),
            const SizedBox(height: 8),
            if (_kids != null) ..._kids!.map((kid) => Card(
              margin: const EdgeInsets.only(bottom: 8),
              child: ListTile(
                leading: CircleAvatar(
                  backgroundColor: AppColors.kidGradient1.withValues(alpha: 0.15),
                  child: Text('${kid['firstName'][0]}', style: const TextStyle(color: AppColors.kidGradient1, fontWeight: FontWeight.bold)),
                ),
                title: Text('${kid['firstName']} ${kid['lastName']}'),
                subtitle: Text('XP: ${kid['totalXp'] ?? 0} • Стрик: ${kid['currentStreak'] ?? 0} дн.', style: TextStyle(color: context.textSecondary, fontSize: 12)),
                trailing: IconButton(
                  icon: Icon(Icons.delete_outline, color: AppColors.error),
                  onPressed: () => _confirmRemoveKid(kid),
                ),
              ),
            )),
            const SizedBox(height: 16),
            Text('Меню', style: TextStyle(fontSize: 14, fontWeight: FontWeight.w600, color: context.textSecondary)),
            const SizedBox(height: 8),
            _MenuItem(icon: Icons.history, title: 'Активность', onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => const ActivityScreen()))),
            _MenuItem(icon: Icons.notifications_none, title: 'Уведомления', badge: _notifCount, onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => const NotificationsScreen()))),
            _MenuItem(icon: Icons.chat_bubble_outline, title: 'Семейный чат', onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => const ChatScreen()))),
            _MenuItem(icon: Icons.request_page, title: 'Запросы денег', onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => const ParentMoneyRequestsScreen()))),
            _MenuItem(icon: Icons.schedule, title: 'Авто-пополнение', onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => const ParentAllowanceScreen()))),
            _MenuItem(icon: Icons.block, title: 'Лимиты расходов', onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => const ParentSpendingLimitsScreen()))),
            _MenuItem(icon: Icons.workspace_premium, title: 'Подписка Pro', onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => const SubscriptionScreen()))),
            _MenuItem(icon: Icons.settings_outlined, title: 'Настройки', onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => const SettingsScreen()))),
            _MenuItem(icon: Icons.description_outlined, title: 'Публичная оферта', onTap: () {}),
            _MenuItem(icon: Icons.privacy_tip_outlined, title: 'Политика конфиденциальности', onTap: () {}),
            const SizedBox(height: 16),
            SizedBox(width: double.infinity, child: OutlinedButton.icon(
              onPressed: () => context.read<AuthProvider>().logout(),
              icon: const Icon(Icons.logout, color: AppColors.error),
              label: const Text('Выйти', style: TextStyle(color: AppColors.error)),
              style: OutlinedButton.styleFrom(side: const BorderSide(color: AppColors.error)),
            )),
            const SizedBox(height: 32),
          ],
        ),
      ),
    );
  }

  void _confirmRemoveKid(dynamic kid) {
    showDialog(context: context, builder: (ctx) => AlertDialog(
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
      title: const Text('Удалить ребёнка?'),
      content: Text('Удалить ${kid['firstName']} ${kid['lastName']} из семьи? Это действие необратимо.'),
      actions: [
        TextButton(onPressed: () => Navigator.pop(ctx), child: const Text('Отмена')),
        ElevatedButton(
          onPressed: () async {
            Navigator.pop(ctx);
            try {
              await context.read<ApiService>().delete('users/${kid['id']}');
              _load();
              if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('${kid['firstName']} удалён')));
            } catch (e) {
              if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(e.toString())));
            }
          },
          style: ElevatedButton.styleFrom(backgroundColor: AppColors.error),
          child: const Text('Удалить'),
        ),
      ],
    ));
  }
}

class _MenuItem extends StatelessWidget {
  final IconData icon;
  final String title;
  final VoidCallback onTap;
  final int badge;
  const _MenuItem({required this.icon, required this.title, required this.onTap, this.badge = 0});
  @override
  Widget build(BuildContext context) {
    return Card(
      margin: const EdgeInsets.only(bottom: 6),
      child: ListTile(
        leading: Icon(icon, color: AppColors.primary),
        title: Text(title),
        trailing: Row(mainAxisSize: MainAxisSize.min, children: [
          if (badge > 0) Container(
            padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
            decoration: BoxDecoration(color: AppColors.error, borderRadius: BorderRadius.circular(12)),
            child: Text('$badge', style: const TextStyle(color: Colors.white, fontSize: 12)),
          ),
          Icon(Icons.chevron_right, color: context.textHint),
        ]),
        onTap: onTap,
      ),
    );
  }
}
