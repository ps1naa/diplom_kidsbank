import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:provider/provider.dart';
import '../../core/api_service.dart';
import '../../core/theme.dart';

class ParentCardsScreen extends StatefulWidget {
  const ParentCardsScreen({super.key});
  @override
  State<ParentCardsScreen> createState() => _ParentCardsScreenState();
}

class _ParentCardsScreenState extends State<ParentCardsScreen> with TickerProviderStateMixin {
  List<dynamic> _cards = [];
  bool _loading = true;
  late AnimationController _fabAnim;

  @override
  void initState() {
    super.initState();
    _fabAnim = AnimationController(vsync: this, duration: const Duration(milliseconds: 300));
    _load();
  }

  Future<void> _load() async {
    try {
      final cards = await context.read<ApiService>().get('cards/my');
      setState(() { _cards = cards as List; _loading = false; });
      _fabAnim.forward();
    } catch (_) { setState(() => _loading = false); }
  }

  @override
  void dispose() { _fabAnim.dispose(); super.dispose(); }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Мои карты')),
      floatingActionButton: ScaleTransition(
        scale: _fabAnim,
        child: FloatingActionButton.extended(
          heroTag: 'parentCardsFab',
          onPressed: _createCard,
          icon: const Icon(Icons.add_card),
          label: const Text('Новая карта'),
        ),
      ),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : _cards.isEmpty
              ? _buildEmpty()
              : RefreshIndicator(
                  onRefresh: _load,
                  child: ListView.builder(
                    padding: const EdgeInsets.all(16),
                    itemCount: _cards.length,
                    itemBuilder: (_, i) => _buildCard(_cards[i], i),
                  ),
                ),
    );
  }

  Widget _buildEmpty() {
    return Center(child: Column(mainAxisSize: MainAxisSize.min, children: [
      Container(
        width: 80, height: 80,
        decoration: BoxDecoration(
          color: AppColors.primary.withValues(alpha: 0.1),
          borderRadius: BorderRadius.circular(20),
        ),
        child: const Icon(Icons.credit_card_off, size: 40, color: AppColors.primary),
      ),
      const SizedBox(height: 16),
      Text('Нет карт', style: TextStyle(fontSize: 18, fontWeight: FontWeight.w600, color: context.textPrimary)),
      const SizedBox(height: 8),
      Text('Создайте виртуальную карту', style: TextStyle(color: context.textSecondary)),
    ]));
  }

  Widget _buildCard(dynamic card, int index) {
    final isFrozen = card['isFrozen'] == true;
    final cn = card['cardNumber'] as String? ?? '';
    final last4 = cn.length >= 4 ? cn.substring(cn.length - 4) : '****';
    final name = card['cardHolderName'] as String? ?? '';
    return TweenAnimationBuilder<double>(
      tween: Tween(begin: 0, end: 1),
      duration: Duration(milliseconds: 400 + index * 100),
      curve: Curves.easeOutCubic,
      builder: (_, val, child) => Opacity(
        opacity: val,
        child: Transform.translate(offset: Offset(0, 30 * (1 - val)), child: child),
      ),
      child: Container(
        margin: const EdgeInsets.only(bottom: 16),
        decoration: BoxDecoration(
          gradient: LinearGradient(
            colors: isFrozen
                ? [Colors.blueGrey.shade700, Colors.blueGrey.shade900]
                : [AppColors.cardGradient1, AppColors.cardGradient2],
            begin: Alignment.topLeft, end: Alignment.bottomRight,
          ),
          borderRadius: BorderRadius.circular(20),
          boxShadow: [BoxShadow(color: (isFrozen ? Colors.blueGrey : AppColors.primary).withValues(alpha: 0.3), blurRadius: 12, offset: const Offset(0, 6))],
        ),
        child: Material(
          color: Colors.transparent,
          child: InkWell(
            onTap: () => _showCardActions(card),
            borderRadius: BorderRadius.circular(20),
            child: Padding(
              padding: const EdgeInsets.all(24),
              child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                Row(mainAxisAlignment: MainAxisAlignment.spaceBetween, children: [
                  const Text('KidBank', style: TextStyle(color: Colors.white, fontSize: 18, fontWeight: FontWeight.bold)),
                  if (isFrozen) Container(
                    padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                    decoration: BoxDecoration(color: Colors.white24, borderRadius: BorderRadius.circular(8)),
                    child: const Row(mainAxisSize: MainAxisSize.min, children: [
                      Icon(Icons.ac_unit, color: Colors.white70, size: 14),
                      SizedBox(width: 4),
                      Text('Заморожена', style: TextStyle(color: Colors.white70, fontSize: 12)),
                    ]),
                  ),
                ]),
                const SizedBox(height: 32),
                Text('•••• •••• •••• $last4', style: const TextStyle(color: Colors.white, fontSize: 22, fontWeight: FontWeight.w500, letterSpacing: 2)),
                const SizedBox(height: 20),
                Row(mainAxisAlignment: MainAxisAlignment.spaceBetween, children: [
                  Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                    const Text('ВЛАДЕЛЕЦ', style: TextStyle(color: Colors.white60, fontSize: 10)),
                    const SizedBox(height: 2),
                    Text(name.toUpperCase(), style: const TextStyle(color: Colors.white, fontSize: 14, fontWeight: FontWeight.w500)),
                  ]),
                  const Icon(Icons.contactless, color: Colors.white70, size: 32),
                ]),
              ]),
            ),
          ),
        ),
      ),
    );
  }

  void _showCardActions(dynamic card) {
    final isFrozen = card['isFrozen'] == true;
    showModalBottomSheet(
      context: context,
      shape: const RoundedRectangleBorder(borderRadius: BorderRadius.vertical(top: Radius.circular(20))),
      builder: (ctx) => SafeArea(
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Column(mainAxisSize: MainAxisSize.min, children: [
            Container(width: 40, height: 4, decoration: BoxDecoration(color: context.dividerColor, borderRadius: BorderRadius.circular(2))),
            const SizedBox(height: 20),
            Text('Действия с картой', style: TextStyle(fontSize: 20, fontWeight: FontWeight.bold, color: context.textPrimary)),
            const SizedBox(height: 20),
            _ActionTile(
              icon: isFrozen ? Icons.lock_open : Icons.lock,
              title: isFrozen ? 'Разморозить' : 'Заморозить',
              subtitle: isFrozen ? 'Активировать карту' : 'Временная блокировка',
              color: isFrozen ? AppColors.success : AppColors.warning,
              onTap: () async {
                Navigator.pop(ctx);
                final action = isFrozen ? 'unfreeze' : 'freeze';
                await context.read<ApiService>().post('cards/${card['id']}/$action');
                _load();
              },
            ),
            _ActionTile(
              icon: Icons.visibility,
              title: 'Показать CVV',
              subtitle: 'Безопасный просмотр данных карты',
              color: AppColors.primary,
              onTap: () {
                Navigator.pop(ctx);
                _showCVV(card);
              },
            ),
            _ActionTile(
              icon: Icons.copy,
              title: 'Скопировать номер',
              subtitle: 'Скопировать полный номер карты',
              color: AppColors.accent,
              onTap: () {
                Navigator.pop(ctx);
                Clipboard.setData(ClipboardData(text: card['cardNumber'] ?? ''));
                ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Номер скопирован')));
              },
            ),
          ]),
        ),
      ),
    );
  }

  void _showCVV(dynamic card) {
    showDialog(
      context: context,
      builder: (_) => AlertDialog(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
        title: const Text('Данные карты'),
        content: Column(mainAxisSize: MainAxisSize.min, children: [
          _InfoRow('Номер', card['cardNumber'] ?? '****'),
          _InfoRow('Владелец', card['cardHolderName'] ?? ''),
          _InfoRow('Срок', _formatExpiry(card['expiryDate'])),
        ]),
        actions: [TextButton(onPressed: () => Navigator.pop(context), child: const Text('Закрыть'))],
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

  String _formatExpiry(dynamic raw) {
    if (raw == null) return '--/--';
    try {
      final dt = DateTime.parse(raw.toString());
      return '${dt.month.toString().padLeft(2, '0')}/${dt.year.toString().substring(2)}';
    } catch (_) {
      return raw.toString();
    }
  }

  void _createCard() {
    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
        title: const Text('Создать карту'),
        content: const Text('Будет создана виртуальная карта для совершения покупок в интернете.'),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx), child: const Text('Отмена')),
          ElevatedButton(
            onPressed: () async {
              Navigator.pop(ctx);
              try {
                final accountId = await _getMainAccountId();
                if (accountId == null) throw Exception('Основной счёт не найден');
                await context.read<ApiService>().post('cards', body: {'accountId': accountId});
                _load();
                if (mounted) ScaffoldMessenger.of(context).showSnackBar(
                  const SnackBar(content: Text('Карта создана!')),
                );
              } catch (e) {
                if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(e.toString())));
              }
            },
            child: const Text('Создать'),
          ),
        ],
      ),
    );
  }
}

class _ActionTile extends StatelessWidget {
  final IconData icon;
  final String title, subtitle;
  final Color color;
  final VoidCallback onTap;
  const _ActionTile({required this.icon, required this.title, required this.subtitle, required this.color, required this.onTap});
  @override
  Widget build(BuildContext context) {
    return ListTile(
      leading: Container(
        width: 44, height: 44,
        decoration: BoxDecoration(color: color.withValues(alpha: 0.15), borderRadius: BorderRadius.circular(12)),
        child: Icon(icon, color: color),
      ),
      title: Text(title, style: const TextStyle(fontWeight: FontWeight.w600)),
      subtitle: Text(subtitle, style: TextStyle(fontSize: 12, color: context.textSecondary)),
      trailing: Icon(Icons.chevron_right, color: context.textHint),
      onTap: onTap,
    );
  }
}

class _InfoRow extends StatelessWidget {
  final String label, value;
  const _InfoRow(this.label, this.value);
  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 6),
      child: Row(mainAxisAlignment: MainAxisAlignment.spaceBetween, children: [
        Text(label, style: TextStyle(color: context.textSecondary)),
        SelectableText(value, style: const TextStyle(fontWeight: FontWeight.w600)),
      ]),
    );
  }
}
