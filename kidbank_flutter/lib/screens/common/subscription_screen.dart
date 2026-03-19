import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/api_service.dart';
import '../../core/theme.dart';

class SubscriptionScreen extends StatefulWidget {
  const SubscriptionScreen({super.key});
  @override
  State<SubscriptionScreen> createState() => _SubscriptionScreenState();
}

class _SubscriptionScreenState extends State<SubscriptionScreen> with SingleTickerProviderStateMixin {
  late AnimationController _animCtrl;
  int _selectedPlan = 1;
  Map<String, dynamic>? _status;
  bool _loading = true;
  bool _subscribing = false;

  @override
  void initState() {
    super.initState();
    _animCtrl = AnimationController(vsync: this, duration: const Duration(milliseconds: 800));
    _animCtrl.forward();
    _load();
  }

  Future<void> _load() async {
    try {
      final data = await context.read<ApiService>().get('subscriptions');
      setState(() { _status = data; _loading = false; });
    } catch (_) { setState(() => _loading = false); }
  }

  bool get _isPro => _status?['isPro'] == true;
  Map<String, dynamic>? get _activeSub => _status?['activeSubscription'];
  Map<String, dynamic>? get _limits => _status?['limits'];

  @override
  void dispose() { _animCtrl.dispose(); super.dispose(); }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Подписка KidBank Pro')),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : SingleChildScrollView(
              padding: const EdgeInsets.all(20),
              child: Column(children: [
                FadeTransition(
                  opacity: _animCtrl,
                  child: SlideTransition(
                    position: Tween(begin: const Offset(0, 0.3), end: Offset.zero).animate(CurvedAnimation(parent: _animCtrl, curve: Curves.easeOutCubic)),
                    child: _buildHeader(),
                  ),
                ),
                const SizedBox(height: 24),
                if (_isPro) _buildActiveSubscription() else ...[
                  _buildPlans(),
                  const SizedBox(height: 24),
                ],
                _buildFeatures(),
                const SizedBox(height: 24),
                _buildComparisonTable(),
                const SizedBox(height: 32),
                if (!_isPro) SizedBox(width: double.infinity, child: ElevatedButton(
                  onPressed: _subscribing ? null : _subscribe,
                  style: ElevatedButton.styleFrom(padding: const EdgeInsets.symmetric(vertical: 16)),
                  child: _subscribing
                      ? const SizedBox(width: 20, height: 20, child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white))
                      : const Text('Оформить подписку', style: TextStyle(fontSize: 16)),
                )),
                if (_isPro && _activeSub?['autoRenew'] == true) SizedBox(width: double.infinity, child: OutlinedButton(
                  onPressed: _cancelSubscription,
                  style: OutlinedButton.styleFrom(foregroundColor: AppColors.error, side: const BorderSide(color: AppColors.error)),
                  child: const Text('Отменить автопродление'),
                )),
                const SizedBox(height: 16),
                Text(
                  _isPro ? 'Подписка активна для всей семьи' : 'Подписку можно отменить в любое время',
                  style: TextStyle(color: context.textSecondary, fontSize: 12),
                ),
              ]),
            ),
    );
  }

  Widget _buildHeader() {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(24),
      decoration: BoxDecoration(
        gradient: LinearGradient(colors: _isPro
            ? [AppColors.success, AppColors.primary]
            : [AppColors.cardGradient1, AppColors.cardGradient2]),
        borderRadius: BorderRadius.circular(20),
      ),
      child: Column(children: [
        Container(
          width: 64, height: 64,
          decoration: BoxDecoration(color: Colors.white24, borderRadius: BorderRadius.circular(16)),
          child: Icon(_isPro ? Icons.verified : Icons.workspace_premium, color: Colors.white, size: 36),
        ),
        const SizedBox(height: 16),
        Text(_isPro ? 'KidBank Pro' : 'KidBank Pro', style: const TextStyle(color: Colors.white, fontSize: 24, fontWeight: FontWeight.bold)),
        const SizedBox(height: 8),
        Text(
          _isPro ? 'Все возможности разблокированы!' : 'Получите доступ ко всем возможностям',
          style: const TextStyle(color: Colors.white70, fontSize: 14),
          textAlign: TextAlign.center,
        ),
      ]),
    );
  }

  Widget _buildActiveSubscription() {
    final endDate = _activeSub?['endDate'] != null
        ? DateTime.tryParse(_activeSub!['endDate'].toString())
        : null;
    final daysLeft = endDate != null ? endDate.difference(DateTime.now()).inDays : 0;

    return Card(
      margin: const EdgeInsets.only(bottom: 24),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
          Row(children: [
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
              decoration: BoxDecoration(color: AppColors.success.withValues(alpha: 0.15), borderRadius: BorderRadius.circular(12)),
              child: const Text('Активна', style: TextStyle(color: AppColors.success, fontWeight: FontWeight.w600, fontSize: 13)),
            ),
            const Spacer(),
            Text('${_activeSub?['plan'] == 'Yearly' ? 'Годовая' : 'Месячная'}', style: TextStyle(color: context.textSecondary, fontSize: 13)),
          ]),
          const SizedBox(height: 12),
          Text('$daysLeft дн. осталось', style: TextStyle(fontSize: 28, fontWeight: FontWeight.bold, color: context.textPrimary)),
          const SizedBox(height: 4),
          if (endDate != null) Text(
            'Действует до ${endDate.day.toString().padLeft(2, '0')}.${endDate.month.toString().padLeft(2, '0')}.${endDate.year}',
            style: TextStyle(color: context.textSecondary, fontSize: 13),
          ),
          const SizedBox(height: 8),
          if (_activeSub?['autoRenew'] == true)
            Row(children: [
              Icon(Icons.autorenew, size: 16, color: AppColors.success),
              const SizedBox(width: 4),
              Text('Автопродление включено', style: TextStyle(color: AppColors.success, fontSize: 12)),
            ])
          else
            Row(children: [
              Icon(Icons.cancel_outlined, size: 16, color: AppColors.warning),
              const SizedBox(width: 4),
              Text('Автопродление отключено', style: TextStyle(color: AppColors.warning, fontSize: 12)),
            ]),
        ]),
      ),
    );
  }

  Widget _buildPlans() {
    return Row(children: [
      Expanded(child: _PlanCard(
        title: 'Месяц', price: '4.99', period: 'мес',
        isSelected: _selectedPlan == 0,
        onTap: () => setState(() => _selectedPlan = 0),
      )),
      const SizedBox(width: 12),
      Expanded(child: Stack(children: [
        _PlanCard(
          title: 'Год', price: '39.99', period: 'год',
          isSelected: _selectedPlan == 1,
          onTap: () => setState(() => _selectedPlan = 1),
        ),
        Positioned(
          top: 0, right: 8,
          child: Container(
            padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
            decoration: BoxDecoration(color: AppColors.accent, borderRadius: BorderRadius.circular(8)),
            child: const Text('–33%', style: TextStyle(color: Colors.black, fontSize: 11, fontWeight: FontWeight.bold)),
          ),
        ),
      ])),
    ]);
  }

  Widget _buildFeatures() {
    final features = [
      _Feature(Icons.family_restroom, 'До 10 детей в семье', 'Вместо 2 на бесплатном плане'),
      _Feature(Icons.credit_card, 'До 5 карт на ребёнка', 'Вместо 1 виртуальной карты'),
      _Feature(Icons.flag, 'До 20 целей', 'Вместо 3 на бесплатном плане'),
      _Feature(Icons.savings, 'До 5 копилок', 'Вместо 1 сберегательного счёта'),
      _Feature(Icons.block, 'Лимиты расходов', 'По дням, неделям и месяцам'),
      _Feature(Icons.category, 'Блокировка категорий', 'Ограничьте траты ребёнка'),
      _Feature(Icons.school, 'Все образовательные модули', 'Доступ ко всем миссиям и квизам'),
      _Feature(Icons.bar_chart, 'Расширенная аналитика', 'Подробная статистика за все время'),
      _Feature(Icons.description, 'До 50 шаблонов заданий', 'Вместо 5 на бесплатном плане'),
    ];
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text('Возможности Pro', style: TextStyle(fontSize: 18, fontWeight: FontWeight.w600, color: context.textPrimary)),
        const SizedBox(height: 16),
        ...features.asMap().entries.map((e) => TweenAnimationBuilder<double>(
          tween: Tween(begin: 0, end: 1),
          duration: Duration(milliseconds: 400 + e.key * 80),
          curve: Curves.easeOutCubic,
          builder: (_, val, child) => Opacity(opacity: val, child: Transform.translate(offset: Offset(20 * (1 - val), 0), child: child)),
          child: Padding(
            padding: const EdgeInsets.only(bottom: 12),
            child: Row(children: [
              Container(
                width: 44, height: 44,
                decoration: BoxDecoration(color: AppColors.primary.withValues(alpha: 0.15), borderRadius: BorderRadius.circular(12)),
                child: Icon(e.value.icon, color: AppColors.primary, size: 22),
              ),
              const SizedBox(width: 12),
              Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                Text(e.value.title, style: const TextStyle(fontWeight: FontWeight.w600)),
                Text(e.value.subtitle, style: TextStyle(fontSize: 12, color: context.textSecondary)),
              ])),
              if (_isPro) const Icon(Icons.check_circle, color: AppColors.success, size: 20),
            ]),
          ),
        )),
      ],
    );
  }

  Widget _buildComparisonTable() {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
          Text('Сравнение планов', style: TextStyle(fontSize: 16, fontWeight: FontWeight.w600, color: context.textPrimary)),
          const Divider(),
          _CompRow('Детей в семье', '2', '10'),
          _CompRow('Виртуальные карты', '1', 'До 5'),
          _CompRow('Цели накопления', '3', 'До 20'),
          _CompRow('Копилки', '1', 'До 5'),
          _CompRow('Лимиты расходов', '—', 'Да'),
          _CompRow('Блокировка категорий', '—', 'Да'),
          _CompRow('Образование', 'Базовое', 'Все модули'),
          _CompRow('Аналитика', 'Базовая', 'Расширенная'),
          _CompRow('Шаблоны заданий', '5', 'До 50'),
        ]),
      ),
    );
  }

  Future<void> _subscribe() async {
    setState(() => _subscribing = true);
    try {
      final plan = _selectedPlan == 0 ? 'Monthly' : 'Yearly';
      await context.read<ApiService>().post('subscriptions', body: {'plan': plan});
      await _load();
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(const SnackBar(
          content: Text('Подписка Pro оформлена!'),
          backgroundColor: AppColors.success,
        ));
      }
    } catch (e) {
      if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(e.toString())));
    }
    setState(() => _subscribing = false);
  }

  Future<void> _cancelSubscription() async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
        title: const Text('Отменить автопродление?'),
        content: const Text('Подписка будет активна до конца оплаченного периода, но не продлится автоматически.'),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx, false), child: const Text('Нет')),
          ElevatedButton(
            onPressed: () => Navigator.pop(ctx, true),
            style: ElevatedButton.styleFrom(backgroundColor: AppColors.error),
            child: const Text('Отменить'),
          ),
        ],
      ),
    );
    if (confirmed != true) return;
    try {
      await context.read<ApiService>().post('subscriptions/cancel');
      await _load();
      if (mounted) ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Автопродление отменено')));
    } catch (e) {
      if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(e.toString())));
    }
  }
}

