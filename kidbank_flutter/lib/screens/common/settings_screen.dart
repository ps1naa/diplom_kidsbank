import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/theme.dart';
import '../../providers/theme_provider.dart';

class SettingsScreen extends StatelessWidget {
  const SettingsScreen({super.key});

  @override
  Widget build(BuildContext context) {
    final theme = context.watch<ThemeProvider>();
    return Scaffold(
      appBar: AppBar(title: const Text('Настройки')),
      body: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          Text('Внешний вид', style: TextStyle(fontSize: 14, fontWeight: FontWeight.w600, color: context.textSecondary)),
          const SizedBox(height: 8),
          Card(
            child: Column(children: [
              _ThemeOption(
                icon: Icons.dark_mode,
                title: 'Тёмная тема',
                isSelected: theme.mode == ThemeMode.dark,
                onTap: () => theme.setMode(ThemeMode.dark),
              ),
              Divider(height: 1, color: context.dividerColor),
              _ThemeOption(
                icon: Icons.light_mode,
                title: 'Светлая тема',
                isSelected: theme.mode == ThemeMode.light,
                onTap: () => theme.setMode(ThemeMode.light),
              ),
              Divider(height: 1, color: context.dividerColor),
              _ThemeOption(
                icon: Icons.brightness_auto,
                title: 'Системная тема',
                isSelected: theme.mode == ThemeMode.system,
                onTap: () => theme.setMode(ThemeMode.system),
              ),
            ]),
          ),
          const SizedBox(height: 24),
          Text('Уведомления', style: TextStyle(fontSize: 14, fontWeight: FontWeight.w600, color: context.textSecondary)),
          const SizedBox(height: 8),
          Card(
            child: Column(children: [
              SwitchListTile(
                title: const Text('Push-уведомления'),
                subtitle: Text('Получать уведомления на телефон', style: TextStyle(fontSize: 12, color: context.textSecondary)),
                value: true,
                onChanged: (_) {},
                secondary: Icon(Icons.notifications, color: AppColors.primary),
              ),
              Divider(height: 1, color: context.dividerColor),
              SwitchListTile(
                title: const Text('Уведомления о транзакциях'),
                subtitle: Text('При каждом списании или зачислении', style: TextStyle(fontSize: 12, color: context.textSecondary)),
                value: true,
                onChanged: (_) {},
                secondary: Icon(Icons.payment, color: AppColors.primary),
              ),
            ]),
          ),
          const SizedBox(height: 24),
          Text('Безопасность', style: TextStyle(fontSize: 14, fontWeight: FontWeight.w600, color: context.textSecondary)),
          const SizedBox(height: 8),
          Card(
            child: Column(children: [
              ListTile(
                leading: Icon(Icons.lock, color: AppColors.primary),
                title: const Text('Изменить пароль'),
                trailing: Icon(Icons.chevron_right, color: context.textHint),
                onTap: () {},
              ),
              Divider(height: 1, color: context.dividerColor),
              ListTile(
                leading: Icon(Icons.fingerprint, color: AppColors.primary),
                title: const Text('Биометрия'),
                trailing: Switch(value: false, onChanged: (_) {}),
              ),
            ]),
          ),
          const SizedBox(height: 24),
          Text('О приложении', style: TextStyle(fontSize: 14, fontWeight: FontWeight.w600, color: context.textSecondary)),
          const SizedBox(height: 8),
          Card(
            child: Column(children: [
              ListTile(
                leading: Icon(Icons.info_outline, color: AppColors.primary),
                title: const Text('Версия'),
                trailing: Text('1.0.0', style: TextStyle(color: context.textSecondary)),
              ),
            ]),
          ),
        ],
      ),
    );
  }
}

class _ThemeOption extends StatelessWidget {
  final IconData icon;
  final String title;
  final bool isSelected;
  final VoidCallback onTap;
  const _ThemeOption({required this.icon, required this.title, required this.isSelected, required this.onTap});
  @override
  Widget build(BuildContext context) {
    return ListTile(
      leading: Icon(icon, color: isSelected ? AppColors.primary : context.textSecondary),
      title: Text(title, style: TextStyle(fontWeight: isSelected ? FontWeight.w600 : FontWeight.w400)),
      trailing: isSelected
          ? const Icon(Icons.check_circle, color: AppColors.primary)
          : Icon(Icons.circle_outlined, color: context.textHint),
      onTap: onTap,
    );
  }
}
