import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/api_service.dart';
import '../../core/theme.dart';

class KidCategoriesScreen extends StatefulWidget {
  const KidCategoriesScreen({super.key});
  @override
  State<KidCategoriesScreen> createState() => _KidCategoriesScreenState();
}

class _KidCategoriesScreenState extends State<KidCategoriesScreen> {
  List<dynamic> _categories = [];
  bool _loading = true;

  @override
  void initState() { super.initState(); _load(); }

  Future<void> _load() async {
    try {
      final data = await context.read<ApiService>().get('categories');
      setState(() { _categories = data as List; _loading = false; });
    } catch (_) { setState(() => _loading = false); }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Категории расходов')),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : ListView.builder(
              padding: const EdgeInsets.all(16),
              itemCount: _categories.length,
              itemBuilder: (_, i) => _buildCategory(_categories[i], i),
            ),
    );
  }

  Widget _buildCategory(dynamic cat, int index) {
    final isBlocked = cat['isBlocked'] == true;
    final name = cat['name'] ?? '';
    return TweenAnimationBuilder<double>(
      tween: Tween(begin: 0, end: 1),
      duration: Duration(milliseconds: 200 + index * 50),
      curve: Curves.easeOutCubic,
      builder: (_, val, child) => Opacity(opacity: val, child: Transform.translate(offset: Offset(0, 15 * (1 - val)), child: child)),
      child: Card(
        margin: const EdgeInsets.only(bottom: 8),
        child: ListTile(
          leading: Container(
            width: 44, height: 44,
            decoration: BoxDecoration(
              color: (isBlocked ? AppColors.error : AppColors.success).withValues(alpha: 0.15),
              borderRadius: BorderRadius.circular(12),
            ),
            child: Icon(
              _getCategoryIcon(cat['iconName']),
              color: isBlocked ? AppColors.error : AppColors.success,
            ),
          ),
          title: Text(name, style: const TextStyle(fontWeight: FontWeight.w500)),
          subtitle: Text(
            isBlocked ? 'Заблокирована родителем' : 'Доступна для покупок',
            style: TextStyle(color: isBlocked ? AppColors.error : AppColors.success, fontSize: 12),
          ),
          trailing: Container(
            padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
            decoration: BoxDecoration(
              color: (isBlocked ? AppColors.error : AppColors.success).withValues(alpha: 0.1),
              borderRadius: BorderRadius.circular(12),
            ),
            child: Text(
              isBlocked ? 'Закрыто' : 'Открыто',
              style: TextStyle(color: isBlocked ? AppColors.error : AppColors.success, fontSize: 12, fontWeight: FontWeight.w600),
            ),
          ),
        ),
      ),
    );
  }

  IconData _getCategoryIcon(String? code) {
    switch (code?.toLowerCase()) {
      case 'food': return Icons.restaurant;
      case 'games': return Icons.sports_esports;
      case 'education': return Icons.school;
      case 'transport': return Icons.directions_bus;
      case 'clothes': return Icons.checkroom;
      case 'entertainment': return Icons.movie;
      case 'health': return Icons.local_hospital;
      case 'sports': return Icons.fitness_center;
      default: return Icons.category;
    }
  }
}
