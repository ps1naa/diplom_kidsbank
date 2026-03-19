import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:intl/intl.dart';
import '../../core/api_service.dart';
import '../../core/theme.dart';

class ParentTxHistoryScreen extends StatefulWidget {
  final String kidId;
  final String kidName;
  final String accountId;
  const ParentTxHistoryScreen({super.key, required this.kidId, required this.kidName, required this.accountId});
  @override
  State<ParentTxHistoryScreen> createState() => _ParentTxHistoryScreenState();
}

class _ParentTxHistoryScreenState extends State<ParentTxHistoryScreen> {
  List<dynamic> _transactions = [];
  bool _loading = true;
  int _page = 1;
  bool _hasMore = true;
  final _scrollCtrl = ScrollController();

  @override
  void initState() {
    super.initState();
    _load();
    _scrollCtrl.addListener(() {
      if (_scrollCtrl.position.pixels >= _scrollCtrl.position.maxScrollExtent - 200 && _hasMore && !_loading) {
        _loadMore();
      }
    });
  }

  Future<void> _load() async {
    _page = 1;
    try {
      final data = await context.read<ApiService>().get('accounts/${widget.accountId}/transactions?pageNumber=1&pageSize=20');
      final items = data is List ? data : (data['items'] ?? []) as List;
      setState(() { _transactions = items; _loading = false; _hasMore = items.length >= 20; });
    } catch (_) { setState(() => _loading = false); }
  }

  Future<void> _loadMore() async {
    _page++;
    setState(() => _loading = true);
    try {
      final data = await context.read<ApiService>().get('accounts/${widget.accountId}/transactions?pageNumber=$_page&pageSize=20');
      final items = data is List ? data : (data['items'] ?? []) as List;
      setState(() { _transactions.addAll(items); _loading = false; _hasMore = items.length >= 20; });
    } catch (_) { setState(() => _loading = false); }
  }

  @override
  void dispose() { _scrollCtrl.dispose(); super.dispose(); }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text('Транзакции: ${widget.kidName}')),
      body: _loading && _transactions.isEmpty
          ? const Center(child: CircularProgressIndicator())
          : _transactions.isEmpty
              ? Center(child: Column(mainAxisSize: MainAxisSize.min, children: [
                  Icon(Icons.receipt_long, size: 64, color: context.textHint),
                  const SizedBox(height: 16),
                  Text('Нет транзакций', style: TextStyle(fontSize: 18, color: context.textSecondary)),
                ]))
              : RefreshIndicator(
                  onRefresh: _load,
                  child: ListView.builder(
                    controller: _scrollCtrl,
                    padding: const EdgeInsets.all(16),
                    itemCount: _transactions.length + (_loading ? 1 : 0),
                    itemBuilder: (_, i) {
                      if (i == _transactions.length) return const Center(child: Padding(padding: EdgeInsets.all(16), child: CircularProgressIndicator()));
                      return _buildTransaction(_transactions[i], i);
                    },
                  ),
                ),
    );
  }

  Widget _buildTransaction(dynamic tx, int index) {
    final type = tx['type'] ?? '';
    final amount = (tx['amount'] as num?)?.toStringAsFixed(2) ?? '0.00';
    final isIncoming = type == 'Deposit' || type == 'Reward' || type == 'Allowance';
    final date = tx['createdAt'] != null ? DateFormat('dd.MM.yyyy HH:mm').format(DateTime.parse(tx['createdAt']).toLocal()) : '';
    return TweenAnimationBuilder<double>(
      tween: Tween(begin: 0, end: 1),
      duration: Duration(milliseconds: 200 + index * 40),
      curve: Curves.easeOutCubic,
      builder: (_, val, child) => Opacity(opacity: val, child: Transform.translate(offset: Offset(0, 10 * (1 - val)), child: child)),
      child: Card(
        margin: const EdgeInsets.only(bottom: 8),
        child: ListTile(
          leading: Container(
            width: 44, height: 44,
            decoration: BoxDecoration(
              color: (isIncoming ? AppColors.success : AppColors.error).withValues(alpha: 0.15),
              borderRadius: BorderRadius.circular(12),
            ),
            child: Icon(
              isIncoming ? Icons.arrow_downward : Icons.arrow_upward,
              color: isIncoming ? AppColors.success : AppColors.error,
            ),
          ),
          title: Text(tx['description'] ?? type, style: const TextStyle(fontWeight: FontWeight.w500)),
          subtitle: Text(date, style: TextStyle(fontSize: 12, color: context.textSecondary)),
          trailing: Text(
            '${isIncoming ? '+' : '-'}$amount BYN',
            style: TextStyle(fontWeight: FontWeight.bold, color: isIncoming ? AppColors.success : AppColors.error),
          ),
        ),
      ),
    );
  }
}
