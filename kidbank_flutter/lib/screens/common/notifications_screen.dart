import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:intl/intl.dart';
import '../../core/api_service.dart';
import '../../core/theme.dart';

class NotificationsScreen extends StatefulWidget {
  const NotificationsScreen({super.key});
  @override
  State<NotificationsScreen> createState() => _NotificationsScreenState();
}

class _NotificationsScreenState extends State<NotificationsScreen> {
  List<dynamic> _notifications = [];
  bool _loading = true;

  @override
  void initState() { super.initState(); _load(); }

  Future<void> _load() async {
    try {
      final data = await context.read<ApiService>().get('notifications?limit=50');
      setState(() { _notifications = data as List; _loading = false; });
    } catch (_) { setState(() => _loading = false); }
  }

  @override
  Widget build(BuildContext context) {
    final unreadCount = _notifications.where((n) => n['isRead'] != true).length;
    return Scaffold(
      appBar: AppBar(
        title: const Text('Уведомления'),
        actions: [
          if (unreadCount > 0) TextButton(
            onPressed: () async {
              for (final n in _notifications.where((n) => n['isRead'] != true)) {
                await context.read<ApiService>().post('notifications/${n['id']}/read');
              }
              _load();
            },
            child: const Text('Прочитать все'),
          ),
        ],
      ),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : _notifications.isEmpty
              ? Center(child: Column(mainAxisSize: MainAxisSize.min, children: [
                  Icon(Icons.notifications_off, size: 64, color: context.textHint),
                  const SizedBox(height: 16),
                  Text('Нет уведомлений', style: TextStyle(fontSize: 18, color: context.textSecondary)),
                ]))
              : RefreshIndicator(
                  onRefresh: _load,
                  child: ListView.builder(
                    padding: const EdgeInsets.all(16),
                    itemCount: _notifications.length,
                    itemBuilder: (_, i) {
                      final n = _notifications[i];
                      final isRead = n['isRead'] == true;
                      final date = n['createdAt'] != null ? DateFormat('dd.MM HH:mm').format(DateTime.parse(n['createdAt']).toLocal()) : '';
                      return TweenAnimationBuilder<double>(
                        tween: Tween(begin: 0, end: 1),
                        duration: Duration(milliseconds: 200 + i * 40),
                        curve: Curves.easeOutCubic,
                        builder: (_, val, child) => Opacity(opacity: val, child: Transform.translate(offset: Offset(0, 10 * (1 - val)), child: child)),
                        child: Card(
                          margin: const EdgeInsets.only(bottom: 8),
                          color: isRead ? null : AppColors.primary.withValues(alpha: 0.05),
                          child: ListTile(
                            leading: Container(
                              width: 40, height: 40,
                              decoration: BoxDecoration(
                                color: (isRead ? context.textHint : AppColors.primary).withValues(alpha: 0.15),
                                borderRadius: BorderRadius.circular(10),
                              ),
                              child: Icon(
                                isRead ? Icons.notifications_none : Icons.notifications_active,
                                color: isRead ? context.textHint : AppColors.primary,
                                size: 20,
                              ),
                            ),
                            title: Text(n['title'] ?? n['message'] ?? '', style: TextStyle(fontWeight: isRead ? FontWeight.w400 : FontWeight.w600)),
                            subtitle: Text(date, style: TextStyle(fontSize: 12, color: context.textSecondary)),
                            trailing: !isRead ? Container(width: 8, height: 8, decoration: const BoxDecoration(color: AppColors.primary, shape: BoxShape.circle)) : null,
                            onTap: () async {
                              if (!isRead) {
                                await context.read<ApiService>().post('notifications/${n['id']}/read');
                                _load();
                              }
                            },
                          ),
                        ),
                      );
                    },
                  ),
                ),
    );
  }
}
