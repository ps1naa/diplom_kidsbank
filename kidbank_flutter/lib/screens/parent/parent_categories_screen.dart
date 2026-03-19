import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/api_service.dart';
import '../../core/theme.dart';

class ParentCategoriesScreen extends StatefulWidget {
  final String kidId;
  final String kidName;
  const ParentCategoriesScreen({super.key, required this.kidId, required this.kidName});
  @override
  State<ParentCategoriesScreen> createState() => _ParentCategoriesScreenState();
}

class _ParentCategoriesScreenState extends State<ParentCategoriesScreen> {
  List<dynamic> _categories = [];
  Set<String> _blockedIds = {};
  bool _loading = true;

  @override
  void initState() { super.initState(); _load(); }

  Future<void> _load() async {
    final api = context.read<ApiService>();
    try {
      final cats = await api.get('categories') as List;
      final blocked = <String>{};
      for (final c in cats) {
        if (c['isBlocked'] == true) blocked.add(c['id']);
      }
      setState(() { _categories = cats; _blockedIds = blocked; _loading = false; });
    } catch (_) { setState(() => _loading = false); }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text('Категории: ${widget.kidName}')),
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
    final isBlocked = _blockedIds.contains(cat['id']);
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
          subtitle: Text(isBlocked ? 'Заблокирована' : 'Доступна', style: TextStyle(color: isBlocked ? AppColors.error : AppColors.success, fontSize: 12)),
          trailing: Switch(
            value: !isBlocked,
            onChanged: (val) => _toggleCategory(cat['id'], !val),
            activeColor: AppColors.success,
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

  Future<void> _toggleCategory(String catId, bool block) async {
    try {
      await context.read<ApiService>().post('categories/block', body: {
        'kidId': widget.kidId,
        'categoryId': catId,
        'isBlocked': block,
      });
      setState(() {
        if (block) { _blockedIds.add(catId); } else { _blockedIds.remove(catId); }
      });
    } catch (e) {
      if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(e.toString())));
    }
  }
}
