import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:intl/intl.dart';
import '../../core/api_service.dart';
import '../../core/theme.dart';

class ActivityScreen extends StatefulWidget {
  const ActivityScreen({super.key});
  @override
  State<ActivityScreen> createState() => _ActivityScreenState();
}

class _ActivityScreenState extends State<ActivityScreen> {
  List<dynamic> _feed = [];
  bool _loading = true;

  @override
  void initState() { super.initState(); _load(); }

  Future<void> _load() async {
    try {
      final data = await context.read<ApiService>().get('activity/feed?pageNumber=1&pageSize=50');
      final items = data is List ? data : (data['items'] ?? []) as List;
      setState(() { _feed = items; _loading = false; });
    } catch (_) { setState(() => _loading = false); }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Активность')),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : _feed.isEmpty
              ? Center(child: Column(mainAxisSize: MainAxisSize.min, children: [
                  Icon(Icons.history, size: 64, color: context.textHint),
                  const SizedBox(height: 16),
                  Text('Нет активности', style: TextStyle(fontSize: 18, color: context.textSecondary)),
                ]))
              : RefreshIndicator(
                  onRefresh: _load,
                  child: ListView.builder(
                    padding: const EdgeInsets.all(16),
                    itemCount: _feed.length,
                    itemBuilder: (_, i) {
                      final item = _feed[i];
                      final type = item['type'] ?? '';
                      final Color color;
                      final IconData icon;
                      switch (type) {
                        case 'Transfer': color = AppColors.primary; icon = Icons.swap_horiz;
                        case 'Task': color = AppColors.accent; icon = Icons.assignment;
                        case 'Goal': color = AppColors.success; icon = Icons.flag;
                        case 'Achievement': color = AppColors.warning; icon = Icons.emoji_events;
                        default: color = context.textSecondary; icon = Icons.circle;
                      }
                      final date = item['createdAt'] != null ? DateFormat('dd.MM.yyyy HH:mm').format(DateTime.parse(item['createdAt']).toLocal()) : '';
                      return TweenAnimationBuilder<double>(
                        tween: Tween(begin: 0, end: 1),
                        duration: Duration(milliseconds: 200 + i * 40),
                        curve: Curves.easeOutCubic,
                        builder: (_, val, child) => Opacity(opacity: val, child: Transform.translate(offset: Offset(0, 10 * (1 - val)), child: child)),
                        child: Card(
                          margin: const EdgeInsets.only(bottom: 8),
                          child: ListTile(
                            leading: Container(
                              width: 40, height: 40,
                              decoration: BoxDecoration(color: color.withValues(alpha: 0.15), borderRadius: BorderRadius.circular(10)),
                              child: Icon(icon, color: color, size: 20),
                            ),
                            title: Text(item['description'] ?? item['title'] ?? type),
                            subtitle: Text(date, style: TextStyle(fontSize: 12, color: context.textSecondary)),
                          ),
                        ),
                      );
                    },
                  ),
                ),
    );
  }
}
