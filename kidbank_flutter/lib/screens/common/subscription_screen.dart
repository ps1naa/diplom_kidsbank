import 'package:flutter/material.dart';
import '../../core/theme.dart';

class SubscriptionScreen extends StatefulWidget {
  const SubscriptionScreen({super.key});
  @override
  State<SubscriptionScreen> createState() => _SubscriptionScreenState();
}

class _SubscriptionScreenState extends State<SubscriptionScreen> with SingleTickerProviderStateMixin {
  late AnimationController _animCtrl;
  int _selectedPlan = 1;

  @override
  void initState() {
    super.initState();
    _animCtrl = AnimationController(vsync: this, duration: const Duration(milliseconds: 800));
    _animCtrl.forward();
  }

  @override
  void dispose() { _animCtrl.dispose(); super.dispose(); }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Подписка KidBank Pro')),
      body: SingleChildScrollView(
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
          _buildPlans(),
          const SizedBox(height: 24),
          _buildFeatures(),
          const SizedBox(height: 24),
          _buildComparisonTable(),
          const SizedBox(height: 32),
          SizedBox(width: double.infinity, child: ElevatedButton(
            onPressed: () {
              ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Функция подписки скоро будет доступна!')));
            },
            style: ElevatedButton.styleFrom(padding: const EdgeInsets.symmetric(vertical: 16)),
            child: const Text('Оформить подписку', style: TextStyle(fontSize: 16)),
          )),
          const SizedBox(height: 16),
          Text('Подписку можно отменить в любое время', style: TextStyle(color: context.textSecondary, fontSize: 12)),
        ]),
      ),
    );
  }

  Widget _buildHeader() {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(24),
      decoration: BoxDecoration(
        gradient: const LinearGradient(colors: [AppColors.cardGradient1, AppColors.cardGradient2]),
        borderRadius: BorderRadius.circular(20),
      ),
      child: Column(children: [
        Container(
          width: 64, height: 64,
          decoration: BoxDecoration(color: Colors.white24, borderRadius: BorderRadius.circular(16)),
          child: const Icon(Icons.workspace_premium, color: Colors.white, size: 36),
        ),
        const SizedBox(height: 16),
        const Text('KidBank Pro', style: TextStyle(color: Colors.white, fontSize: 24, fontWeight: FontWeight.bold)),
        const SizedBox(height: 8),
        const Text('Получите доступ ко всем возможностям', style: TextStyle(color: Colors.white70, fontSize: 14), textAlign: TextAlign.center),
      ]),
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
      _Feature(Icons.credit_card, 'Неограниченные карты', 'Создавайте сколько угодно виртуальных карт'),
      _Feature(Icons.school, 'Все образовательные квизы', 'Доступ ко всем модулям и урокам'),
      _Feature(Icons.trending_up, 'Повышенный процент', 'Выше процент на сберегательном счёте'),
      _Feature(Icons.savings, 'Досрочный вывод', 'Возможность досрочно снять деньги с копилки'),
      _Feature(Icons.bar_chart, 'Расширенная аналитика', 'Подробная аналитика расходов и доходов'),
      _Feature(Icons.support_agent, 'Приоритетная поддержка', 'Ответ в течение 1 часа'),
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
          const SizedBox(height: 12),
          _CompRow('Виртуальные карты', '1', 'Безлимит'),
          _CompRow('Образовательные квизы', 'Пробные', 'Все'),
          _CompRow('Процент по копилке', '2%', '5%'),
          _CompRow('Досрочный вывод', '—', 'Да'),
          _CompRow('Аналитика', 'Базовая', 'Расширенная'),
          _CompRow('Поддержка', 'Стандарт', 'Приоритет'),
        ]),
      ),
    );
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
