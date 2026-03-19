import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

class AppColors {
  static const primary = Color(0xFF00BFA6);
  static const primaryDark = Color(0xFF00A389);
  static const primaryLight = Color(0xFF1A3A3A);
  static const accent = Color(0xFFFFC107);
  static const accentLight = Color(0xFF3A3520);
  static const success = Color(0xFF00E676);
  static const error = Color(0xFFFF5252);
  static const warning = Color(0xFFFFAB40);

  static const backgroundDark = Color(0xFF0D1117);
  static const surfaceDark = Color(0xFF161B22);
  static const surfaceLightDark = Color(0xFF1C2333);
  static const cardDark = Color(0xFF1C2333);
  static const textPrimaryDark = Color(0xFFE6EDF3);
  static const textSecondaryDark = Color(0xFF8B949E);
  static const textHintDark = Color(0xFF484F58);
  static const dividerDark = Color(0xFF21262D);

  static const backgroundLight = Color(0xFFF5F7FA);
  static const surfaceLight = Color(0xFFFFFFFF);
  static const surfaceLightLight = Color(0xFFF0F2F5);
  static const cardLight = Color(0xFFFFFFFF);
  static const textPrimaryLight = Color(0xFF1A1A2E);
  static const textSecondaryLight = Color(0xFF6B7280);
  static const textHintLight = Color(0xFF9CA3AF);
  static const dividerLight = Color(0xFFE5E7EB);

  static const cardGradient1 = Color(0xFF00BFA6);
  static const cardGradient2 = Color(0xFF00897B);
  static const kidGradient1 = Color(0xFF00BFA6);
  static const kidGradient2 = Color(0xFF26C6DA);
}

class AppTheme {
  static ThemeData get darkTheme => _buildTheme(Brightness.dark);
  static ThemeData get lightTheme => _buildTheme(Brightness.light);

