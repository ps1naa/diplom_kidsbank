import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/api_service.dart';
import '../../core/theme.dart';

class KidTasksScreen extends StatefulWidget {
  const KidTasksScreen({super.key});
  @override
  State<KidTasksScreen> createState() => _KidTasksScreenState();
}

class _KidTasksScreenState extends State<KidTasksScreen> with SingleTickerProviderStateMixin {
  late TabController _tab;
  List<dynamic> _allTasks = [];
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    _tab = TabController(length: 3, vsync: this);
    _load();
  }

  Future<void> _load() async {
    try {
      final data = await context.read<ApiService>().get('tasks/my');
      setState(() { _allTasks = data as List; _loading = false; });
    } catch (_) { setState(() => _loading = false); }
  }

  List<dynamic> _filter(List<String> statuses) =>
      _allTasks.where((t) => statuses.contains(t['status'])).toList();

  @override
  void dispose() { _tab.dispose(); super.dispose(); }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Мои задания'),
        bottom: TabBar(controller: _tab, tabs: const [
          Tab(text: 'Активные'),
          Tab(text: 'На проверке'),
          Tab(text: 'Завершённые'),
        ]),
      ),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : TabBarView(controller: _tab, children: [
              _buildList(_filter(['Pending']), canComplete: true),
              _buildList(_filter(['Completed'])),
              _buildList(_filter(['Approved', 'Rejected'])),
            ]),
    );
  }

  Widget _buildList(List<dynamic> tasks, {bool canComplete = false}) {
    if (tasks.isEmpty) {
      return Center(child: Column(mainAxisSize: MainAxisSize.min, children: [
        Icon(Icons.assignment_outlined, size: 64, color: context.textHint),
        const SizedBox(height: 16),
        Text('Нет заданий', style: TextStyle(fontSize: 18, color: context.textSecondary)),
      ]));
    }
    return RefreshIndicator(
      onRefresh: _load,
      child: ListView.builder(
        padding: const EdgeInsets.all(16),
        itemCount: tasks.length,
        itemBuilder: (_, i) => _buildTaskCard(tasks[i], i, canComplete),
      ),
    );
  }

  Widget _buildTaskCard(dynamic task, int index, bool canComplete) {
    final status = task['status'] ?? '';
    final Color statusColor;
    final IconData statusIcon;
    final String statusText;
    switch (status) {
      case 'Approved':
        statusColor = AppColors.success; statusIcon = Icons.check_circle; statusText = 'Одобрено';
      case 'Rejected':
        statusColor = AppColors.error; statusIcon = Icons.cancel; statusText = 'Отклонено';
      case 'Completed':
        statusColor = AppColors.warning; statusIcon = Icons.hourglass_top; statusText = 'На проверке';
      default:
        statusColor = AppColors.primary; statusIcon = Icons.radio_button_unchecked; statusText = 'Активно';
    }

    return TweenAnimationBuilder<double>(
      tween: Tween(begin: 0, end: 1),
      duration: Duration(milliseconds: 300 + index * 80),
      curve: Curves.easeOutCubic,
      builder: (_, val, child) => Opacity(opacity: val, child: Transform.translate(offset: Offset(0, 20 * (1 - val)), child: child)),
      child: Card(
        margin: const EdgeInsets.only(bottom: 12),
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
            Row(children: [
              Container(
                width: 44, height: 44,
                decoration: BoxDecoration(color: statusColor.withValues(alpha: 0.15), borderRadius: BorderRadius.circular(12)),
                child: Icon(statusIcon, color: statusColor),
              ),
              const SizedBox(width: 12),
              Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                Text(task['title'] ?? '', style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 16)),
                const SizedBox(height: 2),
                Text(statusText, style: TextStyle(color: statusColor, fontSize: 12, fontWeight: FontWeight.w500)),
              ])),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
                decoration: BoxDecoration(color: AppColors.accent.withValues(alpha: 0.15), borderRadius: BorderRadius.circular(12)),
                child: Text('${task['rewardAmount'] ?? 0} BYN', style: const TextStyle(fontWeight: FontWeight.bold, color: AppColors.accent)),
              ),
            ]),
            if (task['description'] != null && (task['description'] as String).isNotEmpty) ...[
              const SizedBox(height: 8),
              Text(task['description'], style: TextStyle(color: context.textSecondary, fontSize: 13)),
            ],
            if (status == 'Rejected' && task['rejectionReason'] != null) ...[
              const SizedBox(height: 8),
              Container(
                padding: const EdgeInsets.all(10),
                width: double.infinity,
                decoration: BoxDecoration(color: AppColors.error.withValues(alpha: 0.1), borderRadius: BorderRadius.circular(8)),
                child: Row(children: [
                  const Icon(Icons.info_outline, color: AppColors.error, size: 16),
                  const SizedBox(width: 8),
                  Expanded(child: Text(task['rejectionReason'], style: const TextStyle(color: AppColors.error, fontSize: 12))),
                ]),
              ),
            ],
            if (canComplete && status == 'Pending') ...[
              const SizedBox(height: 12),
              SizedBox(width: double.infinity, child: ElevatedButton.icon(
                onPressed: () => _completeTask(task),
                icon: const Icon(Icons.check, size: 18),
                label: const Text('Отметить выполненным'),
                style: ElevatedButton.styleFrom(backgroundColor: AppColors.kidGradient1),
              )),
            ],
          ]),
        ),
      ),
    );
  }

  Future<void> _completeTask(dynamic task) async {
    try {
      await context.read<ApiService>().post('tasks/${task['id']}/complete', body: {});
      _load();
      if (mounted) ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Задание отмечено как выполненное!')));
    } catch (e) {
      if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(e.toString())));
    }
  }
}
