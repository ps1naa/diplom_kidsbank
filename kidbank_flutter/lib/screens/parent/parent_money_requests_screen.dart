import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/api_service.dart';
import '../../core/theme.dart';

class ParentMoneyRequestsScreen extends StatefulWidget {
  const ParentMoneyRequestsScreen({super.key});
  @override
  State<ParentMoneyRequestsScreen> createState() => _ParentMoneyRequestsScreenState();
}

class _ParentMoneyRequestsScreenState extends State<ParentMoneyRequestsScreen> {
  List<dynamic> _requests = [];
  bool _loading = true;

  @override
  void initState() { super.initState(); _load(); }

  Future<void> _load() async {
    try {
      final data = await context.read<ApiService>().get('moneyrequests/pending');
      setState(() { _requests = data as List; _loading = false; });
    } catch (_) { setState(() => _loading = false); }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Запросы денег')),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : _requests.isEmpty
              ? Center(child: Column(mainAxisSize: MainAxisSize.min, children: [
                  Container(
                    width: 80, height: 80,
                    decoration: BoxDecoration(color: AppColors.accent.withValues(alpha: 0.1), borderRadius: BorderRadius.circular(20)),
                    child: const Icon(Icons.inbox, size: 40, color: AppColors.accent),
                  ),
                  const SizedBox(height: 16),
                  Text('Нет запросов', style: TextStyle(fontSize: 18, fontWeight: FontWeight.w600, color: context.textPrimary)),
                  const SizedBox(height: 8),
                  Text('Все запросы обработаны', style: TextStyle(color: context.textSecondary)),
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
              CircleAvatar(
                radius: 22,
                backgroundColor: AppColors.accent.withValues(alpha: 0.15),
                child: Text(
                  (req['kidName'] ?? 'К')[0],
                  style: const TextStyle(fontWeight: FontWeight.bold, color: AppColors.accent),
                ),
              ),
              const SizedBox(width: 12),
              Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                Text(req['kidName'] ?? 'Ребёнок', style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 16)),
                const SizedBox(height: 2),
                Text('Запрашивает деньги', style: TextStyle(color: context.textSecondary, fontSize: 13)),
              ])),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                decoration: BoxDecoration(color: AppColors.accent.withValues(alpha: 0.15), borderRadius: BorderRadius.circular(12)),
                child: Text('${(req['amount'] as num?)?.toStringAsFixed(2) ?? '0.00'} BYN',
                    style: const TextStyle(fontWeight: FontWeight.bold, color: AppColors.accent, fontSize: 16)),
              ),
            ]),
            if (req['reason'] != null && (req['reason'] as String).isNotEmpty) ...[
              const SizedBox(height: 12),
              Container(
                padding: const EdgeInsets.all(12),
                width: double.infinity,
                decoration: BoxDecoration(
                  color: context.surfaceVariant,
                  borderRadius: BorderRadius.circular(12),
                ),
                child: Row(crossAxisAlignment: CrossAxisAlignment.start, children: [
                  Icon(Icons.message, size: 16, color: context.textSecondary),
                  const SizedBox(width: 8),
                  Expanded(child: Text(req['reason'], style: TextStyle(color: context.textSecondary))),
                ]),
              ),
            ],
            const SizedBox(height: 16),
            Row(children: [
              Expanded(child: OutlinedButton.icon(
                onPressed: () => _handleReject(req),
                icon: const Icon(Icons.close, size: 18),
                label: const Text('Отклонить'),
                style: OutlinedButton.styleFrom(foregroundColor: AppColors.error, side: const BorderSide(color: AppColors.error)),
              )),
              const SizedBox(width: 12),
              Expanded(child: ElevatedButton.icon(
                onPressed: () => _handleApprove(req),
                icon: const Icon(Icons.check, size: 18),
                label: const Text('Одобрить'),
              )),
            ]),
          ]),
        ),
      ),
    );
  }

  Future<void> _handleApprove(dynamic req) async {
    final noteCtrl = TextEditingController();
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
        title: const Text('Одобрить запрос?'),
        content: Column(mainAxisSize: MainAxisSize.min, children: [
          Text('Перевести ${req['amount']} BYN для ${req['kidName']}?'),
          const SizedBox(height: 12),
          TextField(controller: noteCtrl, decoration: const InputDecoration(labelText: 'Комментарий (необязательно)', hintText: 'Например: на карманные расходы')),
        ]),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx, false), child: const Text('Отмена')),
          ElevatedButton(onPressed: () => Navigator.pop(ctx, true), child: const Text('Одобрить')),
        ],
      ),
    );
    if (confirmed == true) {
      try {
        await context.read<ApiService>().post('moneyrequests/${req['id']}/approve', body: {'note': noteCtrl.text});
        _load();
        if (mounted) ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Запрос одобрен!')));
      } catch (e) {
        if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(e.toString())));
      }
    }
  }

  Future<void> _handleReject(dynamic req) async {
    final noteCtrl = TextEditingController();
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
        title: const Text('Отклонить запрос?'),
        content: Column(mainAxisSize: MainAxisSize.min, children: [
          Text('Отклонить запрос от ${req['kidName']} на ${req['amount']} BYN?'),
          const SizedBox(height: 12),
          TextField(controller: noteCtrl, decoration: const InputDecoration(labelText: 'Причина отклонения'), maxLines: 2),
        ]),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx, false), child: const Text('Отмена')),
          ElevatedButton(
            onPressed: () => Navigator.pop(ctx, true),
            style: ElevatedButton.styleFrom(backgroundColor: AppColors.error),
            child: const Text('Отклонить'),
          ),
        ],
      ),
    );
    if (confirmed == true) {
      try {
        await context.read<ApiService>().post('moneyrequests/${req['id']}/reject', body: {'note': noteCtrl.text});
        _load();
        if (mounted) ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Запрос отклонён')));
      } catch (e) {
        if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(e.toString())));
      }
    }
  }
}
