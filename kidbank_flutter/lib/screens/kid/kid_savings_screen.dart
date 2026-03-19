import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/api_service.dart';
import '../../core/theme.dart';

class KidSavingsScreen extends StatefulWidget {
  const KidSavingsScreen({super.key});
  @override
  State<KidSavingsScreen> createState() => _KidSavingsScreenState();
}

class _KidSavingsScreenState extends State<KidSavingsScreen> {
  List<dynamic> _accounts = [];
  bool _loading = true;

  @override
  void initState() { super.initState(); _load(); }

  Future<void> _load() async {
    try {
      final all = await context.read<ApiService>().get('accounts/my') as List;
      setState(() {
        _accounts = all.where((a) => a['type'] == 'Savings').toList();
        _loading = false;
      });
    } catch (_) { setState(() => _loading = false); }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Копилка')),
      floatingActionButton: FloatingActionButton.extended(
        heroTag: 'kidSavingsFab',
        onPressed: _createSavings,
        icon: const Icon(Icons.add),
        label: const Text('Новая копилка'),
      ),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : _accounts.isEmpty
              ? _buildEmpty()
              : RefreshIndicator(
                  onRefresh: _load,
                  child: ListView.builder(
                    padding: const EdgeInsets.all(16),
                    itemCount: _accounts.length,
                    itemBuilder: (_, i) => _buildSavingsCard(_accounts[i], i),
                  ),
                ),
    );
  }

  Widget _buildEmpty() {
    return Center(child: Column(mainAxisSize: MainAxisSize.min, children: [
      Container(
        width: 80, height: 80,
        decoration: BoxDecoration(color: AppColors.accent.withValues(alpha: 0.1), borderRadius: BorderRadius.circular(20)),
        child: const Icon(Icons.savings, size: 40, color: AppColors.accent),
      ),
      const SizedBox(height: 16),
      Text('Нет копилок', style: TextStyle(fontSize: 18, fontWeight: FontWeight.w600, color: context.textPrimary)),
      const SizedBox(height: 8),
      Text('Создайте свою первую копилку!', style: TextStyle(color: context.textSecondary)),
      const SizedBox(height: 24),
      ElevatedButton.icon(
        onPressed: _createSavings,
        icon: const Icon(Icons.add),
        label: const Text('Создать копилку'),
      ),
    ]));
  }

  Widget _buildSavingsCard(dynamic account, int index) {
    final balance = (account['balance'] as num?)?.toDouble() ?? 0;
    final name = account['name'] ?? 'Копилка';

    return TweenAnimationBuilder<double>(
      tween: Tween(begin: 0, end: 1),
      duration: Duration(milliseconds: 400 + index * 100),
      curve: Curves.easeOutCubic,
      builder: (_, val, child) => Opacity(opacity: val, child: Transform.translate(offset: Offset(0, 30 * (1 - val)), child: child)),
      child: Card(
        margin: const EdgeInsets.only(bottom: 16),
        child: Padding(
          padding: const EdgeInsets.all(20),
          child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
            Row(children: [
              Container(
                width: 52, height: 52,
                decoration: BoxDecoration(
                  gradient: const LinearGradient(colors: [AppColors.accent, Color(0xFFFF8F00)]),
                  borderRadius: BorderRadius.circular(14),
                ),
                child: const Icon(Icons.savings, color: Colors.white, size: 28),
              ),
              const SizedBox(width: 16),
              Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                Text(name, style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 16)),
                const SizedBox(height: 4),
                Row(children: [
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
                    decoration: BoxDecoration(color: AppColors.success.withValues(alpha: 0.15), borderRadius: BorderRadius.circular(8)),
                    child: const Text('Накопительный', style: TextStyle(color: AppColors.success, fontSize: 11, fontWeight: FontWeight.w600)),
                  ),
                ]),
              ])),
            ]),
            const SizedBox(height: 20),
            Container(
              width: double.infinity,
              padding: const EdgeInsets.all(16),
              decoration: BoxDecoration(color: context.surfaceVariant, borderRadius: BorderRadius.circular(12)),
              child: Column(children: [
                Text('Накоплено', style: TextStyle(color: context.textSecondary, fontSize: 12)),
                const SizedBox(height: 4),
                Text('${balance.toStringAsFixed(2)} BYN',
                    style: TextStyle(fontSize: 28, fontWeight: FontWeight.bold, color: context.textPrimary)),
              ]),
            ),
            const SizedBox(height: 16),
            Row(children: [
              Expanded(child: OutlinedButton.icon(
                onPressed: () => _withdraw(account),
                icon: const Icon(Icons.arrow_upward, size: 18),
                label: const Text('Снять'),
              )),
              const SizedBox(width: 12),
              Expanded(child: ElevatedButton.icon(
                onPressed: () => _deposit(account),
                icon: const Icon(Icons.arrow_downward, size: 18),
                label: const Text('Пополнить'),
              )),
            ]),
            const SizedBox(height: 8),
            Text('С подпиской Pro: 5% годовых и досрочный вывод', style: TextStyle(fontSize: 11, color: context.textHint), textAlign: TextAlign.center),
          ]),
        ),
      ),
    );
  }

  void _createSavings() {
    final nameCtrl = TextEditingController(text: 'Моя копилка');
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      shape: const RoundedRectangleBorder(borderRadius: BorderRadius.vertical(top: Radius.circular(20))),
      builder: (ctx) => Padding(
        padding: EdgeInsets.fromLTRB(24, 24, 24, MediaQuery.of(ctx).viewInsets.bottom + 24),
        child: Column(mainAxisSize: MainAxisSize.min, children: [
          Container(width: 40, height: 4, decoration: BoxDecoration(color: context.dividerColor, borderRadius: BorderRadius.circular(2))),
          const SizedBox(height: 20),
          Text('Новая копилка', style: TextStyle(fontSize: 20, fontWeight: FontWeight.bold, color: context.textPrimary)),
          const SizedBox(height: 8),
          Text('Начните копить на свою мечту!', style: TextStyle(color: context.textSecondary)),
          const SizedBox(height: 20),
          TextField(controller: nameCtrl, decoration: const InputDecoration(labelText: 'Название', prefixIcon: Icon(Icons.savings_outlined))),
          const SizedBox(height: 20),
          SizedBox(width: double.infinity, child: ElevatedButton(
            onPressed: () async {
              Navigator.pop(ctx);
              try {
                await context.read<ApiService>().post('accounts/savings', body: {
                  'name': nameCtrl.text.trim(),
                  'currency': 'BYN',
                });
                _load();
                if (mounted) ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Копилка создана!')));
              } catch (e) {
                if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(e.toString())));
              }
            },
            child: const Text('Создать'),
          )),
        ]),
      ),
    );
  }

  Future<String?> _getMainAccountId() async {
    try {
      final all = await context.read<ApiService>().get('accounts/my') as List;
      final main = all.firstWhere((a) => a['type'] == 'Main', orElse: () => null);
      return main?['id'] as String?;
    } catch (_) {
      return null;
    }
  }

  void _deposit(dynamic account) {
    final ctrl = TextEditingController();
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      shape: const RoundedRectangleBorder(borderRadius: BorderRadius.vertical(top: Radius.circular(20))),
      builder: (ctx) => Padding(
        padding: EdgeInsets.fromLTRB(24, 24, 24, MediaQuery.of(ctx).viewInsets.bottom + 24),
        child: Column(mainAxisSize: MainAxisSize.min, children: [
          Text('Пополнить копилку', style: TextStyle(fontSize: 20, fontWeight: FontWeight.bold, color: context.textPrimary)),
          const SizedBox(height: 20),
          TextField(controller: ctrl, decoration: const InputDecoration(labelText: 'Сумма (BYN)'), keyboardType: TextInputType.number),
          const SizedBox(height: 20),
          SizedBox(width: double.infinity, child: ElevatedButton(
            onPressed: () async {
              Navigator.pop(ctx);
              try {
                final mainId = await _getMainAccountId();
                if (mainId == null) throw Exception('Основной счёт не найден');
                await context.read<ApiService>().post('accounts/transfer', body: {
                  'sourceAccountId': mainId,
                  'destinationAccountId': account['id'],
                  'amount': double.tryParse(ctrl.text) ?? 0,
                  'description': 'Пополнение копилки',
                });
                _load();
                if (mounted) ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Копилка пополнена!')));
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

  void _withdraw(dynamic account) {
    final ctrl = TextEditingController();
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      shape: const RoundedRectangleBorder(borderRadius: BorderRadius.vertical(top: Radius.circular(20))),
      builder: (ctx) => Padding(
        padding: EdgeInsets.fromLTRB(24, 24, 24, MediaQuery.of(ctx).viewInsets.bottom + 24),
        child: Column(mainAxisSize: MainAxisSize.min, children: [
          Text('Снять с копилки', style: TextStyle(fontSize: 20, fontWeight: FontWeight.bold, color: context.textPrimary)),
          const SizedBox(height: 8),
          Text('При досрочном снятии процент может быть потерян', style: TextStyle(color: context.textSecondary, fontSize: 12)),
          const SizedBox(height: 20),
          TextField(controller: ctrl, decoration: const InputDecoration(labelText: 'Сумма (BYN)'), keyboardType: TextInputType.number),
          const SizedBox(height: 20),
          SizedBox(width: double.infinity, child: ElevatedButton(
            onPressed: () async {
              Navigator.pop(ctx);
              try {
                final mainId = await _getMainAccountId();
                if (mainId == null) throw Exception('Основной счёт не найден');
                await context.read<ApiService>().post('accounts/transfer', body: {
                  'sourceAccountId': account['id'],
                  'destinationAccountId': mainId,
                  'amount': double.tryParse(ctrl.text) ?? 0,
                  'description': 'Вывод из копилки',
                });
                _load();
                if (mounted) ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Средства переведены!')));
              } catch (e) {
                if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(e.toString())));
              }
            },
            style: ElevatedButton.styleFrom(backgroundColor: AppColors.warning),
            child: const Text('Снять'),
          )),
        ]),
      ),
    );
  }
}
