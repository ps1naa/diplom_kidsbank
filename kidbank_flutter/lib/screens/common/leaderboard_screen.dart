import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/api_service.dart';
import '../../core/theme.dart';
import '../../providers/auth_provider.dart';

class LeaderboardScreen extends StatefulWidget {
  const LeaderboardScreen({super.key});
  @override
  State<LeaderboardScreen> createState() => _LeaderboardScreenState();
}

class _LeaderboardScreenState extends State<LeaderboardScreen> {
  List<dynamic> _entries = [];
  bool _loading = true;

  @override
  void initState() { super.initState(); _load(); }

  Future<void> _load() async {
    try {
      final data = await context.read<ApiService>().get('leaderboard/family');
      setState(() { _entries = data is List ? data : []; _loading = false; });
    } catch (_) { setState(() => _loading = false); }
  }

  @override
  Widget build(BuildContext context) {
    final myId = context.read<AuthProvider>().userId;
    return Scaffold(
      appBar: AppBar(title: const Text('Семейный рейтинг')),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : _entries.isEmpty
              ? Center(child: Text('Нет данных', style: TextStyle(color: context.textSecondary)))
              : RefreshIndicator(
                  onRefresh: _load,
                  child: ListView.builder(
                    padding: const EdgeInsets.all(16),
                    itemCount: _entries.length,
                    itemBuilder: (_, i) {
                      final e = _entries[i];
                      final isMe = e['userId'] == myId;
                      final medal = i < 3 ? ['🥇', '🥈', '🥉'][i] : '${i + 1}';
                      return TweenAnimationBuilder<double>(
                        tween: Tween(begin: 0, end: 1),
                        duration: Duration(milliseconds: 300 + i * 80),
                        curve: Curves.easeOutCubic,
                        builder: (_, val, child) => Opacity(opacity: val, child: Transform.translate(offset: Offset(0, 20 * (1 - val)), child: child)),
                        child: Card(
                          margin: const EdgeInsets.only(bottom: 8),
                          color: isMe ? AppColors.primary.withValues(alpha: 0.1) : null,
                          shape: isMe ? RoundedRectangleBorder(borderRadius: BorderRadius.circular(16), side: const BorderSide(color: AppColors.primary, width: 1.5)) : null,
                          child: ListTile(
                            leading: Text(medal, style: const TextStyle(fontSize: 24)),
                            title: Text(e['name'] ?? '', style: TextStyle(fontWeight: isMe ? FontWeight.bold : FontWeight.w500)),
                            subtitle: Text('Уровень ${e['level'] ?? 1} • Стрик ${e['currentStreak'] ?? 0} дн.', style: TextStyle(fontSize: 12, color: context.textSecondary)),
                            trailing: Container(
                              padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                              decoration: BoxDecoration(color: AppColors.primary.withValues(alpha: 0.15), borderRadius: BorderRadius.circular(12)),
                              child: Text('${e['totalXp'] ?? 0} XP', style: const TextStyle(fontWeight: FontWeight.bold, color: AppColors.primary)),
                            ),
                          ),
                        ),
                      );
                    },
                  ),
                ),
    );
  }
}
