import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:intl/intl.dart';
import '../../core/api_service.dart';
import '../../core/theme.dart';

class KidMoneyRequestScreen extends StatefulWidget {
  const KidMoneyRequestScreen({super.key});
  @override
  State<KidMoneyRequestScreen> createState() => _KidMoneyRequestScreenState();
}

class _KidMoneyRequestScreenState extends State<KidMoneyRequestScreen> {
  List<dynamic> _requests = [];
  bool _loading = true;

  @override
  void initState() { super.initState(); _load(); }

  Future<void> _load() async {
    try {
      final data = await context.read<ApiService>().get('moneyrequests/my');
      setState(() { _requests = data as List; _loading = false; });
    } catch (_) { setState(() => _loading = false); }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Мои запросы')),
      floatingActionButton: FloatingActionButton.extended(
        heroTag: 'kidMoneyRequestFab',
        onPressed: _createRequest,
        icon: const Icon(Icons.add),
        label: const Text('Запросить'),
      ),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : _requests.isEmpty
              ? Center(child: Column(mainAxisSize: MainAxisSize.min, children: [
                  Icon(Icons.attach_money, size: 64, color: context.textHint),
                  const SizedBox(height: 16),
                  Text('Нет запросов', style: TextStyle(fontSize: 18, color: context.textSecondary)),
                  const SizedBox(height: 8),
                  Text('Запросите деньги у родителей', style: TextStyle(color: context.textHint)),
                ]))
              : RefreshIndicator(
                  onRefresh: _load,
                  child: ListView.builder(
                    padding: const EdgeInsets.all(16),
                    itemCount: _requests.length,
                    itemBuilder: (_, i) => _buildRequest(_requests[i], i),
                  ),
                ),
    );
  }

  Widget _buildRequest(dynamic req, int index) {
    final status = req['status'] ?? '';
    final Color statusColor;
    final String statusText;
    final IconData statusIcon;
    switch (status) {
      case 'Approved':
        statusColor = AppColors.success; statusText = 'Одобрен'; statusIcon = Icons.check_circle;
      case 'Rejected':
        statusColor = AppColors.error; statusText = 'Отклонён'; statusIcon = Icons.cancel;
      default:
        statusColor = AppColors.warning; statusText = 'Ожидает'; statusIcon = Icons.hourglass_top;
    }

    final date = req['createdAt'] != null
        ? DateFormat('dd.MM.yyyy HH:mm').format(DateTime.parse(req['createdAt']).toLocal()) : '';

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
                Text('${(req['amount'] as num?)?.toStringAsFixed(2) ?? '0.00'} BYN',
                    style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 18)),
                const SizedBox(height: 2),
                Text(date, style: TextStyle(fontSize: 12, color: context.textSecondary)),
              ])),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                decoration: BoxDecoration(color: statusColor.withValues(alpha: 0.15), borderRadius: BorderRadius.circular(12)),
                child: Text(statusText, style: TextStyle(color: statusColor, fontSize: 12, fontWeight: FontWeight.w600)),
              ),
            ]),
            if (req['reason'] != null && (req['reason'] as String).isNotEmpty) ...[
              const SizedBox(height: 8),
              Text(req['reason'], style: TextStyle(color: context.textSecondary, fontSize: 13)),
            ],
            if (req['responseNote'] != null && (req['responseNote'] as String).isNotEmpty) ...[
              const SizedBox(height: 8),
              Container(
                padding: const EdgeInsets.all(10),
                width: double.infinity,
                decoration: BoxDecoration(color: statusColor.withValues(alpha: 0.08), borderRadius: BorderRadius.circular(8)),
                child: Row(crossAxisAlignment: CrossAxisAlignment.start, children: [
                  Icon(Icons.message, size: 14, color: statusColor),
                  const SizedBox(width: 8),
                  Expanded(child: Text(req['responseNote'], style: TextStyle(color: statusColor, fontSize: 12))),
                ]),
              ),
            ],
          ]),
        ),
      ),
    );
  }

  void _createRequest() {
    final amountCtrl = TextEditingController();
    final reasonCtrl = TextEditingController();
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      shape: const RoundedRectangleBorder(borderRadius: BorderRadius.vertical(top: Radius.circular(20))),
      builder: (ctx) => Padding(
        padding: EdgeInsets.fromLTRB(24, 24, 24, MediaQuery.of(ctx).viewInsets.bottom + 24),
        child: Column(mainAxisSize: MainAxisSize.min, children: [
          Container(width: 40, height: 4, decoration: BoxDecoration(color: context.dividerColor, borderRadius: BorderRadius.circular(2))),
          const SizedBox(height: 20),
          Text('Запросить деньги', style: TextStyle(fontSize: 20, fontWeight: FontWeight.bold, color: context.textPrimary)),
          const SizedBox(height: 20),
          TextField(controller: amountCtrl, decoration: const InputDecoration(labelText: 'Сумма (BYN)', prefixIcon: Icon(Icons.attach_money)), keyboardType: TextInputType.number),
          const SizedBox(height: 12),
          TextField(controller: reasonCtrl, decoration: const InputDecoration(labelText: 'Причина', hintText: 'Зачем нужны деньги?', prefixIcon: Icon(Icons.message_outlined)), maxLines: 2),
          const SizedBox(height: 20),
          SizedBox(width: double.infinity, child: ElevatedButton.icon(
            onPressed: () async {
              Navigator.pop(ctx);
              try {
                await context.read<ApiService>().post('moneyrequests', body: {
                  'amount': double.tryParse(amountCtrl.text) ?? 0,
                  'currency': 'BYN',
                  'reason': reasonCtrl.text.trim(),
                });
                _load();
                if (mounted) ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Запрос отправлен!')));
              } catch (e) {
                if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(e.toString())));
              }
            },
            icon: const Icon(Icons.send),
            label: const Text('Отправить запрос'),
            style: ElevatedButton.styleFrom(padding: const EdgeInsets.symmetric(vertical: 14)),
          )),
        ]),
      ),
    );
  }
}
