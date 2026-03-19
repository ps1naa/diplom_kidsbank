import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/api_service.dart';
import '../../core/theme.dart';
import 'parent_categories_screen.dart';
import 'parent_kid_goals_screen.dart';
import 'parent_spending_limits_screen.dart';
import 'parent_tx_history_screen.dart';
import '../common/analytics_screen.dart';

class ParentKidsScreen extends StatefulWidget {
  const ParentKidsScreen({super.key});
  @override
  State<ParentKidsScreen> createState() => _ParentKidsScreenState();
}

class _ParentKidsScreenState extends State<ParentKidsScreen> {
  List<dynamic>? _kids;
  bool _loading = true;

  @override
  void initState() { super.initState(); _load(); }

  Future<void> _load() async {
    try {
      final kids = await context.read<ApiService>().get('families/kids');
      setState(() { _kids = kids as List; _loading = false; });
    } catch (_) { setState(() => _loading = false); }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Мои дети')),
      floatingActionButton: FloatingActionButton.extended(
        heroTag: 'parentKidsFab',
        onPressed: _showInviteDialog,
        icon: const Icon(Icons.person_add),
        label: const Text('Пригласить'),
      ),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : _kids == null || _kids!.isEmpty
              ? Center(child: Column(mainAxisSize: MainAxisSize.min, children: [
                  Container(
                    width: 80, height: 80,
                    decoration: BoxDecoration(color: AppColors.primary.withValues(alpha: 0.1), borderRadius: BorderRadius.circular(20)),
                    child: Icon(Icons.family_restroom, size: 40, color: context.textHint),
                  ),
                  const SizedBox(height: 16),
                  Text('Пока нет детей', style: TextStyle(fontSize: 18, color: context.textSecondary)),
                  const SizedBox(height: 8),
                  Text('Нажмите + чтобы пригласить ребёнка', style: TextStyle(color: context.textHint)),
                ]))
              : RefreshIndicator(
                  onRefresh: _load,
                  child: ListView.builder(
                    padding: const EdgeInsets.all(16),
                    itemCount: _kids!.length,
                    itemBuilder: (_, i) => _buildKidCard(_kids![i], i),
                  ),
                ),
    );
  }

  Widget _buildKidCard(dynamic kid, int index) {
    return TweenAnimationBuilder<double>(
      tween: Tween(begin: 0, end: 1),
      duration: Duration(milliseconds: 300 + index * 100),
      curve: Curves.easeOutCubic,
      builder: (_, val, child) => Opacity(opacity: val, child: Transform.translate(offset: Offset(0, 20 * (1 - val)), child: child)),
      child: Card(
        margin: const EdgeInsets.only(bottom: 12),
        child: InkWell(
          onTap: () => _showKidDetails(kid),
          borderRadius: BorderRadius.circular(16),
          child: Padding(
            padding: const EdgeInsets.all(16),
            child: Row(children: [
              CircleAvatar(
                radius: 28,
                backgroundColor: AppColors.kidGradient1.withValues(alpha: 0.15),
                child: Text('${kid['firstName'][0]}${kid['lastName'][0]}',
                    style: const TextStyle(color: AppColors.kidGradient1, fontWeight: FontWeight.bold, fontSize: 18)),
              ),
              const SizedBox(width: 16),
              Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                Text('${kid['firstName']} ${kid['lastName']}', style: TextStyle(fontSize: 16, fontWeight: FontWeight.w600, color: context.textPrimary)),
                const SizedBox(height: 4),
                Text('XP: ${kid['totalXp'] ?? 0} • Стрик: ${kid['currentStreak'] ?? 0} дн.', style: TextStyle(color: context.textSecondary, fontSize: 13)),
              ])),
              Icon(Icons.chevron_right, color: context.textHint),
            ]),
          ),
        ),
      ),
    );
  }

  void _showKidDetails(dynamic kid) {
    Navigator.push(context, MaterialPageRoute(builder: (_) => _KidDetailPage(kid: kid)));
  }

  void _showInviteDialog() {
    bool consent = false;
    int validDays = 7;
    showDialog(
      context: context,
      builder: (ctx) => StatefulBuilder(builder: (ctx, setDS) => AlertDialog(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
        title: const Text('Пригласить ребёнка'),
        content: Column(mainAxisSize: MainAxisSize.min, children: [
          Text('Будет создан код-приглашение для регистрации ребёнка в вашей семье.',
              style: TextStyle(fontSize: 13, color: context.textSecondary)),
          const SizedBox(height: 16),
          DropdownButtonFormField<int>(
            value: validDays,
            decoration: const InputDecoration(labelText: 'Срок действия', prefixIcon: Icon(Icons.timer_outlined)),
            items: const [
              DropdownMenuItem(value: 1, child: Text('1 день')),
              DropdownMenuItem(value: 3, child: Text('3 дня')),
              DropdownMenuItem(value: 7, child: Text('7 дней')),
              DropdownMenuItem(value: 14, child: Text('14 дней')),
              DropdownMenuItem(value: 30, child: Text('30 дней')),
            ],
            onChanged: (v) => setDS(() => validDays = v!),
          ),
          const SizedBox(height: 16),
          Row(crossAxisAlignment: CrossAxisAlignment.start, children: [
            Checkbox(value: consent, onChanged: (v) => setDS(() => consent = v!)),
            Expanded(child: Padding(
              padding: const EdgeInsets.only(top: 12),
              child: Text('Я даю согласие на создание банковского счёта для моего ребёнка в соответствии с законодательством РБ',
                  style: TextStyle(fontSize: 13, color: context.textSecondary)),
            )),
          ]),
        ]),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx), child: const Text('Отмена')),
          ElevatedButton(
            onPressed: consent ? () async {
              try {
                final res = await context.read<ApiService>().post('families/invite', body: {'validForDays': validDays});
                if (ctx.mounted) Navigator.pop(ctx);
                if (mounted) _showInviteResult(res['token'] ?? '');
              } catch (e) {
                if (ctx.mounted) ScaffoldMessenger.of(ctx).showSnackBar(SnackBar(content: Text(e.toString())));
              }
            } : null,
            child: const Text('Создать код'),
          ),
        ],
      )),
    );
  }

  void _showInviteResult(String token) {
    showDialog(context: context, builder: (_) => AlertDialog(
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
      title: Row(children: [
        const Icon(Icons.check_circle, color: AppColors.success),
        const SizedBox(width: 8),
        const Text('Код приглашения'),
      ]),
      content: Column(mainAxisSize: MainAxisSize.min, children: [
        const Text('Отправьте этот код ребёнку для регистрации:'),
        const SizedBox(height: 16),
        Container(
          padding: const EdgeInsets.all(16),
          width: double.infinity,
          decoration: BoxDecoration(color: context.surfaceVariant, borderRadius: BorderRadius.circular(12)),
          child: SelectableText(token, style: const TextStyle(fontSize: 20, fontWeight: FontWeight.bold, color: AppColors.primary), textAlign: TextAlign.center),
        ),
      ]),
      actions: [TextButton(onPressed: () => Navigator.pop(context), child: const Text('Закрыть'))],
    ));
  }
}

