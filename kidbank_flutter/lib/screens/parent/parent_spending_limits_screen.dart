import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/api_service.dart';
import '../../core/theme.dart';

class ParentSpendingLimitsScreen extends StatefulWidget {
  const ParentSpendingLimitsScreen({super.key});
  @override
  State<ParentSpendingLimitsScreen> createState() => _ParentSpendingLimitsScreenState();
}

class _ParentSpendingLimitsScreenState extends State<ParentSpendingLimitsScreen> {
  List<dynamic> _kids = [];
  Map<String, List<dynamic>> _limits = {};
  bool _loading = true;

  @override
  void initState() { super.initState(); _load(); }

  Future<void> _load() async {
    final api = context.read<ApiService>();
    try {
      final kids = await api.get('families/kids') as List;
      final limitsMap = <String, List<dynamic>>{};
      for (final kid in kids) {
        try {
          final l = await api.get('spendinglimits/kid/${kid['id']}');
          limitsMap[kid['id']] = l is List ? l : [];
        } catch (_) {
          limitsMap[kid['id'] as String] = [];
        }
      }
      setState(() { _kids = kids; _limits = limitsMap; _loading = false; });
    } catch (_) { setState(() => _loading = false); }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Лимиты расходов')),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : _kids.isEmpty
              ? Center(child: Column(mainAxisSize: MainAxisSize.min, children: [
                  Icon(Icons.block, size: 64, color: context.textHint),
                  const SizedBox(height: 16),
                  Text('Нет детей', style: TextStyle(fontSize: 18, color: context.textSecondary)),
                ]))
              : RefreshIndicator(
                  onRefresh: _load,
                  child: ListView.builder(
                    padding: const EdgeInsets.all(16),
                    itemCount: _kids.length,
                    itemBuilder: (_, i) => _buildKidLimits(_kids[i], i),
                  ),
                ),
    );
  }

  Widget _buildKidLimits(dynamic kid, int index) {
    final kidId = kid['id'] as String;
    final limits = _limits[kidId] ?? [];
    final activeLimit = limits.isNotEmpty
        ? limits.firstWhere((l) => l['isActive'] == true, orElse: () => null)
        : null;

    return TweenAnimationBuilder<double>(
      tween: Tween(begin: 0, end: 1),
      duration: Duration(milliseconds: 300 + index * 100),
      curve: Curves.easeOutCubic,
      builder: (_, val, child) => Opacity(opacity: val, child: Transform.translate(offset: Offset(0, 20 * (1 - val)), child: child)),
      child: Card(
        margin: const EdgeInsets.only(bottom: 16),
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
            Row(children: [
              CircleAvatar(
                radius: 22,
                backgroundColor: AppColors.kidGradient1.withValues(alpha: 0.15),
                child: Text('${kid['firstName']?[0] ?? ''}', style: const TextStyle(color: AppColors.kidGradient1, fontWeight: FontWeight.bold)),
              ),
              const SizedBox(width: 12),
              Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                Text('${kid['firstName'] ?? ''} ${kid['lastName'] ?? ''}', style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 16)),
                const SizedBox(height: 2),
                if (activeLimit != null) Text(
                  '${activeLimit['limitAmount']} BYN / ${_periodLabel(activeLimit['period'])}',
                  style: TextStyle(color: context.textSecondary, fontSize: 13),
                ) else Text('Лимит не установлен', style: TextStyle(color: context.textHint, fontSize: 13)),
              ])),
              if (activeLimit != null) Container(
                padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                decoration: BoxDecoration(color: AppColors.success.withValues(alpha: 0.15), borderRadius: BorderRadius.circular(12)),
                child: const Text('Активен', style: TextStyle(color: AppColors.success, fontSize: 12, fontWeight: FontWeight.w600)),
              ),
            ]),

            if (activeLimit != null) ...[
              const SizedBox(height: 16),
              _buildProgressBar(activeLimit),
            ],

            const SizedBox(height: 16),
            Row(children: [
              Expanded(child: OutlinedButton.icon(
                onPressed: () => _showSetLimit(kid, activeLimit),
                icon: Icon(activeLimit != null ? Icons.edit : Icons.add, size: 18),
                label: Text(activeLimit != null ? 'Изменить' : 'Установить лимит'),
              )),
            ]),
          ]),
        ),
      ),
    );
  }

  Widget _buildProgressBar(dynamic limit) {
    final total = (limit['limitAmount'] as num?)?.toDouble() ?? 1;
    final spent = (limit['spentAmount'] as num?)?.toDouble() ?? 0;
    final remaining = (limit['remainingAmount'] as num?)?.toDouble() ?? total;
    final percent = total > 0 ? (spent / total).clamp(0.0, 1.0) : 0.0;
    final isOver = spent >= total;

    return Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
      Row(mainAxisAlignment: MainAxisAlignment.spaceBetween, children: [
        Text('Потрачено: ${spent.toStringAsFixed(2)} BYN', style: TextStyle(fontSize: 13, color: context.textSecondary)),
        Text('Осталось: ${remaining.toStringAsFixed(2)} BYN', style: TextStyle(fontSize: 13, color: isOver ? AppColors.error : AppColors.success, fontWeight: FontWeight.w600)),
      ]),
      const SizedBox(height: 8),
      ClipRRect(
        borderRadius: BorderRadius.circular(4),
        child: LinearProgressIndicator(
          value: percent,
          backgroundColor: context.dividerColor,
          valueColor: AlwaysStoppedAnimation(isOver ? AppColors.error : percent > 0.8 ? AppColors.warning : AppColors.success),
          minHeight: 8,
        ),
      ),
      const SizedBox(height: 4),
      Text(
        'Период: ${_formatDate(limit['periodStartDate'])} — ${_formatDate(limit['periodEndDate'])}',
        style: TextStyle(fontSize: 11, color: context.textHint),
      ),
    ]);
  }

  String _formatDate(dynamic raw) {
    if (raw == null) return '—';
    try {
      final dt = DateTime.parse(raw.toString());
      return '${dt.day.toString().padLeft(2, '0')}.${dt.month.toString().padLeft(2, '0')}';
    } catch (_) {
      return raw.toString();
    }
  }

  String _periodLabel(String? period) {
    switch (period) {
      case 'Daily': return 'день';
      case 'Weekly': return 'неделю';
      case 'Monthly': return 'месяц';
      default: return period ?? 'период';
    }
  }

  void _showSetLimit(dynamic kid, dynamic existing) {
    final amountCtrl = TextEditingController(text: existing?['limitAmount']?.toString() ?? '');
    String period = existing?['period'] ?? 'Weekly';

    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      shape: const RoundedRectangleBorder(borderRadius: BorderRadius.vertical(top: Radius.circular(20))),
      builder: (ctx) => StatefulBuilder(
        builder: (ctx, setBS) => Padding(
          padding: EdgeInsets.fromLTRB(24, 24, 24, MediaQuery.of(ctx).viewInsets.bottom + 24),
          child: Column(mainAxisSize: MainAxisSize.min, children: [
            Container(width: 40, height: 4, decoration: BoxDecoration(color: context.dividerColor, borderRadius: BorderRadius.circular(2))),
            const SizedBox(height: 20),
            Text('Лимит для ${kid['firstName']}', style: const TextStyle(fontSize: 20, fontWeight: FontWeight.bold)),
            const SizedBox(height: 8),
            Text(
              'Установите максимальную сумму, которую ребёнок может потратить за выбранный период.',
              style: TextStyle(color: context.textSecondary, fontSize: 13),
              textAlign: TextAlign.center,
            ),
            const SizedBox(height: 20),
            TextField(
              controller: amountCtrl,
              decoration: const InputDecoration(labelText: 'Максимальная сумма (BYN)', prefixIcon: Icon(Icons.attach_money)),
              keyboardType: TextInputType.number,
            ),
            const SizedBox(height: 16),
            DropdownButtonFormField<String>(
              value: period,
              decoration: const InputDecoration(labelText: 'Период', prefixIcon: Icon(Icons.repeat)),
              items: const [
                DropdownMenuItem(value: 'Daily', child: Text('В день')),
                DropdownMenuItem(value: 'Weekly', child: Text('В неделю')),
                DropdownMenuItem(value: 'Monthly', child: Text('В месяц')),
              ],
              onChanged: (v) => setBS(() => period = v!),
            ),
            const SizedBox(height: 24),
            SizedBox(width: double.infinity, child: ElevatedButton.icon(
              onPressed: () async {
                Navigator.pop(ctx);
                try {
                  final amount = double.tryParse(amountCtrl.text) ?? 0;
                  if (amount <= 0) {
                    if (mounted) ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Введите сумму больше 0')));
                    return;
                  }
                  if (existing != null) {
                    await context.read<ApiService>().put('spendinglimits/${existing['id']}', body: {
                      'limitAmount': amount,
                    });
                  } else {
                    await context.read<ApiService>().post('spendinglimits', body: {
                      'kidId': kid['id'],
                      'limitAmount': amount,
                      'period': period,
                    });
                  }
                  _load();
                  if (mounted) ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Лимит установлен!')));
                } catch (e) {
                  if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(e.toString())));
                }
              },
              icon: const Icon(Icons.check),
              label: Text(existing != null ? 'Обновить лимит' : 'Установить лимит'),
            )),
          ]),
        ),
      ),
    );
  }
}