class _PlanCard extends StatelessWidget {
  final String title, price, period;
  final bool isSelected;
  final VoidCallback onTap;
  const _PlanCard({required this.title, required this.price, required this.period, required this.isSelected, required this.onTap});
  @override
  Widget build(BuildContext context) {
    return AnimatedContainer(
      duration: const Duration(milliseconds: 200),
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(16),
        child: Container(
          padding: const EdgeInsets.all(16),
          decoration: BoxDecoration(
            border: Border.all(color: isSelected ? AppColors.primary : context.dividerColor, width: isSelected ? 2 : 1),
            borderRadius: BorderRadius.circular(16),
            color: isSelected ? AppColors.primary.withValues(alpha: 0.08) : Colors.transparent,
          ),
          child: Column(children: [
            Text(title, style: TextStyle(fontWeight: FontWeight.w500, color: context.textSecondary)),
            const SizedBox(height: 4),
            RichText(text: TextSpan(children: [
              TextSpan(text: price, style: TextStyle(fontSize: 24, fontWeight: FontWeight.bold, color: context.textPrimary)),
              TextSpan(text: ' BYN', style: TextStyle(fontSize: 14, color: context.textSecondary)),
            ])),
            Text('/ $period', style: TextStyle(fontSize: 12, color: context.textSecondary)),
          ]),
        ),
      ),
    );
  }
}

class _Feature {
  final IconData icon;
  final String title, subtitle;
  const _Feature(this.icon, this.title, this.subtitle);
}

class _CompRow extends StatelessWidget {
  final String feature, free, pro;
  const _CompRow(this.feature, this.free, this.pro);
  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 6),
      child: Row(children: [
        Expanded(flex: 3, child: Text(feature, style: TextStyle(fontSize: 13, color: context.textSecondary))),
        Expanded(flex: 2, child: Text(free, style: TextStyle(fontSize: 13, color: context.textHint), textAlign: TextAlign.center)),
        Expanded(flex: 2, child: Text(pro, style: const TextStyle(fontSize: 13, color: AppColors.primary, fontWeight: FontWeight.w600), textAlign: TextAlign.center)),
      ]),
    );
  }
}
