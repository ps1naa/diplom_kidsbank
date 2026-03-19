import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/api_service.dart';
import '../../core/theme.dart';

class ParentAllowanceScreen extends StatefulWidget {
  const ParentAllowanceScreen({super.key});
  @override
  State<ParentAllowanceScreen> createState() => _ParentAllowanceScreenState();
}

class _ParentAllowanceScreenState extends State<ParentAllowanceScreen> {
  List<dynamic> _kids = [];
  Map<String, dynamic> _allowances = {};
  bool _loading = true;

  @override
  void initState() { super.initState(); _load(); }

  Future<void> _load() async {
    final api = context.read<ApiService>();
    try {
      final kids = await api.get('families/kids') as List;
      final map = <String, dynamic>{};
      for (final kid in kids) {
        try {
          final a = await api.get('allowances/kid/${kid['id']}');
          map[kid['id']] = a;
        } catch (_) {}
      }
      setState(() { _kids = kids; _allowances = map; _loading = false; });
    } catch (_) { setState(() => _loading = false); }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Авто-пополнение')),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : _kids.isEmpty
              ? Center(child: Column(mainAxisSize: MainAxisSize.min, children: [
                  Icon(Icons.schedule, size: 64, color: context.textHint),
                  const SizedBox(height: 16),
                  Text('Нет детей', style: TextStyle(fontSize: 18, color: context.textSecondary)),
                ]))
              : RefreshIndicator(
                  onRefresh: _load,
                  child: ListView.builder(
                    padding: const EdgeInsets.all(16),
                    itemCount: _kids.length,
                    itemBuilder: (_, i) => _buildKidAllowance(_kids[i], i),
                  ),
                ),
    );
  }

  Widget _buildKidAllowance(dynamic kid, int index) {
    final allowance = _allowances[kid['id']];
    final isActive = allowance != null && allowance['isActive'] == true;
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
                backgroundColor: AppColors.primary.withValues(alpha: 0.15),
                child: Text('${kid['firstName'][0]}', style: const TextStyle(color: AppColors.primary, fontWeight: FontWeight.bold)),
              ),
              const SizedBox(width: 12),
              Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                Text('${kid['firstName']} ${kid['lastName']}', style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 16)),
                if (isActive) Text(
                  '${allowance['amount']} BYN / ${_periodLabel(allowance['frequency'])}',
                  style: TextStyle(color: context.textSecondary, fontSize: 13),
                ) else Text('Не настроено', style: TextStyle(color: context.textHint, fontSize: 13)),
              ])),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                decoration: BoxDecoration(
                  color: (isActive ? AppColors.success : context.textHint).withValues(alpha: 0.15),
                  borderRadius: BorderRadius.circular(12),
                ),
                child: Text(
                  isActive ? 'Активно' : 'Выкл',
                  style: TextStyle(color: isActive ? AppColors.success : context.textHint, fontSize: 12, fontWeight: FontWeight.w600),
                ),
              ),
            ]),
            const SizedBox(height: 16),
            SizedBox(
              width: double.infinity,
              child: OutlinedButton.icon(
                onPressed: () => _configureAllowance(kid, allowance),
                icon: Icon(isActive ? Icons.edit : Icons.add, size: 18),
                label: Text(isActive ? 'Изменить' : 'Настроить'),
              ),
            ),
          ]),
        ),
      ),
    );
  }

  String _periodLabel(String? period) {
    switch (period) {
      case 'Weekly': return 'неделю';
      case 'Monthly': return 'месяц';
      case 'Daily': return 'день';
      default: return period ?? 'период';
    }
  }

  void _configureAllowance(dynamic kid, dynamic current) {
    final amountCtrl = TextEditingController(text: current?['amount']?.toString() ?? '');
    String period = current?['frequency'] ?? 'Weekly';
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
            Text('Авто-пополнение для ${kid['firstName']}', style: const TextStyle(fontSize: 20, fontWeight: FontWeight.bold)),
            const SizedBox(height: 20),
            TextField(controller: amountCtrl, decoration: const InputDecoration(labelText: 'Сумма (BYN)', prefixIcon: Icon(Icons.attach_money)), keyboardType: TextInputType.number),
            const SizedBox(height: 16),
            DropdownButtonFormField<String>(
              value: period,
              decoration: const InputDecoration(labelText: 'Период', prefixIcon: Icon(Icons.repeat)),
              items: const [
                DropdownMenuItem(value: 'Daily', child: Text('Ежедневно')),
                DropdownMenuItem(value: 'Weekly', child: Text('Еженедельно')),
                DropdownMenuItem(value: 'Monthly', child: Text('Ежемесячно')),
              ],
              onChanged: (v) => setBS(() => period = v!),
            ),
            const SizedBox(height: 24),
            SizedBox(width: double.infinity, child: ElevatedButton(
              onPressed: () async {
                Navigator.pop(ctx);
                try {
                  await context.read<ApiService>().post('allowances', body: {
                    'kidId': kid['id'],
                    'amount': double.tryParse(amountCtrl.text) ?? 0,
                    'frequency': period,
                  });
                  _load();
                  if (mounted) ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Настроено!')));
                } catch (e) {
                  if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(e.toString())));
                }
              },
              child: const Text('Сохранить'),
            )),
          ]),
        ),
      ),
    );
  }
}
