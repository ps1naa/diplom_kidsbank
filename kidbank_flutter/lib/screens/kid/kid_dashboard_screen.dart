import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/api_service.dart';
import '../../core/theme.dart';
import '../../providers/auth_provider.dart';
import '../../widgets/gradient_card.dart';
import '../common/chat_screen.dart';
import '../common/leaderboard_screen.dart';
import 'kid_cards_screen.dart';
import 'kid_money_request_screen.dart';
import 'kid_savings_screen.dart';
import 'kid_goals_screen.dart';
import 'kid_categories_screen.dart';
import 'kid_education_screen.dart' as kid_education_screen;

class KidDashboardScreen extends StatefulWidget {
  const KidDashboardScreen({super.key});
  @override
  State<KidDashboardScreen> createState() => _KidDashboardScreenState();
}

class _KidDashboardScreenState extends State<KidDashboardScreen> with SingleTickerProviderStateMixin {
  List<dynamic>? _accounts;
  List<dynamic>? _tasks;
  Map<String, dynamic>? _me;
  List<dynamic>? _achievements;
  bool _loading = true;
  late AnimationController _anim;

  @override
  void initState() {
    super.initState();
    _anim = AnimationController(vsync: this, duration: const Duration(milliseconds: 600));
    _load();
  }

  Future<void> _load() async {
    final api = context.read<ApiService>();
    try {
      final results = await Future.wait([
        api.get('accounts/my'),
        api.get('tasks/my'),
        api.get('users/me'),
        api.get('achievements/my').catchError((_) => <dynamic>[]),
      ]);
      setState(() {
        _accounts = results[0] as List;
        _tasks = results[1] as List;
        _me = results[2];
        _achievements = results[3] as List;
        _loading = false;
      });
      _anim.forward();
    } catch (_) { setState(() => _loading = false); }
  }

  @override
  void dispose() { _anim.dispose(); super.dispose(); }