  static ThemeData _buildTheme(Brightness brightness) {
    final isDark = brightness == Brightness.dark;
    final bg = isDark ? AppColors.backgroundDark : AppColors.backgroundLight;
    final surface = isDark ? AppColors.surfaceDark : AppColors.surfaceLight;
    final card = isDark ? AppColors.cardDark : AppColors.cardLight;
    final surfaceVariant = isDark ? AppColors.surfaceLightDark : AppColors.surfaceLightLight;
    final textPrimary = isDark ? AppColors.textPrimaryDark : AppColors.textPrimaryLight;
    final textSecondary = isDark ? AppColors.textSecondaryDark : AppColors.textSecondaryLight;
    final textHint = isDark ? AppColors.textHintDark : AppColors.textHintLight;
    final divider = isDark ? AppColors.dividerDark : AppColors.dividerLight;

    final baseText = isDark
        ? GoogleFonts.interTextTheme(ThemeData.dark().textTheme)
        : GoogleFonts.interTextTheme(ThemeData.light().textTheme);

    return ThemeData(
      useMaterial3: true,
      brightness: brightness,
      colorScheme: isDark
          ? const ColorScheme.dark(
              primary: AppColors.primary,
              secondary: AppColors.accent,
              surface: AppColors.surfaceDark,
              error: AppColors.error,
              onPrimary: Colors.white,
              onSecondary: Colors.black,
              onSurface: AppColors.textPrimaryDark,
              onError: Colors.white,
            )
          : const ColorScheme.light(
              primary: AppColors.primary,
              secondary: AppColors.accent,
              surface: AppColors.surfaceLight,
              error: AppColors.error,
              onPrimary: Colors.white,
              onSecondary: Colors.black,
              onSurface: AppColors.textPrimaryLight,
              onError: Colors.white,
            ),
      scaffoldBackgroundColor: bg,
      textTheme: baseText,
      appBarTheme: AppBarTheme(
        backgroundColor: bg,
        elevation: 0,
        centerTitle: true,
        scrolledUnderElevation: 0,
        titleTextStyle: GoogleFonts.inter(
          fontSize: 18, fontWeight: FontWeight.w600, color: textPrimary,
        ),
        iconTheme: IconThemeData(color: textPrimary),
      ),
      cardTheme: CardTheme(
        elevation: isDark ? 0 : 2,
        shadowColor: isDark ? Colors.transparent : Colors.black12,
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
        color: card,
      ),
      elevatedButtonTheme: ElevatedButtonThemeData(
        style: ElevatedButton.styleFrom(
          backgroundColor: AppColors.primary,
          foregroundColor: Colors.white,
          elevation: 0,
          padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 14),
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
          textStyle: GoogleFonts.inter(fontSize: 16, fontWeight: FontWeight.w600),
        ),
      ),
      outlinedButtonTheme: OutlinedButtonThemeData(
        style: OutlinedButton.styleFrom(
          foregroundColor: AppColors.primary,
          side: const BorderSide(color: AppColors.primary),
          padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 14),
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
          textStyle: GoogleFonts.inter(fontSize: 16, fontWeight: FontWeight.w600),
        ),
      ),
      inputDecorationTheme: InputDecorationTheme(
        filled: true,
        fillColor: surfaceVariant,
        contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
        border: OutlineInputBorder(
          borderRadius: BorderRadius.circular(12),
          borderSide: BorderSide(color: divider),
        ),
        enabledBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(12),
          borderSide: BorderSide(color: divider),
        ),
        focusedBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(12),
          borderSide: const BorderSide(color: AppColors.primary, width: 2),
        ),
        errorBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(12),
          borderSide: const BorderSide(color: AppColors.error),
        ),
        hintStyle: GoogleFonts.inter(color: textHint),
        labelStyle: GoogleFonts.inter(color: textSecondary),
      ),
      bottomNavigationBarTheme: BottomNavigationBarThemeData(
        backgroundColor: surface,
        selectedItemColor: AppColors.primary,
        unselectedItemColor: textHint,
        type: BottomNavigationBarType.fixed,
        elevation: isDark ? 0 : 8,
      ),
      dialogTheme: DialogTheme(
        backgroundColor: surface,
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
      ),
      bottomSheetTheme: BottomSheetThemeData(backgroundColor: surface),
      chipTheme: ChipThemeData(
        backgroundColor: isDark ? AppColors.primaryLight : const Color(0xFFE0F2F1),
        labelStyle: GoogleFonts.inter(color: AppColors.primary, fontWeight: FontWeight.w500),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
      ),
      dividerTheme: DividerThemeData(color: divider),
      snackBarTheme: SnackBarThemeData(
        backgroundColor: surfaceVariant,
        contentTextStyle: GoogleFonts.inter(color: textPrimary),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
        behavior: SnackBarBehavior.floating,
      ),
      checkboxTheme: CheckboxThemeData(
        fillColor: WidgetStateProperty.resolveWith((states) =>
            states.contains(WidgetState.selected) ? AppColors.primary : Colors.transparent),
        checkColor: WidgetStateProperty.all(Colors.white),
        side: BorderSide(color: textHint),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(4)),
      ),
      floatingActionButtonTheme: const FloatingActionButtonThemeData(
        backgroundColor: AppColors.primary,
        foregroundColor: Colors.white,
      ),
      tabBarTheme: TabBarTheme(
        labelColor: AppColors.primary,
        unselectedLabelColor: textSecondary,
        indicatorColor: AppColors.primary,
        labelStyle: GoogleFonts.inter(fontWeight: FontWeight.w600),
        unselectedLabelStyle: GoogleFonts.inter(fontWeight: FontWeight.w400),
      ),
      switchTheme: SwitchThemeData(
        thumbColor: WidgetStateProperty.resolveWith((states) =>
            states.contains(WidgetState.selected) ? AppColors.primary : textHint),
        trackColor: WidgetStateProperty.resolveWith((states) =>
            states.contains(WidgetState.selected)
                ? AppColors.primary.withValues(alpha: 0.3)
                : divider),
      ),
    );
  }
}

extension ThemeExtension on BuildContext {
  bool get isDark => Theme.of(this).brightness == Brightness.dark;
  Color get cardColor => isDark ? AppColors.cardDark : AppColors.cardLight;
  Color get surfaceVariant => isDark ? AppColors.surfaceLightDark : AppColors.surfaceLightLight;
  Color get textPrimary => isDark ? AppColors.textPrimaryDark : AppColors.textPrimaryLight;
  Color get textSecondary => isDark ? AppColors.textSecondaryDark : AppColors.textSecondaryLight;
  Color get textHint => isDark ? AppColors.textHintDark : AppColors.textHintLight;
  Color get dividerColor => isDark ? AppColors.dividerDark : AppColors.dividerLight;
  Color get bg => isDark ? AppColors.backgroundDark : AppColors.backgroundLight;
}
