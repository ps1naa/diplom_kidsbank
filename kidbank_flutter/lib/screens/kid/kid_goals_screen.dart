import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:percent_indicator/circular_percent_indicator.dart';
import '../../core/api_service.dart';
import '../../core/theme.dart';

class KidGoalsScreen extends StatefulWidget {
  const KidGoalsScreen({super.key});
  @override
  State<KidGoalsScreen> createState() => _KidGoalsScreenState();
}

class _KidGoalsScreenState extends State<KidGoalsScreen> {
  List<dynamic> _goals = [];
  bool _loading = true;

  @override
  void initState() { super.initState(); _load(); }

  Future<void> _load() async {
    try {
      final goals = await context.read<ApiService>().get('goals/my?includeCompleted=true');
      setState(() { _goals = goals as List; _loading = false; });
    } catch (_) { setState(() => _loading = false); }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Мои цели и вишлист')),
      floatingActionButton: FloatingActionButton.extended(
        heroTag: 'kidGoalsFab',
        onPressed: _createGoal,
        icon: const Icon(Icons.add),
        label: const Text('Новая цель'),
      ),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : _goals.isEmpty
              ? _buildEmpty()
              : RefreshIndicator(
                  onRefresh: _load,
                  child: ListView.builder(
                    padding: const EdgeInsets.all(16),
                    itemCount: _goals.length,
                    itemBuilder: (_, i) => _buildGoalCard(_goals[i], i),
                  ),
                ),
    );
  }

  Widget _buildEmpty() {
    return Center(child: Column(mainAxisSize: MainAxisSize.min, children: [
      Container(
        width: 80, height: 80,
        decoration: BoxDecoration(color: AppColors.accent.withValues(alpha: 0.1), borderRadius: BorderRadius.circular(20)),
        child: const Icon(Icons.flag, size: 40, color: AppColors.accent),
      ),
      const SizedBox(height: 16),
      Text('Нет целей', style: TextStyle(fontSize: 18, fontWeight: FontWeight.w600, color: context.textPrimary)),
      const SizedBox(height: 8),
      Text('Добавьте то, на что хотите накопить!', style: TextStyle(color: context.textSecondary)),
    ]));
  }

  Widget _buildGoalCard(dynamic goal, int index) {
    final target = (goal['targetAmount'] as num?)?.toDouble() ?? 1;
    final current = (goal['currentAmount'] as num?)?.toDouble() ?? 0;
    final percent = target > 0 ? (current / target).clamp(0.0, 1.0) : 0.0;
    final isCompleted = goal['status'] == 'Completed';

    return TweenAnimationBuilder<double>(
      tween: Tween(begin: 0, end: 1),
      duration: Duration(milliseconds: 300 + index * 80),
      curve: Curves.easeOutCubic,
      builder: (_, val, child) => Opacity(opacity: val, child: Transform.translate(offset: Offset(0, 20 * (1 - val)), child: child)),
      child: Card(
        margin: const EdgeInsets.only(bottom: 16),
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Column(children: [
            Row(children: [
              CircularPercentIndicator(
                radius: 32,
                lineWidth: 5,
                percent: percent,
                center: Text('${(percent * 100).toInt()}%', style: const TextStyle(fontSize: 12, fontWeight: FontWeight.bold)),
                progressColor: isCompleted ? AppColors.success : AppColors.kidGradient1,
                backgroundColor: context.dividerColor,
                circularStrokeCap: CircularStrokeCap.round,
                animation: true,
                animationDuration: 800,
              ),
              const SizedBox(width: 16),
              Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                Row(children: [
                  Expanded(child: Text(goal['title'] ?? '', style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 16))),
                  if (isCompleted) Container(
                    padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
                    decoration: BoxDecoration(color: AppColors.success.withValues(alpha: 0.15), borderRadius: BorderRadius.circular(8)),
                    child: const Text('Достигнута!', style: TextStyle(color: AppColors.success, fontSize: 11, fontWeight: FontWeight.w600)),
                  ),
                ]),
                const SizedBox(height: 4),
                Text('${current.toStringAsFixed(2)} / ${target.toStringAsFixed(2)} BYN', style: TextStyle(color: context.textSecondary, fontSize: 13)),
                const SizedBox(height: 4),
                ClipRRect(
                  borderRadius: BorderRadius.circular(4),
                  child: LinearProgressIndicator(
                    value: percent,
                    backgroundColor: context.dividerColor,
                    valueColor: AlwaysStoppedAnimation(isCompleted ? AppColors.success : AppColors.kidGradient1),
                    minHeight: 4,
                  ),
                ),
              ])),
            ]),
            if (!isCompleted) ...[
              const SizedBox(height: 12),
              Row(children: [
                Expanded(child: OutlinedButton(
                  onPressed: () => _deleteGoal(goal),
                  style: OutlinedButton.styleFrom(foregroundColor: AppColors.error, side: const BorderSide(color: AppColors.error)),
                  child: const Text('Удалить'),
                )),
                const SizedBox(width: 12),
                Expanded(child: ElevatedButton.icon(
                  onPressed: () => _depositToGoal(goal),
                  icon: const Icon(Icons.add, size: 18),
                  label: const Text('Пополнить'),
                )),
              ]),
            ],
          ]),
        ),
      ),
    );
  }

  void _createGoal() {
    final titleCtrl = TextEditingController();
    final targetCtrl = TextEditingController();
    showModalBottomSheet(
      context: context, isScrollControlled: true,
      shape: const RoundedRectangleBorder(borderRadius: BorderRadius.vertical(top: Radius.circular(20))),
      builder: (ctx) => Padding(
        padding: EdgeInsets.fromLTRB(24, 24, 24, MediaQuery.of(ctx).viewInsets.bottom + 24),
        child: Column(mainAxisSize: MainAxisSize.min, children: [
          Container(width: 40, height: 4, decoration: BoxDecoration(color: context.dividerColor, borderRadius: BorderRadius.circular(2))),
          const SizedBox(height: 20),
          Text('Новая цель', style: TextStyle(fontSize: 20, fontWeight: FontWeight.bold, color: context.textPrimary)),
          const SizedBox(height: 8),
          Text('На что хочешь накопить?', style: TextStyle(color: context.textSecondary)),
          const SizedBox(height: 20),
          TextField(controller: titleCtrl, decoration: const InputDecoration(labelText: 'Название', hintText: 'Например: iPhone, велосипед', prefixIcon: Icon(Icons.flag_outlined))),
          const SizedBox(height: 12),
          TextField(controller: targetCtrl, decoration: const InputDecoration(labelText: 'Целевая сумма (BYN)', prefixIcon: Icon(Icons.attach_money)), keyboardType: TextInputType.number),
          const SizedBox(height: 20),
          SizedBox(width: double.infinity, child: ElevatedButton(
            onPressed: () async {
              Navigator.pop(ctx);
              try {
                await context.read<ApiService>().post('goals', body: {
                  'title': titleCtrl.text.trim(),
                  'targetAmount': double.tryParse(targetCtrl.text) ?? 0,
                  'currency': 'BYN',
                });
                _load();
              } catch (e) {
                if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(e.toString())));
              }
            },
            child: const Text('Создать цель'),
          )),
        ]),
      ),
    );
  }

  void _depositToGoal(dynamic goal) {
    final ctrl = TextEditingController();
    showModalBottomSheet(
      context: context, isScrollControlled: true,
      shape: const RoundedRectangleBorder(borderRadius: BorderRadius.vertical(top: Radius.circular(20))),
      builder: (ctx) => Padding(
        padding: EdgeInsets.fromLTRB(24, 24, 24, MediaQuery.of(ctx).viewInsets.bottom + 24),
        child: Column(mainAxisSize: MainAxisSize.min, children: [
          Text('Пополнить "${goal['title']}"', style: TextStyle(fontSize: 20, fontWeight: FontWeight.bold, color: context.textPrimary)),
          const SizedBox(height: 20),
          TextField(controller: ctrl, decoration: const InputDecoration(labelText: 'Сумма (BYN)'), keyboardType: TextInputType.number),
          const SizedBox(height: 20),
          SizedBox(width: double.infinity, child: ElevatedButton(
            onPressed: () async {
              Navigator.pop(ctx);
              try {
                await context.read<ApiService>().post('goals/${goal['id']}/deposit', body: {'amount': double.tryParse(ctrl.text) ?? 0});
                _load();
              } catch (e) {
                if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(e.toString())));
              }
            },
            child: const Text('Пополнить'),
          )),
        ]),
      ),
    );
  }

  Future<void> _deleteGoal(dynamic goal) async {
    final confirmed = await showDialog<bool>(context: context, builder: (ctx) => AlertDialog(
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
      title: const Text('Удалить цель?'),
      content: Text('Удалить "${goal['title']}"? Накопленные средства будут возвращены.'),
      actions: [
        TextButton(onPressed: () => Navigator.pop(ctx, false), child: const Text('Отмена')),
        ElevatedButton(onPressed: () => Navigator.pop(ctx, true), style: ElevatedButton.styleFrom(backgroundColor: AppColors.error), child: const Text('Удалить')),
      ],
    ));
    if (confirmed == true) {
      try {
        await context.read<ApiService>().delete('goals/${goal['id']}');
        _load();
      } catch (e) {
        if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(e.toString())));
      }
    }
  }
}
