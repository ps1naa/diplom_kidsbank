import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:percent_indicator/circular_percent_indicator.dart';
import '../../core/api_service.dart';
import '../../core/theme.dart';

class ParentKidGoalsScreen extends StatefulWidget {
  final String kidId;
  final String kidName;
  const ParentKidGoalsScreen({super.key, required this.kidId, required this.kidName});
  @override
  State<ParentKidGoalsScreen> createState() => _ParentKidGoalsScreenState();
}

class _ParentKidGoalsScreenState extends State<ParentKidGoalsScreen> {
  List<dynamic> _goals = [];
  bool _loading = true;

  @override
  void initState() { super.initState(); _load(); }

  Future<void> _load() async {
    try {
      final goals = await context.read<ApiService>().get('goals/kid/${widget.kidId}?includeCompleted=true');
      setState(() { _goals = goals as List; _loading = false; });
    } catch (_) { setState(() => _loading = false); }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text('Цели: ${widget.kidName}')),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : _goals.isEmpty
              ? Center(child: Column(mainAxisSize: MainAxisSize.min, children: [
                  Icon(Icons.flag_outlined, size: 64, color: context.textHint),
                  const SizedBox(height: 16),
                  Text('Нет целей', style: TextStyle(fontSize: 18, color: context.textSecondary)),
                ]))
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
                progressColor: isCompleted ? AppColors.success : AppColors.primary,
                backgroundColor: context.dividerColor,
                circularStrokeCap: CircularStrokeCap.round,
              ),
              const SizedBox(width: 16),
              Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                Row(children: [
                  Expanded(child: Text(goal['title'] ?? '', style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 16))),
                  if (isCompleted) Container(
                    padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
                    decoration: BoxDecoration(color: AppColors.success.withValues(alpha: 0.15), borderRadius: BorderRadius.circular(8)),
                    child: const Text('Достигнута', style: TextStyle(color: AppColors.success, fontSize: 11, fontWeight: FontWeight.w600)),
                  ),
                ]),
                const SizedBox(height: 4),
                Text('${current.toStringAsFixed(2)} / ${target.toStringAsFixed(2)} BYN', style: TextStyle(color: context.textSecondary, fontSize: 13)),
              ])),
            ]),
            if (!isCompleted) ...[
              const SizedBox(height: 8),
              Text('Только ребёнок может пополнять свои цели', style: TextStyle(fontSize: 12, color: context.textHint)),
            ],
          ]),
        ),
      ),
    );
  }

}
