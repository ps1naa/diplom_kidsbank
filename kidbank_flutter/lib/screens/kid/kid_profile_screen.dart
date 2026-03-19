import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/api_service.dart';
import '../../core/theme.dart';
import '../../providers/auth_provider.dart';
import '../../providers/theme_provider.dart';
import '../common/chat_screen.dart';
import '../common/notifications_screen.dart';
import '../common/leaderboard_screen.dart';
import '../common/subscription_screen.dart';
import '../common/settings_screen.dart';
import 'kid_cards_screen.dart';
import 'kid_money_request_screen.dart';
import 'kid_savings_screen.dart';

class KidProfileScreen extends StatefulWidget {
  const KidProfileScreen({super.key});
  @override
  State<KidProfileScreen> createState() => _KidProfileScreenState();
}

class _KidProfileScreenState extends State<KidProfileScreen> {
  Map<String, dynamic>? _me;
  List<dynamic>? _achievements;
  bool _loading = true;

  @override
  void initState() { super.initState(); _load(); }

  Future<void> _load() async {
    final api = context.read<ApiService>();
    try {
      final results = await Future.wait([
        api.get('users/me'),
        api.get('achievements/my').catchError((_) => <dynamic>[]),
      ]);
      setState(() { _me = results[0]; _achievements = results[1] as List; _loading = false; });
    } catch (_) { setState(() => _loading = false); }
  }

  @override
  Widget build(BuildContext context) {
    final auth = context.watch<AuthProvider>();
    final theme = context.watch<ThemeProvider>();
    final level = _me?['level'] ?? 1;
    final xp = _me?['totalXp'] ?? 0;
    final streak = _me?['currentStreak'] ?? 0;
    final unlockedCount = _achievements?.where((a) => a['unlockedAt'] != null).length ?? 0;
    final totalAch = _achievements?.length ?? 0;

    return Scaffold(
      appBar: AppBar(title: const Text('Профиль')),
      body: _loading ? const Center(child: CircularProgressIndicator()) : RefreshIndicator(
        onRefresh: _load,
        child: ListView(
          padding: const EdgeInsets.all(16),
          children: [
            Card(child: Padding(
              padding: const EdgeInsets.all(20),
              child: Column(children: [
                CircleAvatar(
                  radius: 40,
                  backgroundColor: AppColors.kidGradient1.withValues(alpha: 0.15),
                  child: Text(auth.firstName?[0] ?? 'K', style: const TextStyle(fontSize: 32, fontWeight: FontWeight.bold, color: AppColors.kidGradient1)),
                ),
                const SizedBox(height: 12),
                Text(auth.fullName ?? '', style: TextStyle(fontSize: 20, fontWeight: FontWeight.bold, color: context.textPrimary)),
                const SizedBox(height: 4),
                Text(auth.email ?? '', style: TextStyle(color: context.textSecondary, fontSize: 13)),
                const SizedBox(height: 16),
                Row(mainAxisAlignment: MainAxisAlignment.spaceEvenly, children: [
                  _StatColumn(Icons.star, '$level', 'Уровень', AppColors.accent),
                  _StatColumn(Icons.bolt, '$xp', 'XP', AppColors.primary),
                  _StatColumn(Icons.local_fire_department, '$streak', 'Стрик', AppColors.error),
                  _StatColumn(Icons.emoji_events, '$unlockedCount/$totalAch', 'Ачивки', AppColors.warning),
                ]),
              ]),
            )),
            const SizedBox(height: 8),
            Card(child: ListTile(
              leading: Icon(Icons.dark_mode, color: AppColors.primary),
              title: const Text('Тёмная тема'),
              trailing: Switch(value: theme.isDark, onChanged: (_) => theme.toggle(), activeColor: AppColors.primary),
            )),
            if (_achievements != null && _achievements!.isNotEmpty) ...[
              const SizedBox(height: 16),
              Text('Достижения', style: TextStyle(fontSize: 16, fontWeight: FontWeight.w600, color: context.textPrimary)),
              const SizedBox(height: 8),
              Card(child: Padding(
                padding: const EdgeInsets.all(12),
                child: Wrap(
                  spacing: 8, runSpacing: 8,
                  children: _achievements!.map<Widget>((a) {
                    final unlocked = a['unlockedAt'] != null;
                    return Tooltip(
                      message: a['description'] ?? '',
                      child: SizedBox(
                        width: 64, height: 72,
                        child: Column(children: [
                          Container(
                            width: 44, height: 44,
                            decoration: BoxDecoration(
                              color: (unlocked ? AppColors.accent : context.textHint).withValues(alpha: unlocked ? 0.2 : 0.08),
                              borderRadius: BorderRadius.circular(12),
                            ),
                            child: Icon(Icons.emoji_events, color: unlocked ? AppColors.accent : context.textHint, size: 22),
                          ),
                          const SizedBox(height: 4),
                          Text(a['title'] ?? '', style: TextStyle(fontSize: 9, color: unlocked ? context.textPrimary : context.textHint), maxLines: 1, overflow: TextOverflow.ellipsis, textAlign: TextAlign.center),
                        ]),
                      ),
                    );
                  }).toList(),
                ),
              )),
            ],
            const SizedBox(height: 16),
            Text('Меню', style: TextStyle(fontSize: 14, fontWeight: FontWeight.w600, color: context.textSecondary)),
            const SizedBox(height: 8),
            _MenuItem(icon: Icons.credit_card, title: 'Мои карты', onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => const KidCardsScreen()))),
            _MenuItem(icon: Icons.savings, title: 'Копилка', onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => const KidSavingsScreen()))),
            _MenuItem(icon: Icons.request_page, title: 'Запросы денег', onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => const KidMoneyRequestScreen()))),
            _MenuItem(icon: Icons.emoji_events, title: 'Рейтинг', onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => const LeaderboardScreen()))),
            _MenuItem(icon: Icons.chat_bubble_outline, title: 'Семейный чат', onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => const ChatScreen()))),
            _MenuItem(icon: Icons.notifications_none, title: 'Уведомления', onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => const NotificationsScreen()))),
            _MenuItem(icon: Icons.workspace_premium, title: 'Подписка Pro', onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => const SubscriptionScreen()))),
            _MenuItem(icon: Icons.settings, title: 'Настройки', onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => const SettingsScreen()))),
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
}

class _StatColumn extends StatelessWidget {
  final IconData icon;
  final String value, label;
  final Color color;
  const _StatColumn(this.icon, this.value, this.label, this.color);
  @override
  Widget build(BuildContext context) {
    return Column(children: [
      Icon(icon, color: color, size: 24),
      const SizedBox(height: 4),
      Text(value, style: TextStyle(fontWeight: FontWeight.bold, color: context.textPrimary)),
      Text(label, style: TextStyle(fontSize: 11, color: context.textSecondary)),
    ]);
  }
}

class _MenuItem extends StatelessWidget {
  final IconData icon;
  final String title;
  final VoidCallback onTap;
  const _MenuItem({required this.icon, required this.title, required this.onTap});
  @override
  Widget build(BuildContext context) {
    return Card(
      margin: const EdgeInsets.only(bottom: 6),
      child: ListTile(
        leading: Icon(icon, color: AppColors.primary),
        title: Text(title),
        trailing: Icon(Icons.chevron_right, color: context.textHint),
        onTap: onTap,
      ),
    );
  }
}
