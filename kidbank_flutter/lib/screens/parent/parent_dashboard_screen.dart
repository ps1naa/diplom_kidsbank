import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:fl_chart/fl_chart.dart';
import '../../core/api_service.dart';
import '../../core/theme.dart';
import '../../providers/auth_provider.dart';
import '../../widgets/gradient_card.dart';
import '../common/chat_screen.dart';
import '../common/notifications_screen.dart';
import '../common/subscription_screen.dart';
import 'parent_money_requests_screen.dart';
import 'parent_allowance_screen.dart';

class ParentDashboardScreen extends StatefulWidget {
  const ParentDashboardScreen({super.key});
  @override
  State<ParentDashboardScreen> createState() => _ParentDashboardScreenState();
}

class _ParentDashboardScreenState extends State<ParentDashboardScreen> with SingleTickerProviderStateMixin {
  Map<String, dynamic>? _dashboard;
  List<dynamic>? _accounts;
  List<dynamic>? _pendingRequests;
  Map<String, dynamic>? _familyOverview;
  bool _loading = true;
  late AnimationController _anim;

  @override
  void initState() {
    super.initState();
    _anim = AnimationController(vsync: this, duration: const Duration(milliseconds: 600));
    _load();
  }

  Future<void> _load() async {
    final api = context.read<ApiService>();
    try {
      final results = await Future.wait([
        api.get('families/dashboard'),
        api.get('accounts/my'),
        api.get('moneyrequests/pending'),
        api.get('analytics/family/overview').catchError((_) => <String, dynamic>{}),
      ]);
      setState(() {
        _dashboard = results[0];
        _accounts = results[1] as List;
        _pendingRequests = results[2] as List;
        _familyOverview = results[3] is Map ? results[3] as Map<String, dynamic> : null;
        _loading = false;
      });
      _anim.forward();
    } catch (e) { setState(() => _loading = false); }
  }

  @override
  void dispose() { _anim.dispose(); super.dispose(); }

