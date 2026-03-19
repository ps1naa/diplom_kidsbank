import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/api_service.dart';
import '../../core/theme.dart';

class ParentTasksScreen extends StatefulWidget {
  const ParentTasksScreen({super.key});
  @override
  State<ParentTasksScreen> createState() => _ParentTasksScreenState();
}

class _ParentTasksScreenState extends State<ParentTasksScreen> with SingleTickerProviderStateMixin {
  late TabController _tabCtrl;
  List<dynamic>? _pending;
  List<dynamic>? _templates;
  List<dynamic>? _allTasks;
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    _tabCtrl = TabController(length: 3, vsync: this);
    _load();
  }

  Future<void> _load() async {
    final api = context.read<ApiService>();
    try {
      final results = await Future.wait([
        api.get('tasks/pending-approval'),
        api.get('tasktemplates/my'),
        api.get('tasks/my'),
      ]);
      setState(() { _pending = results[0] as List; _templates = results[1] as List; _allTasks = results[2] as List; _loading = false; });
    } catch (_) { setState(() => _loading = false); }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Задания'),
        bottom: TabBar(controller: _tabCtrl, tabs: const [
          Tab(text: 'На проверку'),
          Tab(text: 'Все задания'),
          Tab(text: 'Шаблоны'),
        ]),
      ),
      floatingActionButton: FloatingActionButton(
        heroTag: 'parentTasksFab',
        onPressed: _showCreateMenu,
        child: const Icon(Icons.add),
      ),
      body: _loading ? const Center(child: CircularProgressIndicator()) : TabBarView(
        controller: _tabCtrl,
        children: [_buildPendingList(), _buildAllTasksList(), _buildTemplatesList()],
      ),
    );
  }

  Widget _buildPendingList() {
    if (_pending == null || _pending!.isEmpty) {
      return Center(child: Column(mainAxisSize: MainAxisSize.min, children: [
        Icon(Icons.check_circle_outline, size: 64, color: context.textHint),
        const SizedBox(height: 16),
        Text('Нет заданий на проверку', style: TextStyle(color: context.textSecondary)),
      ]));
    }
    return RefreshIndicator(
      onRefresh: _load,
      child: ListView.builder(
        padding: const EdgeInsets.all(16),
        itemCount: _pending!.length,
        itemBuilder: (_, i) => _buildPendingCard(_pending![i], i),
      ),
    );
  }

  Widget _buildPendingCard(dynamic t, int index) {
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
                decoration: BoxDecoration(color: AppColors.warning.withValues(alpha: 0.15), borderRadius: BorderRadius.circular(12)),
                child: const Icon(Icons.hourglass_top, color: AppColors.warning),
              ),
              const SizedBox(width: 12),
              Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                Text(t['title'] ?? '', style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 16)),
                Text(t['assignedToName'] ?? 'Ребёнок', style: TextStyle(color: context.textSecondary, fontSize: 13)),
              ])),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                decoration: BoxDecoration(color: AppColors.accent.withValues(alpha: 0.15), borderRadius: BorderRadius.circular(12)),
                child: Text('${t['rewardAmount'] ?? 0} BYN', style: const TextStyle(fontWeight: FontWeight.bold, color: AppColors.accent)),
              ),
            ]),
            const SizedBox(height: 16),
            Row(children: [
              Expanded(child: OutlinedButton.icon(
                onPressed: () => _rejectTask(t),
                icon: const Icon(Icons.close, size: 18),
                label: const Text('Отклонить'),
                style: OutlinedButton.styleFrom(foregroundColor: AppColors.error, side: const BorderSide(color: AppColors.error)),
              )),
              const SizedBox(width: 12),
              Expanded(child: ElevatedButton.icon(
                onPressed: () async {
                  await context.read<ApiService>().post('tasks/${t['id']}/approve');
                  _load();
                  if (mounted) ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Задание одобрено!')));
                },
                icon: const Icon(Icons.check, size: 18),
                label: const Text('Одобрить'),
              )),
            ]),
          ]),
        ),
      ),
    );
  }

  void _rejectTask(dynamic t) {
    final reasonCtrl = TextEditingController();
    showDialog(context: context, builder: (ctx) => AlertDialog(
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
      title: const Text('Отклонить задание'),
      content: Column(mainAxisSize: MainAxisSize.min, children: [
        Text('Отклонить "${t['title']}"?'),
        const SizedBox(height: 12),
        TextField(controller: reasonCtrl, decoration: const InputDecoration(labelText: 'Причина (необязательно)'), maxLines: 2),
      ]),
      actions: [
        TextButton(onPressed: () => Navigator.pop(ctx), child: const Text('Отмена')),
        ElevatedButton(
          onPressed: () async {
            Navigator.pop(ctx);
            await context.read<ApiService>().post('tasks/${t['id']}/reject', body: {'reason': reasonCtrl.text});
            _load();
          },
          style: ElevatedButton.styleFrom(backgroundColor: AppColors.error),
          child: const Text('Отклонить'),
        ),
      ],
    ));
  }

  Widget _buildAllTasksList() {
    if (_allTasks == null || _allTasks!.isEmpty) {
      return Center(child: Text('Нет заданий', style: TextStyle(color: context.textSecondary)));
    }
    return RefreshIndicator(
      onRefresh: _load,
      child: ListView.builder(
        padding: const EdgeInsets.all(16),
        itemCount: _allTasks!.length,
        itemBuilder: (_, i) {
          final t = _allTasks![i];
          final status = t['status'] ?? '';
          final Color sc;
          final String st;
          switch (status) {
            case 'Approved': sc = AppColors.success; st = 'Одобрено';
            case 'Completed': sc = AppColors.warning; st = 'На проверке';
            case 'Rejected': sc = AppColors.error; st = 'Отклонено';
            default: sc = AppColors.primary; st = 'Активно';
          }
          return Card(
            margin: const EdgeInsets.only(bottom: 8),
            child: ListTile(
              leading: Container(
                width: 40, height: 40,
                decoration: BoxDecoration(color: sc.withValues(alpha: 0.15), borderRadius: BorderRadius.circular(10)),
                child: Icon(status == 'Approved' ? Icons.check_circle : status == 'Rejected' ? Icons.cancel : Icons.assignment, color: sc, size: 20),
              ),
              title: Text(t['title'] ?? ''),
              subtitle: Text('${t['assignedToName'] ?? ''} • ${t['rewardAmount']} BYN • $st', style: TextStyle(fontSize: 12, color: context.textSecondary)),
            ),
          );
        },
      ),
    );
  }

  Widget _buildTemplatesList() {
    if (_templates == null || _templates!.isEmpty) {
      return Center(child: Text('Нет шаблонов', style: TextStyle(color: context.textSecondary)));
    }
    return RefreshIndicator(
      onRefresh: _load,
      child: ListView.builder(
        padding: const EdgeInsets.all(16),
        itemCount: _templates!.length,
        itemBuilder: (_, i) {
          final t = _templates![i];
          return Card(
            margin: const EdgeInsets.only(bottom: 8),
            child: ListTile(
              leading: Container(
                width: 40, height: 40,
                decoration: BoxDecoration(color: AppColors.accent.withValues(alpha: 0.15), borderRadius: BorderRadius.circular(10)),
                child: const Icon(Icons.description, color: AppColors.accent, size: 20),
              ),
              title: Text(t['title'] ?? ''),
              subtitle: Text('${t['rewardAmount']} ${t['currency'] ?? 'BYN'}'),
              trailing: Row(mainAxisSize: MainAxisSize.min, children: [
                IconButton(icon: const Icon(Icons.play_arrow, color: AppColors.primary), onPressed: () => _assignFromTemplate(t)),
                IconButton(icon: Icon(Icons.delete_outline, color: AppColors.error), onPressed: () async {
                  await context.read<ApiService>().delete('tasktemplates/${t['id']}');
                  _load();
                }),
              ]),
            ),
          );
        },
      ),
    );
  }

  void _showCreateMenu() {
    showModalBottomSheet(
      context: context,
      shape: const RoundedRectangleBorder(borderRadius: BorderRadius.vertical(top: Radius.circular(20))),
      builder: (ctx) => SafeArea(child: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(mainAxisSize: MainAxisSize.min, children: [
          Container(width: 40, height: 4, decoration: BoxDecoration(color: context.dividerColor, borderRadius: BorderRadius.circular(2))),
          const SizedBox(height: 20),
          ListTile(
            leading: Container(width: 44, height: 44, decoration: BoxDecoration(color: AppColors.primary.withValues(alpha: 0.15), borderRadius: BorderRadius.circular(12)), child: const Icon(Icons.add_task, color: AppColors.primary)),
            title: const Text('Новое задание', style: TextStyle(fontWeight: FontWeight.w600)),
            subtitle: Text('Назначить ребёнку напрямую', style: TextStyle(fontSize: 12, color: context.textSecondary)),
            onTap: () { Navigator.pop(ctx); _showCreateDirectTask(); },
          ),
          ListTile(
            leading: Container(width: 44, height: 44, decoration: BoxDecoration(color: AppColors.accent.withValues(alpha: 0.15), borderRadius: BorderRadius.circular(12)), child: const Icon(Icons.description, color: AppColors.accent)),
            title: const Text('Новый шаблон', style: TextStyle(fontWeight: FontWeight.w600)),
            subtitle: Text('Сохранить для повторного использования', style: TextStyle(fontSize: 12, color: context.textSecondary)),
            onTap: () { Navigator.pop(ctx); _showCreateTemplate(); },
          ),
        ]),
      )),
    );
  }

  void _showCreateDirectTask() async {
    final kids = await context.read<ApiService>().get('families/kids') as List;
    if (!mounted || kids.isEmpty) return;
    final titleCtrl = TextEditingController();
    final descCtrl = TextEditingController();
    final rewardCtrl = TextEditingController();
    String? selectedKidId;
    showModalBottomSheet(
      context: context, isScrollControlled: true,
      shape: const RoundedRectangleBorder(borderRadius: BorderRadius.vertical(top: Radius.circular(20))),
      builder: (ctx) => StatefulBuilder(builder: (ctx, setBS) => Padding(
        padding: EdgeInsets.fromLTRB(24, 24, 24, MediaQuery.of(ctx).viewInsets.bottom + 24),
        child: Column(mainAxisSize: MainAxisSize.min, children: [
          Text('Новое задание', style: TextStyle(fontSize: 20, fontWeight: FontWeight.bold, color: context.textPrimary)),
          const SizedBox(height: 16),
          TextField(controller: titleCtrl, decoration: const InputDecoration(labelText: 'Название')),
          const SizedBox(height: 12),
          TextField(controller: descCtrl, decoration: const InputDecoration(labelText: 'Описание'), maxLines: 2),
          const SizedBox(height: 12),
          TextField(controller: rewardCtrl, decoration: const InputDecoration(labelText: 'Награда (BYN)'), keyboardType: TextInputType.number),
          const SizedBox(height: 12),
          DropdownButtonFormField<String>(
            decoration: const InputDecoration(labelText: 'Ребёнок'),
            items: kids.map<DropdownMenuItem<String>>((k) => DropdownMenuItem(value: k['id'], child: Text('${k['firstName']} ${k['lastName']}'))).toList(),
            onChanged: (v) => setBS(() => selectedKidId = v),
          ),
          const SizedBox(height: 16),
          SizedBox(width: double.infinity, child: ElevatedButton(
            onPressed: selectedKidId != null ? () async {
              Navigator.pop(ctx);
              await context.read<ApiService>().post('tasks', body: {
                'title': titleCtrl.text.trim(),
                'description': descCtrl.text.trim(),
                'rewardAmount': double.tryParse(rewardCtrl.text) ?? 0,
                'assignedToId': selectedKidId,
              });
              _load();
            } : null,
            child: const Text('Создать'),
          )),
        ]),
      )),
    );
  }

  void _showCreateTemplate() {
    final titleCtrl = TextEditingController();
    final rewardCtrl = TextEditingController();
    showModalBottomSheet(
      context: context, isScrollControlled: true,
      shape: const RoundedRectangleBorder(borderRadius: BorderRadius.vertical(top: Radius.circular(20))),
      builder: (ctx) => Padding(
        padding: EdgeInsets.fromLTRB(24, 24, 24, MediaQuery.of(ctx).viewInsets.bottom + 24),
        child: Column(mainAxisSize: MainAxisSize.min, children: [
          Text('Новый шаблон', style: TextStyle(fontSize: 20, fontWeight: FontWeight.bold, color: context.textPrimary)),
          const SizedBox(height: 16),
          TextField(controller: titleCtrl, decoration: const InputDecoration(labelText: 'Название')),
          const SizedBox(height: 12),
          TextField(controller: rewardCtrl, decoration: const InputDecoration(labelText: 'Награда (BYN)'), keyboardType: TextInputType.number),
          const SizedBox(height: 16),
          SizedBox(width: double.infinity, child: ElevatedButton(
            onPressed: () async {
              Navigator.pop(ctx);
              await context.read<ApiService>().post('tasktemplates', body: {'title': titleCtrl.text.trim(), 'rewardAmount': double.tryParse(rewardCtrl.text) ?? 0});
              _load();
            },
            child: const Text('Создать шаблон'),
          )),
        ]),
      ),
    );
  }

  void _assignFromTemplate(dynamic tpl) async {
    final api = context.read<ApiService>();
    final kids = await api.get('families/kids') as List;
    if (!mounted || kids.isEmpty) return;
    showDialog(context: context, builder: (ctx) => SimpleDialog(
      title: const Text('Назначить ребёнку'),
      children: kids.map<Widget>((k) => SimpleDialogOption(
        onPressed: () async {
          Navigator.pop(ctx);
          await api.post('tasktemplates/${tpl['id']}/assign', body: {'assignedToId': k['id']});
          if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Назначено ${k['firstName']}')));
          _load();
        },
        child: Text('${k['firstName']} ${k['lastName']}'),
      )).toList(),
    ));
  }

  @override
  void dispose() { _tabCtrl.dispose(); super.dispose(); }
}
