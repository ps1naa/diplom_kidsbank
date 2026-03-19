import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/theme.dart';
import '../../core/constants.dart';
import '../../providers/auth_provider.dart';
import '../../widgets/app_text_field.dart';
import '../../widgets/loading_overlay.dart';

class RegisterKidScreen extends StatefulWidget {
  const RegisterKidScreen({super.key});
  @override
  State<RegisterKidScreen> createState() => _RegisterKidScreenState();
}

class _RegisterKidScreenState extends State<RegisterKidScreen> {
  final _formKey = GlobalKey<FormState>();
  final _tokenCtrl = TextEditingController();
  final _emailCtrl = TextEditingController();
  final _passCtrl = TextEditingController();
  final _firstNameCtrl = TextEditingController();
  final _lastNameCtrl = TextEditingController();
  DateTime? _dob;
  bool _loading = false;
  bool _obscure = true;
  bool _agreeOffer = false;
  bool _agreePrivacy = false;
  String? _error;

  Future<void> _register() async {
    if (!_formKey.currentState!.validate()) return;
    if (!_agreeOffer || !_agreePrivacy) { setState(() => _error = 'Необходимо принять все соглашения'); return; }
    if (_dob == null) { setState(() => _error = 'Укажите дату рождения'); return; }
    setState(() { _loading = true; _error = null; });
    try {
      await context.read<AuthProvider>().registerKid(
        invitationToken: _tokenCtrl.text.trim(), email: _emailCtrl.text.trim(), password: _passCtrl.text,
        firstName: _firstNameCtrl.text.trim(), lastName: _lastNameCtrl.text.trim(), dateOfBirth: _dob!.toIso8601String().split('T')[0],
      );
      if (mounted) Navigator.of(context).popUntil((route) => route.isFirst);
    } catch (e) { setState(() => _error = e.toString()); }
    finally { if (mounted) setState(() => _loading = false); }
  }

  Future<void> _pickDate() async {
    final d = await showDatePicker(context: context, initialDate: DateTime(2012), firstDate: DateTime(2005), lastDate: DateTime.now());
    if (d != null) setState(() => _dob = d);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text(AppStrings.registerAsKid)),
      body: LoadingOverlay(
        isLoading: _loading,
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(24),
          child: Form(
            key: _formKey,
            child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
              Container(
                width: double.infinity, padding: const EdgeInsets.all(16),
                decoration: BoxDecoration(
                  gradient: LinearGradient(colors: [AppColors.kidGradient1.withValues(alpha: 0.2), context.surfaceVariant]),
                  borderRadius: BorderRadius.circular(16),
                  border: Border.all(color: AppColors.kidGradient1.withValues(alpha: 0.5)),
                ),
                child: Row(children: [
                  const Icon(Icons.info_outline, color: AppColors.kidGradient1),
                  const SizedBox(width: 12),
                  Expanded(child: Text('Попроси родителя создать приглашение в приложении и введи код ниже', style: TextStyle(color: context.textSecondary, fontSize: 13))),
                ]),
              ),
              const SizedBox(height: 20),
              if (_error != null) Container(
                width: double.infinity, padding: const EdgeInsets.all(12), margin: const EdgeInsets.only(bottom: 16),
                decoration: BoxDecoration(color: context.surfaceVariant, borderRadius: BorderRadius.circular(12), border: Border.all(color: AppColors.error.withValues(alpha: 0.5))),
                child: Text(_error!, style: const TextStyle(color: AppColors.error)),
              ),
              AppTextField(controller: _tokenCtrl, label: AppStrings.invitationCode, hint: 'Вставьте код приглашения', prefixIcon: Icons.vpn_key_outlined, validator: (v) => v!.isEmpty ? 'Введите код' : null),
              const SizedBox(height: 16),
              Row(children: [
                Expanded(child: AppTextField(controller: _firstNameCtrl, label: AppStrings.firstName, prefixIcon: Icons.person_outline, validator: (v) => v!.isEmpty ? 'Обязательное' : null)),
                const SizedBox(width: 12),
                Expanded(child: AppTextField(controller: _lastNameCtrl, label: AppStrings.lastName, prefixIcon: Icons.person_outline, validator: (v) => v!.isEmpty ? 'Обязательное' : null)),
              ]),
              const SizedBox(height: 16),
              AppTextField(controller: _emailCtrl, label: AppStrings.email, hint: 'example@mail.com', prefixIcon: Icons.email_outlined, keyboardType: TextInputType.emailAddress, validator: (v) => v != null && v.contains('@') ? null : 'Введите email'),
              const SizedBox(height: 16),
              AppTextField(controller: _passCtrl, label: AppStrings.password, hint: '••••••••', prefixIcon: Icons.lock_outline, obscureText: _obscure,
                suffix: IconButton(icon: Icon(_obscure ? Icons.visibility_off : Icons.visibility, size: 20), onPressed: () => setState(() => _obscure = !_obscure)),
                validator: (v) => v != null && v.length >= 8 ? null : 'Минимум 8 символов'),
              const SizedBox(height: 16),
              AppTextField(label: AppStrings.dateOfBirth, hint: _dob != null ? '${_dob!.day.toString().padLeft(2, '0')}.${_dob!.month.toString().padLeft(2, '0')}.${_dob!.year}' : 'Выберите дату', prefixIcon: Icons.calendar_today_outlined, readOnly: true, onTap: _pickDate),
              const SizedBox(height: 24),
              _AgreementCheckbox(value: _agreeOffer, onChanged: (v) => setState(() => _agreeOffer = v!), text: AppStrings.agreementText, linkText: AppStrings.offerAgreement),
              const SizedBox(height: 8),
              _AgreementCheckbox(value: _agreePrivacy, onChanged: (v) => setState(() => _agreePrivacy = v!), text: AppStrings.agreementText, linkText: AppStrings.privacyPolicy),
              const SizedBox(height: 24),
              SizedBox(width: double.infinity, child: ElevatedButton(
                onPressed: _register,
                style: ElevatedButton.styleFrom(backgroundColor: AppColors.kidGradient1),
                child: const Text('Зарегистрироваться'),
              )),
            ]),
          ),
        ),
      ),
    );
  }

  @override
  void dispose() { _tokenCtrl.dispose(); _emailCtrl.dispose(); _passCtrl.dispose(); _firstNameCtrl.dispose(); _lastNameCtrl.dispose(); super.dispose(); }
}

class _AgreementCheckbox extends StatelessWidget {
  final bool value;
  final ValueChanged<bool?> onChanged;
  final String text, linkText;
  const _AgreementCheckbox({required this.value, required this.onChanged, required this.text, required this.linkText});
  @override
  Widget build(BuildContext context) {
    return Row(crossAxisAlignment: CrossAxisAlignment.start, children: [
      SizedBox(width: 24, height: 24, child: Checkbox(value: value, onChanged: onChanged, activeColor: AppColors.primary, shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(4)))),
      const SizedBox(width: 8),
      Expanded(child: GestureDetector(
        onTap: () => onChanged(!value),
        child: RichText(text: TextSpan(
          style: TextStyle(color: context.textSecondary, fontSize: 14),
          children: [TextSpan(text: '$text '), TextSpan(text: linkText, style: const TextStyle(color: AppColors.primary, decoration: TextDecoration.underline))],
        )),
      )),
    ]);
  }
}
