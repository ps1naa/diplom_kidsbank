import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:kidbank_flutter/core/theme.dart';

void main() {
  TestWidgetsFlutterBinding.ensureInitialized();

  test('AppTheme provides light and dark themes', () {
    expect(AppTheme.lightTheme.brightness, Brightness.light);
    expect(AppTheme.darkTheme.brightness, Brightness.dark);
  });
}