  @override
  Widget build(BuildContext context) {
    final auth = context.watch<AuthProvider>();
    return Scaffold(
      body: SafeArea(
        child: _loading
            ? const Center(child: CircularProgressIndicator())
            : RefreshIndicator(
                onRefresh: _load,
                child: ListView(
                  padding: const EdgeInsets.all(20),
                  children: [
                    FadeTransition(
                      opacity: _anim,
                      child: Row(children: [
                        Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                          Text('Привет, ${auth.firstName}!', style: TextStyle(fontSize: 26, fontWeight: FontWeight.bold, color: context.textPrimary)),
                          const SizedBox(height: 4),
                          Text(_dashboard?['familyName'] ?? 'Семья', style: TextStyle(color: context.textSecondary, fontSize: 14)),
                        ])),
                        GestureDetector(
                          onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => const NotificationsScreen())),
                          child: Container(
                            width: 44, height: 44,
                            decoration: BoxDecoration(color: context.surfaceVariant, borderRadius: BorderRadius.circular(14)),
                            child: Icon(Icons.notifications_none, color: context.textSecondary),
                          ),
                        ),
                      ]),
                    ),
                    const SizedBox(height: 20),
                    GradientCard(
                      child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                        Row(children: [
                          const Icon(Icons.account_balance_wallet, color: Colors.white, size: 20),
                          const SizedBox(width: 8),
                          const Text('Общий баланс', style: TextStyle(color: Colors.white70, fontSize: 14)),
                          const Spacer(),
                          Container(
                            padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
                            decoration: BoxDecoration(color: Colors.white24, borderRadius: BorderRadius.circular(8)),
                            child: Text('${_accounts?.length ?? 0} счетов', style: const TextStyle(color: Colors.white70, fontSize: 11)),
                          ),
                        ]),
                        const SizedBox(height: 12),
                        TweenAnimationBuilder<double>(
                          tween: Tween(begin: 0, end: double.tryParse(_getMainBalance()) ?? 0),
                          duration: const Duration(milliseconds: 1200),
                          curve: Curves.easeOutCubic,
                          builder: (_, val, __) => Text(
                            '${val.toStringAsFixed(2)} BYN',
                            style: const TextStyle(color: Colors.white, fontSize: 32, fontWeight: FontWeight.bold),
                          ),
                        ),
                      ]),
                    ),
                    const SizedBox(height: 20),
                    _buildQuickActions(),
                    const SizedBox(height: 24),
                    if (_familyOverview != null) _buildSpendingChart(),
                    if (_pendingRequests != null && _pendingRequests!.isNotEmpty) ...[
                      const SizedBox(height: 24),
                      Row(mainAxisAlignment: MainAxisAlignment.spaceBetween, children: [
                        Text('Запросы', style: TextStyle(fontSize: 18, fontWeight: FontWeight.w600, color: context.textPrimary)),
                        TextButton(
                          onPressed: () => Navigator.push(context, MaterialPageRoute(builder: (_) => const ParentMoneyRequestsScreen())),
                          child: const Text('Все'),
                        ),
                      ]),
                      const SizedBox(height: 8),
                      ..._pendingRequests!.take(3).map((r) => _buildRequestCard(r)),
                    ],
                  ],
                ),
              ),
      ),
    );
  }

  String _getMainBalance() {
    if (_accounts == null) return '0.00';
    final main = _accounts!.where((a) => a['type'] == 'Main').toList();
    if (main.isEmpty) return '0.00';
    return (main.first['balance'] as num).toStringAsFixed(2);
  }

  Widget _buildQuickActions() {
    return GridView.count(
      crossAxisCount: 4,
      shrinkWrap: true,
      physics: const NeverScrollableScrollPhysics(),
      mainAxisSpacing: 12,
      crossAxisSpacing: 12,
      children: [
        _QuickAction(icon: Icons.send, label: 'Пополнить', color: AppColors.primary, onTap: _showTopUp),
        _QuickAction(icon: Icons.chat_bubble_outline, label: 'Чат', color: AppColors.success, onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => const ChatScreen()))),
        _QuickAction(icon: Icons.schedule, label: 'Авто', color: AppColors.warning, onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => const ParentAllowanceScreen()))),
        _QuickAction(icon: Icons.workspace_premium, label: 'Pro', color: AppColors.accent, onTap: () => Navigator.push(context, MaterialPageRoute(builder: (_) => const SubscriptionScreen()))),
      ],
    );
  }

  Widget _buildSpendingChart() {
    final kids = _familyOverview?['kidsSummary'];
    if (kids == null || (kids is List && kids.isEmpty)) {
      return const SizedBox.shrink();
    }
    final items = (kids is List) ? kids : <dynamic>[];
    if (items.isEmpty) return const SizedBox.shrink();

    final colors = [AppColors.primary, AppColors.accent, AppColors.error, AppColors.success, AppColors.warning, AppColors.kidGradient2];

    return Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
      Text('Обзор детей', style: TextStyle(fontSize: 18, fontWeight: FontWeight.w600, color: context.textPrimary)),
      const SizedBox(height: 16),
      SizedBox(
        height: 200,
        child: Row(children: [
          Expanded(child: PieChart(PieChartData(
            sectionsSpace: 2,
            centerSpaceRadius: 40,
            sections: items.asMap().entries.map((e) {
              final val = (e.value['balance'] as num?)?.toDouble() ?? 0;
              return PieChartSectionData(
                value: val > 0 ? val : 0.1,
                color: colors[e.key % colors.length],
                radius: 35,
                titleStyle: const TextStyle(color: Colors.white, fontSize: 11, fontWeight: FontWeight.bold),
                title: '${val.toStringAsFixed(0)}',
              );
            }).toList(),
          ))),
          const SizedBox(width: 16),
          Column(
            mainAxisAlignment: MainAxisAlignment.center,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: items.asMap().entries.map<Widget>((e) => Padding(
              padding: const EdgeInsets.symmetric(vertical: 3),
              child: Row(children: [
                Container(width: 12, height: 12, decoration: BoxDecoration(color: colors[e.key % colors.length], borderRadius: BorderRadius.circular(3))),
                const SizedBox(width: 8),
                Text(e.value['kidName'] ?? '', style: TextStyle(fontSize: 12, color: context.textSecondary)),
              ]),
            )).toList(),
          ),
        ]),
      ),
      const SizedBox(height: 16),
      ...items.map<Widget>((kid) => Card(
        margin: const EdgeInsets.only(bottom: 8),
        child: Padding(
          padding: const EdgeInsets.all(12),
          child: Row(children: [
            CircleAvatar(radius: 18, backgroundColor: AppColors.kidGradient1.withValues(alpha: 0.15),
              child: Text('${(kid['kidName'] as String? ?? '')[0]}', style: const TextStyle(color: AppColors.kidGradient1, fontWeight: FontWeight.bold, fontSize: 14))),
            const SizedBox(width: 12),
            Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
              Text(kid['kidName'] ?? '', style: const TextStyle(fontWeight: FontWeight.w600)),
              Text('Ур. ${kid['level'] ?? 1} • XP: ${kid['totalXp'] ?? 0} • Стрик: ${kid['currentStreak'] ?? 0}',
                  style: TextStyle(fontSize: 12, color: context.textSecondary)),
            ])),
            Text('${((kid['balance'] as num?) ?? 0).toStringAsFixed(2)} BYN', style: const TextStyle(fontWeight: FontWeight.bold)),
          ]),
        ),
      )),
    ]);
  }

  void _showTopUp() {
    final ctrl = TextEditingController();
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      shape: const RoundedRectangleBorder(borderRadius: BorderRadius.vertical(top: Radius.circular(20))),
      builder: (ctx) => Padding(
        padding: EdgeInsets.fromLTRB(24, 24, 24, MediaQuery.of(ctx).viewInsets.bottom + 24),
        child: Column(mainAxisSize: MainAxisSize.min, children: [
          Container(width: 40, height: 4, decoration: BoxDecoration(color: context.dividerColor, borderRadius: BorderRadius.circular(2))),
          const SizedBox(height: 20),
          Text('Пополнить счёт', style: TextStyle(fontSize: 20, fontWeight: FontWeight.bold, color: context.textPrimary)),
          const SizedBox(height: 16),
          TextField(controller: ctrl, decoration: const InputDecoration(labelText: 'Сумма (BYN)', prefixIcon: Icon(Icons.attach_money)), keyboardType: TextInputType.number),
          const SizedBox(height: 16),
          SizedBox(width: double.infinity, child: ElevatedButton(
            onPressed: () async {
              Navigator.pop(ctx);
              await context.read<ApiService>().post('accounts/topup', body: {'amount': double.tryParse(ctrl.text) ?? 0});
              _load();
            },
            child: const Text('Пополнить'),
          )),
        ]),
      ),
    );
  }

  Widget _buildRequestCard(dynamic req) {
    return Card(
      margin: const EdgeInsets.only(bottom: 8),
      child: ListTile(
        leading: CircleAvatar(
          backgroundColor: AppColors.accent.withValues(alpha: 0.15),
          child: const Icon(Icons.request_page, color: AppColors.accent),
        ),
        title: Text('${req['kidName']} — ${req['amount']} BYN'),
        subtitle: Text(req['reason'] ?? '', maxLines: 1, overflow: TextOverflow.ellipsis, style: TextStyle(color: context.textSecondary)),
        trailing: Row(mainAxisSize: MainAxisSize.min, children: [
          IconButton(icon: const Icon(Icons.check_circle, color: AppColors.success), onPressed: () async {
            await context.read<ApiService>().post('moneyrequests/${req['id']}/approve', body: {'note': ''});
            _load();
          }),
          IconButton(icon: const Icon(Icons.cancel, color: AppColors.error), onPressed: () async {
            await context.read<ApiService>().post('moneyrequests/${req['id']}/reject', body: {'note': ''});
            _load();
          }),
        ]),
      ),
    );
  }
}

class _QuickAction extends StatelessWidget {
  final IconData icon;
  final String label;
  final Color color;
  final VoidCallback onTap;
  const _QuickAction({required this.icon, required this.label, required this.color, required this.onTap});
  @override
  Widget build(BuildContext context) {
    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(16),
      child: Container(
        decoration: BoxDecoration(color: color.withValues(alpha: 0.12), borderRadius: BorderRadius.circular(16)),
        child: Column(mainAxisAlignment: MainAxisAlignment.center, children: [
          Icon(icon, color: color, size: 24),
          const SizedBox(height: 6),
          Text(label, style: TextStyle(fontSize: 11, color: color, fontWeight: FontWeight.w500), textAlign: TextAlign.center),
        ]),
      ),
    );
  }
}
