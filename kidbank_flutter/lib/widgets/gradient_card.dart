import 'package:flutter/material.dart';
import '../core/theme.dart';

class GradientCard extends StatelessWidget {
  final Widget child;
  final List<Color>? colors;
  final double borderRadius;
  const GradientCard({super.key, required this.child, this.colors, this.borderRadius = 20});

  @override
  Widget build(BuildContext context) {
    final c = colors ?? [AppColors.cardGradient1, AppColors.cardGradient2];
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        gradient: LinearGradient(colors: c, begin: Alignment.topLeft, end: Alignment.bottomRight),
        borderRadius: BorderRadius.circular(borderRadius),
        boxShadow: [BoxShadow(color: c.first.withValues(alpha: 0.3), blurRadius: 12, offset: const Offset(0, 6))],
      ),
      child: child,
    );
  }
}
