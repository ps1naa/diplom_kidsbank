import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:fl_chart/fl_chart.dart';
import '../../core/api_service.dart';
import '../../core/theme.dart';

class AnalyticsScreen extends StatefulWidget {
  final String? kidId;
  const AnalyticsScreen({super.key, this.kidId});
  @override
  State<AnalyticsScreen> createState() => _AnalyticsScreenState();
}

class _AnalyticsScreenState extends State<AnalyticsScreen> {
  Map<String, dynamic>? _summary;
  List<dynamic>? _monthly;
  bool _loading = true;

  @override
  void initState() { super.initState(); _load(); }

  Future<void> _load() async {
    final api = context.read<ApiService>();
    final kidId = widget.kidId;
    if (kidId == null) { setState(() => _loading = false); return; }
    try {
      final results = await Future.wait([
        api.get('analytics/kid/$kidId/summary'),
        api.get('analytics/kid/$kidId/monthly?months=6'),
      ]);
      setState(() {
        _summary = results[0] is Map ? results[0] as Map<String, dynamic> : null;
        _monthly = results[1] is List ? results[1] as List : null;
        _loading = false;
      });
    } catch (_) { setState(() => _loading = false); }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Аналитика')),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : RefreshIndicator(
              onRefresh: _load,
              child: ListView(
                padding: const EdgeInsets.all(16),
                children: [
                  if (_summary != null) ...[
                    Row(children: [
                      Expanded(child: _StatCard('Расходы', '${_summary!['totalSpentThisMonth'] ?? 0} BYN', Icons.trending_down, AppColors.error)),
                      const SizedBox(width: 12),
                      Expanded(child: _StatCard('Доходы', '${_summary!['totalEarnedThisMonth'] ?? 0} BYN', Icons.trending_up, AppColors.success)),
                    ]),
                    const SizedBox(height: 12),
                    Row(children: [
                      Expanded(child: _StatCard('Задания', '${_summary!['tasksCompletedThisMonth'] ?? 0}', Icons.assignment, AppColors.primary)),
                      const SizedBox(width: 12),
                      Expanded(child: _StatCard('Цели', '${_summary!['goalsCompleted'] ?? 0}', Icons.flag, AppColors.accent)),
                    ]),
                    const SizedBox(height: 12),
                    Row(children: [
                      Expanded(child: _StatCard('Баланс', '${_summary!['totalBalance'] ?? 0} BYN', Icons.account_balance_wallet, AppColors.kidGradient1)),
                      const SizedBox(width: 12),
                      Expanded(child: _StatCard('Копилки', '${_summary!['goalsSavings'] ?? 0} BYN', Icons.savings, AppColors.warning)),
                    ]),
                  ],
                  if (_monthly != null && _monthly!.isNotEmpty) ...[
                    const SizedBox(height: 24),
                    Text('Динамика расходов', style: TextStyle(fontSize: 18, fontWeight: FontWeight.w600, color: context.textPrimary)),
                    const SizedBox(height: 16),
                    _buildBarChart(),
                  ],
                ],
              ),
            ),
    );
  }

  Widget _buildBarChart() {
    if (_monthly == null || _monthly!.isEmpty) return const SizedBox.shrink();
    return SizedBox(
      height: 200,
      child: BarChart(BarChartData(
        gridData: FlGridData(show: false),
        borderData: FlBorderData(show: false),
        titlesData: FlTitlesData(
          leftTitles: const AxisTitles(sideTitles: SideTitles(showTitles: false)),
          topTitles: const AxisTitles(sideTitles: SideTitles(showTitles: false)),
          rightTitles: const AxisTitles(sideTitles: SideTitles(showTitles: false)),
          bottomTitles: AxisTitles(sideTitles: SideTitles(
            showTitles: true,
            getTitlesWidget: (val, meta) {
              final idx = val.toInt();
              if (idx >= 0 && idx < _monthly!.length) {
                return Padding(
                  padding: const EdgeInsets.only(top: 4),
                  child: Text(_monthly![idx]['monthName']?.toString() ?? '${_monthly![idx]['month'] ?? ''}', style: TextStyle(fontSize: 10, color: context.textSecondary)),
                );
              }
              return const SizedBox.shrink();
            },
          )),
        ),
        barGroups: _monthly!.asMap().entries.map((e) => BarChartGroupData(
          x: e.key,
          barRods: [BarChartRodData(
            toY: (e.value['totalExpenses'] as num?)?.toDouble() ?? 0,
            color: AppColors.primary,
            width: 20,
            borderRadius: const BorderRadius.vertical(top: Radius.circular(6)),
          )],
        )).toList(),
      )),
    );
  }
}

class _StatCard extends StatelessWidget {
  final String title, value;
  final IconData icon;
  final Color color;
  const _StatCard(this.title, this.value, this.icon, this.color);
  @override
  Widget build(BuildContext context) {
    return Card(
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
          Row(children: [
            Container(
              width: 36, height: 36,
              decoration: BoxDecoration(color: color.withValues(alpha: 0.15), borderRadius: BorderRadius.circular(10)),
              child: Icon(icon, color: color, size: 20),
            ),
            const Spacer(),
          ]),
          const SizedBox(height: 12),
          Text(value, style: TextStyle(fontSize: 20, fontWeight: FontWeight.bold, color: context.textPrimary)),
          const SizedBox(height: 2),
          Text(title, style: TextStyle(fontSize: 12, color: context.textSecondary)),
        ]),
      ),
    );
  }
}
