import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/theme.dart';
import '../../core/constants.dart';
import '../../core/router.dart';
import '../../providers/auth_provider.dart';
import '../../widgets/app_text_field.dart';
import '../../widgets/loading_overlay.dart';

class LoginScreen extends StatefulWidget {
  const LoginScreen({super.key});
  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  final _formKey = GlobalKey<FormState>();
  final _emailCtrl = TextEditingController();
  final _passCtrl = TextEditingController();
  bool _loading = false;
  bool _obscure = true;
  String? _error;

  Future<void> _login() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() { _loading = true; _error = null; });
    try {
      await context.read<AuthProvider>().login(_emailCtrl.text.trim(), _passCtrl.text);
    } catch (e) {
      setState(() => _error = e.toString());
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: LoadingOverlay(
        isLoading: _loading,
        child: SafeArea(
          child: SingleChildScrollView(
            padding: const EdgeInsets.all(24),
            child: Form(
              key: _formKey,
              child: Column(children: [
                const SizedBox(height: 40),
                Container(
                  width: 80, height: 80,
                  decoration: BoxDecoration(
                    gradient: const LinearGradient(colors: [AppColors.kidGradient1, AppColors.kidGradient2]),
                    borderRadius: BorderRadius.circular(20),
                  ),
                  child: const Icon(Icons.account_balance_wallet, color: Colors.white, size: 40),
                ),
                const SizedBox(height: 16),
                const Text('KidBank', style: TextStyle(fontSize: 32, fontWeight: FontWeight.bold, color: AppColors.primary)),
                const SizedBox(height: 8),
                Text('Мобильный банкинг для детей', style: TextStyle(color: context.textSecondary)),
                const SizedBox(height: 40),
                if (_error != null) Container(
                  width: double.infinity,
                  padding: const EdgeInsets.all(12),
                  margin: const EdgeInsets.only(bottom: 16),
                  decoration: BoxDecoration(
                    color: context.surfaceVariant,
                    borderRadius: BorderRadius.circular(12),
                    border: Border.all(color: AppColors.error.withValues(alpha: 0.5)),
                  ),
                  child: Text(_error!, style: const TextStyle(color: AppColors.error)),
                ),
                AppTextField(
                  controller: _emailCtrl, label: AppStrings.email, hint: 'example@mail.com',
                  prefixIcon: Icons.email_outlined, keyboardType: TextInputType.emailAddress,
                  validator: (v) => v != null && v.contains('@') ? null : 'Введите корректный email',
                ),
                const SizedBox(height: 16),
                AppTextField(
                  controller: _passCtrl, label: AppStrings.password, hint: '••••••••', prefixIcon: Icons.lock_outline,
                  obscureText: _obscure,
                  suffix: IconButton(icon: Icon(_obscure ? Icons.visibility_off : Icons.visibility, size: 20), onPressed: () => setState(() => _obscure = !_obscure)),
                  validator: (v) => v != null && v.length >= 6 ? null : 'Минимум 6 символов',
                ),
                const SizedBox(height: 24),
                SizedBox(width: double.infinity, child: ElevatedButton(onPressed: _login, child: const Text(AppStrings.login))),
                const SizedBox(height: 16),
                Row(mainAxisAlignment: MainAxisAlignment.center, children: [
                  Text('Нет аккаунта? ', style: TextStyle(color: context.textSecondary)),
                  TextButton(onPressed: () => _showRegisterChoice(context), child: const Text('Регистрация')),
                ]),
              ]),
            ),
          ),
        ),
      ),
    );
  }

  void _showRegisterChoice(BuildContext context) {
    showModalBottomSheet(
      context: context,
      shape: const RoundedRectangleBorder(borderRadius: BorderRadius.vertical(top: Radius.circular(20))),
      builder: (_) => Padding(
        padding: const EdgeInsets.all(24),
        child: Column(mainAxisSize: MainAxisSize.min, children: [
          Container(width: 40, height: 4, decoration: BoxDecoration(color: context.dividerColor, borderRadius: BorderRadius.circular(2))),
          const SizedBox(height: 24),
          Text('Выберите тип аккаунта', style: TextStyle(fontSize: 20, fontWeight: FontWeight.bold, color: context.textPrimary)),
          const SizedBox(height: 24),
          _RegisterOption(icon: Icons.person, title: 'Родитель', subtitle: 'Управляйте финансами семьи', color: AppColors.primary,
            onTap: () { Navigator.pop(context); Navigator.pushNamed(context, AppRouter.registerParent); }),
          const SizedBox(height: 12),
          _RegisterOption(icon: Icons.child_care, title: 'Ребёнок', subtitle: 'Учись управлять деньгами', color: AppColors.kidGradient1,
            onTap: () { Navigator.pop(context); Navigator.pushNamed(context, AppRouter.registerKid); }),
          const SizedBox(height: 16),
        ]),
      ),
    );
  }

  @override
  void dispose() { _emailCtrl.dispose(); _passCtrl.dispose(); super.dispose(); }
}

class _RegisterOption extends StatelessWidget {
  final IconData icon;
  final String title, subtitle;
  final Color color;
  final VoidCallback onTap;
  const _RegisterOption({required this.icon, required this.title, required this.subtitle, required this.color, required this.onTap});
  @override
  Widget build(BuildContext context) {
    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(16),
      child: Container(
        padding: const EdgeInsets.all(16),
        decoration: BoxDecoration(
          color: context.surfaceVariant,
          border: Border.all(color: color.withValues(alpha: 0.5)),
          borderRadius: BorderRadius.circular(16),
        ),
        child: Row(children: [
          Container(width: 48, height: 48, decoration: BoxDecoration(color: color.withValues(alpha: 0.2), borderRadius: BorderRadius.circular(12)), child: Icon(icon, color: color)),
          const SizedBox(width: 16),
          Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
            Text(title, style: TextStyle(fontSize: 16, fontWeight: FontWeight.w600, color: color)),
            Text(subtitle, style: TextStyle(fontSize: 13, color: context.textSecondary)),
          ])),
          Icon(Icons.chevron_right, color: color),
        ]),
      ),
    );
  }
}