  @override
  Widget build(BuildContext context) {
    final auth = context.watch<AuthProvider>();
    final level = _me?['level'] ?? 1;
    final int xp = (_me?['totalXp'] as int?) ?? 0;
    final streak = _me?['currentStreak'] ?? 0;
    return Scaffold(
      body: SafeArea(
        child: _loading ? const Center(child: CircularProgressIndicator()) : RefreshIndicator(
          onRefresh: _load,
          child: ListView(
            padding: const EdgeInsets.all(20),
            children: [
              FadeTransition(
                opacity: _anim,
                child: Row(children: [
                  Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                    Text('Привет, ${auth.firstName}!', style: TextStyle(fontSize: 26, fontWeight: FontWeight.bold, color: context.textPrimary)),
                    const SizedBox(height: 4),
                    Row(children: [
                      _StatBadge(Icons.star, 'Ур. $level', AppColors.accent),
                      const SizedBox(width: 8),
                      _StatBadge(Icons.bolt, '$xp XP', AppColors.primary),
                      const SizedBox(width: 8),
                      _StatBadge(Icons.local_fire_department, '$streak', AppColors.error),
                    ]),
                  ])),
                  CircleAvatar(
                    radius: 26,
                    backgroundColor: AppColors.kidGradient1.withValues(alpha: 0.15),
                    child: Text(auth.firstName?[0] ?? 'K', style: const TextStyle(color: AppColors.kidGradient1, fontWeight: FontWeight.bold, fontSize: 22)),
                  ),
                ]),
              ),
              const SizedBox(height: 8),
              ClipRRect(
                borderRadius: BorderRadius.circular(8),
                child: TweenAnimationBuilder<double>(
                  tween: Tween(begin: 0, end: (xp % 100) / 100),
                  duration: const Duration(milliseconds: 1000),
                  curve: Curves.easeOutCubic,
                  builder: (_, val, __) => LinearProgressIndicator(
                    value: val,
                    backgroundColor: context.dividerColor,
                    valueColor: const AlwaysStoppedAnimation(AppColors.kidGradient1),
                    minHeight: 6,
                  ),
                ),
              ),
              const SizedBox(height: 4),
              Text('${xp % 100}/100 XP до следующего уровня', style: TextStyle(fontSize: 11, color: context.textSecondary)),
              const SizedBox(height: 20),
              GradientCard(
                colors: [AppColors.kidGradient1, AppColors.kidGradient2],
                child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                  Row(children: [
                    const Icon(Icons.account_balance_wallet, color: Colors.white, size: 20),
                    const SizedBox(width: 8),
                    const Text('Мой баланс', style: TextStyle(color: Colors.white70, fontSize: 14)),
                    const Spacer(),
                    Text('${_getSavingsBalance()} BYN в копилке', style: const TextStyle(color: Colors.white60, fontSize: 11)),
                  ]),
                  const SizedBox(height: 12),
                  TweenAnimationBuilder<double>(
                    tween: Tween(begin: 0, end: double.tryParse(_getBalance()) ?? 0),
                    duration: const Duration(milliseconds: 1200),
                    curve: Curves.easeOutCubic,
                    builder: (_, val, __) => Text(
                      '${val.toStringAsFixed(2)} BYN',
                      style: const TextStyle(color: Colors.white, fontSize: 32, fontWeight: FontWeight.bold),
                    ),
                  ),
                ]),
              ),
              const SizedBox(height: 20),
              GridView.count(
                crossAxisCount: 4,
                shrinkWrap: true,
                physics: const NeverScrollableScrollPhysics(),
                mainAxisSpacing: 10,
                crossAxisSpacing: 10,
                children: [
                  _QuickAction(icon: Icons.attach_money, label: 'Запросить', color: AppColors.accent, onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => const KidMoneyRequestScreen()))),
                  _QuickAction(icon: Icons.credit_card, label: 'Карты', color: AppColors.primary, onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => const KidCardsScreen()))),
                  _QuickAction(icon: Icons.savings, label: 'Копилка', color: AppColors.warning, onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => const KidSavingsScreen()))),
                  _QuickAction(icon: Icons.chat_bubble_outline, label: 'Чат', color: AppColors.success, onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => const ChatScreen()))),
                ],
              ),
              const SizedBox(height: 8),
              GridView.count(
                crossAxisCount: 4,
                shrinkWrap: true,
                physics: const NeverScrollableScrollPhysics(),
                mainAxisSpacing: 10,
                crossAxisSpacing: 10,
                children: [
                  _QuickAction(icon: Icons.flag, label: 'Цели', color: AppColors.kidGradient2, onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => const KidGoalsScreen()))),
                  _QuickAction(icon: Icons.emoji_events, label: 'Рейтинг', color: const Color(0xFFAB47BC), onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => const LeaderboardScreen()))),
                  _QuickAction(icon: Icons.category, label: 'Категории', color: const Color(0xFF5C6BC0), onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => const KidCategoriesScreen()))),
                  _QuickAction(icon: Icons.school, label: 'Обучение', color: const Color(0xFFE91E63), onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => const kid_education_screen.KidEducationScreen()))),
                ],
              ),
              if (_achievements != null && _achievements!.isNotEmpty) ...[
                const SizedBox(height: 24),
                Row(mainAxisAlignment: MainAxisAlignment.spaceBetween, children: [
                  Text('Достижения', style: TextStyle(fontSize: 18, fontWeight: FontWeight.w600, color: context.textPrimary)),
                  Text('${_achievements!.where((a) => a['unlockedAt'] != null).length}/${_achievements!.length}', style: TextStyle(color: context.textSecondary)),
                ]),
                const SizedBox(height: 12),
                SizedBox(
                  height: 80,
                  child: ListView.builder(
                    scrollDirection: Axis.horizontal,
                    itemCount: _achievements!.length,
                    itemBuilder: (_, i) {
                      final a = _achievements![i];
                      final unlocked = a['unlockedAt'] != null;
                      return Container(
                        width: 70,
                        margin: const EdgeInsets.only(right: 8),
                        child: Column(children: [
                          Container(
                            width: 48, height: 48,
                            decoration: BoxDecoration(
                              color: (unlocked ? AppColors.accent : context.textHint).withValues(alpha: unlocked ? 0.2 : 0.1),
                              borderRadius: BorderRadius.circular(14),
                            ),
                            child: Icon(Icons.emoji_events, color: unlocked ? AppColors.accent : context.textHint, size: 24),
                          ),
                          const SizedBox(height: 4),
                          Text(a['title'] ?? '', style: TextStyle(fontSize: 10, color: unlocked ? context.textPrimary : context.textHint), maxLines: 1, overflow: TextOverflow.ellipsis, textAlign: TextAlign.center),
                        ]),
                      );
                    },
                  ),
                ),
              ],
              if (_tasks != null && _tasks!.where((t) => t['status'] == 'Pending').isNotEmpty) ...[
                const SizedBox(height: 24),
                Text('Активные задания', style: TextStyle(fontSize: 18, fontWeight: FontWeight.w600, color: context.textPrimary)),
                const SizedBox(height: 12),
                ..._tasks!.where((t) => t['status'] == 'Pending').take(3).map((t) => Card(
                  margin: const EdgeInsets.only(bottom: 8),
                  child: ListTile(
                    leading: Container(
                      width: 40, height: 40,
                      decoration: BoxDecoration(color: AppColors.primary.withValues(alpha: 0.15), borderRadius: BorderRadius.circular(10)),
                      child: const Icon(Icons.assignment, color: AppColors.primary, size: 20),
                    ),
                    title: Text(t['title'] ?? ''),
                    subtitle: Text('${t['rewardAmount']} BYN', style: TextStyle(color: AppColors.accent)),
                    trailing: ElevatedButton(
                      onPressed: () async {
                        await context.read<ApiService>().post('tasks/${t['id']}/complete', body: {});
                        _load();
                      },
                      style: ElevatedButton.styleFrom(padding: const EdgeInsets.symmetric(horizontal: 12)),
                      child: const Text('Готово', style: TextStyle(fontSize: 12)),
                    ),
                  ),
                )),
              ],
            ],
          ),
        ),
      ),
    );
  }

  String _getBalance() {
    if (_accounts == null) return '0.00';
    final main = _accounts!.where((a) => a['type'] == 'Main').toList();
    if (main.isEmpty) return '0.00';
    return (main.first['balance'] as num).toStringAsFixed(2);
  }

  String _getSavingsBalance() {
    if (_accounts == null) return '0.00';
    final savings = _accounts!.where((a) => a['type'] == 'Savings').toList();
    if (savings.isEmpty) return '0.00';
    double total = 0;
    for (final s in savings) {
      total += (s['balance'] as num).toDouble();
    }
    return total.toStringAsFixed(2);
  }
}