class _KidDetailPage extends StatefulWidget {
  final dynamic kid;
  const _KidDetailPage({required this.kid});
  @override
  State<_KidDetailPage> createState() => _KidDetailPageState();
}

class _KidDetailPageState extends State<_KidDetailPage> {
  List<dynamic>? _accounts;
  Map<String, dynamic>? _report;
  bool _loading = true;

  @override
  void initState() { super.initState(); _load(); }

  Future<void> _load() async {
    final api = context.read<ApiService>();
    final kidId = widget.kid['id'];
    try {
      final results = await Future.wait([
        api.get('accounts/kid/$kidId'),
        api.get('reports/kid/$kidId?period=monthly'),
      ]);
      setState(() { _accounts = results[0] as List; _report = results[1]; _loading = false; });
    } catch (_) { setState(() => _loading = false); }
  }

  @override
  Widget build(BuildContext context) {
    final kid = widget.kid;
    final kidId = kid['id'] as String;
    return Scaffold(
      appBar: AppBar(title: Text('${kid['firstName']} ${kid['lastName']}')),
      body: _loading ? const Center(child: CircularProgressIndicator()) : RefreshIndicator(
        onRefresh: _load,
        child: ListView(
          padding: const EdgeInsets.all(16),
          children: [
            Card(child: Padding(
              padding: const EdgeInsets.all(16),
              child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                Text('Счета', style: TextStyle(fontSize: 18, fontWeight: FontWeight.w600, color: context.textPrimary)),
                const SizedBox(height: 12),
                if (_accounts != null && _accounts!.isNotEmpty) ...(_accounts!.map((a) => ListTile(
                  leading: Icon(a['type'] == 'Main' ? Icons.account_balance_wallet : Icons.savings, color: AppColors.primary),
                  title: Text(a['name'] ?? a['type']),
                  trailing: Text('${(a['balance'] as num).toStringAsFixed(2)} BYN', style: const TextStyle(fontWeight: FontWeight.bold)),
                  onTap: () => Navigator.push(context, MaterialPageRoute(
                    builder: (_) => ParentTxHistoryScreen(kidId: kidId, kidName: kid['firstName'], accountId: a['id']),
                  )),
                ))) else Text('Нет счетов', style: TextStyle(color: context.textSecondary)),
              ]),
            )),
            const SizedBox(height: 12),
            if (_report != null) Card(child: Padding(
              padding: const EdgeInsets.all(16),
              child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                Text('Отчёт за месяц', style: TextStyle(fontSize: 18, fontWeight: FontWeight.w600, color: context.textPrimary)),
                const SizedBox(height: 12),
                _ReportRow('Задания выполнено', '${_report!['tasks']?['completed'] ?? 0}'),
                _ReportRow('Заработано', '${_report!['tasks']?['totalEarned'] ?? 0} BYN'),
                _ReportRow('Активных целей', '${_report!['goals']?['activeGoals'] ?? 0}'),
                _ReportRow('Модулей пройдено', '${_report!['education']?['modulesCompleted'] ?? 0}'),
              ]),
            )),
            const SizedBox(height: 16),
            Text('Действия', style: TextStyle(fontSize: 16, fontWeight: FontWeight.w600, color: context.textPrimary)),
            const SizedBox(height: 8),
            Wrap(spacing: 8, runSpacing: 8, children: [
              _ActionChip(icon: Icons.bar_chart, label: 'Аналитика', onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => AnalyticsScreen(kidId: kidId)))),
              _ActionChip(icon: Icons.flag, label: 'Цели', onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => ParentKidGoalsScreen(kidId: kidId, kidName: kid['firstName'])))),
              _ActionChip(icon: Icons.category, label: 'Категории', onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => ParentCategoriesScreen(kidId: kidId, kidName: kid['firstName'])))),
              _ActionChip(icon: Icons.block, label: 'Лимиты', onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => const ParentSpendingLimitsScreen()))),
              _ActionChip(icon: Icons.send, label: 'Пополнить', onTap: () => _depositToKid(kid)),
            ]),
          ],
        ),
      ),
    );
  }

  void _depositToKid(dynamic kid) {
    final ctrl = TextEditingController();
    showModalBottomSheet(
      context: context, isScrollControlled: true,
      shape: const RoundedRectangleBorder(borderRadius: BorderRadius.vertical(top: Radius.circular(20))),
      builder: (ctx) => Padding(
        padding: EdgeInsets.fromLTRB(24, 24, 24, MediaQuery.of(ctx).viewInsets.bottom + 24),
        child: Column(mainAxisSize: MainAxisSize.min, children: [
          Text('Пополнить счёт ${kid['firstName']}', style: TextStyle(fontSize: 20, fontWeight: FontWeight.bold, color: context.textPrimary)),
          const SizedBox(height: 16),
          TextField(controller: ctrl, decoration: const InputDecoration(labelText: 'Сумма (BYN)'), keyboardType: TextInputType.number),
          const SizedBox(height: 16),
          SizedBox(width: double.infinity, child: ElevatedButton(
            onPressed: () async {
              Navigator.pop(ctx);
              try {
                await context.read<ApiService>().post('accounts/deposit', body: {'kidId': kid['id'], 'amount': double.tryParse(ctrl.text) ?? 0, 'currency': 'BYN'});
                _load();
                if (mounted) ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Пополнено!')));
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
}

class _ActionChip extends StatelessWidget {
  final IconData icon;
  final String label;
  final VoidCallback onTap;
  const _ActionChip({required this.icon, required this.label, required this.onTap});
  @override
  Widget build(BuildContext context) {
    return ActionChip(
      avatar: Icon(icon, size: 18, color: AppColors.primary),
      label: Text(label),
      onPressed: onTap,
    );
  }
}

class _ReportRow extends StatelessWidget {
  final String label, value;
  const _ReportRow(this.label, this.value);
  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 4),
      child: Row(mainAxisAlignment: MainAxisAlignment.spaceBetween, children: [
        Text(label, style: TextStyle(color: context.textSecondary)),
        Text(value, style: const TextStyle(fontWeight: FontWeight.w600)),
      ]),
    );
  }
}