class _StatBadge extends StatelessWidget {
  final IconData icon;
  final String text;
  final Color color;
  const _StatBadge(this.icon, this.text, this.color);
  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
      decoration: BoxDecoration(color: color.withValues(alpha: 0.15), borderRadius: BorderRadius.circular(8)),
      child: Row(mainAxisSize: MainAxisSize.min, children: [
        Icon(icon, size: 14, color: color),
        const SizedBox(width: 4),
        Text(text, style: TextStyle(fontSize: 12, color: color, fontWeight: FontWeight.w600)),
      ]),
    );
  }
}

class _QuickAction extends StatelessWidget {
  final IconData icon;
  final String label;
  final Color color;
  final VoidCallback onTap;
  const _QuickAction({required this.icon, required this.label, required this.color, required this.onTap});
  @override
  Widget build(BuildContext context) {
    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(16),
      child: Container(
        decoration: BoxDecoration(color: color.withValues(alpha: 0.12), borderRadius: BorderRadius.circular(16)),
        child: Column(mainAxisAlignment: MainAxisAlignment.center, children: [
          Icon(icon, color: color, size: 24),
          const SizedBox(height: 6),
          Text(label, style: TextStyle(fontSize: 11, color: color, fontWeight: FontWeight.w500), textAlign: TextAlign.center),
        ]),
      ),
    );
  }
}
