import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:percent_indicator/linear_percent_indicator.dart';
import '../../core/api_service.dart';
import '../../core/theme.dart';
import '../common/subscription_screen.dart';

class KidEducationScreen extends StatefulWidget {
  const KidEducationScreen({super.key});
  @override
  State<KidEducationScreen> createState() => _KidEducationScreenState();
}

class _KidEducationScreenState extends State<KidEducationScreen> with SingleTickerProviderStateMixin {
  late TabController _tab;
  List<dynamic>? _missions;
  List<dynamic>? _achievements;
  Map<String, dynamic>? _progress;
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    _tab = TabController(length: 3, vsync: this);
    _load();
  }

  Future<void> _load() async {
    final api = context.read<ApiService>();
    try {
      final results = await Future.wait([
        api.get('education/missions'),
        api.get('achievements/my').catchError((_) => <dynamic>[]),
        api.get('education/progress').catchError((_) => <String, dynamic>{}),
      ]);
      setState(() {
        _missions = results[0] is List ? results[0] as List : [];
        _achievements = results[1] is List ? results[1] as List : [];
        _progress = results[2] is Map ? results[2] as Map<String, dynamic> : {};
        _loading = false;
      });
    } catch (_) { setState(() => _loading = false); }
  }

  @override
  void dispose() { _tab.dispose(); super.dispose(); }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Обучение'),
        bottom: TabBar(controller: _tab, tabs: const [
          Tab(text: 'Квизы'),
          Tab(text: 'Прогресс'),
          Tab(text: 'Достижения'),
        ]),
      ),
      body: _loading ? const Center(child: CircularProgressIndicator()) : TabBarView(
        controller: _tab,
        children: [_buildMissions(), _buildProgress(), _buildAchievements()],
      ),
    );
  }

  Widget _buildMissions() {
    if (_missions == null || _missions!.isEmpty) {
      return Center(child: Column(mainAxisSize: MainAxisSize.min, children: [
        Icon(Icons.school, size: 64, color: context.textHint),
        const SizedBox(height: 16),
        Text('Нет доступных квизов', style: TextStyle(color: context.textSecondary)),
      ]));
    }
    return RefreshIndicator(
      onRefresh: _load,
      child: ListView.builder(
        padding: const EdgeInsets.all(16),
        itemCount: _missions!.length,
        itemBuilder: (_, i) {
          final m = _missions![i];
          final isTrial = i < 2;
          final isLocked = !isTrial; // without sub, only first 2 are accessible
          return TweenAnimationBuilder<double>(
            tween: Tween(begin: 0, end: 1),
            duration: Duration(milliseconds: 300 + i * 80),
            curve: Curves.easeOutCubic,
            builder: (_, val, child) => Opacity(opacity: val, child: Transform.translate(offset: Offset(0, 20 * (1 - val)), child: child)),
            child: Card(
              margin: const EdgeInsets.only(bottom: 12),
              child: InkWell(
                onTap: isLocked ? () => _showLockedDialog() : () => _openMission(m),
                borderRadius: BorderRadius.circular(16),
                child: Padding(
                  padding: const EdgeInsets.all(16),
                  child: Row(children: [
                    Container(
                      width: 52, height: 52,
                      decoration: BoxDecoration(
                        color: (isLocked ? context.textHint : AppColors.primary).withValues(alpha: 0.15),
                        borderRadius: BorderRadius.circular(14),
                      ),
                      child: Icon(
                        isLocked ? Icons.lock : Icons.school,
                        color: isLocked ? context.textHint : AppColors.primary,
                        size: 24,
                      ),
                    ),
                    const SizedBox(width: 16),
                    Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                      Row(children: [
                        Expanded(child: Text(m['title'] ?? '', style: TextStyle(fontWeight: FontWeight.w600, fontSize: 16, color: isLocked ? context.textHint : context.textPrimary))),
                        if (isTrial) Container(
                          padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
                          decoration: BoxDecoration(color: AppColors.success.withValues(alpha: 0.15), borderRadius: BorderRadius.circular(8)),
                          child: const Text('Бесплатно', style: TextStyle(color: AppColors.success, fontSize: 10, fontWeight: FontWeight.w600)),
                        ),
                        if (isLocked) Container(
                          padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
                          decoration: BoxDecoration(color: AppColors.accent.withValues(alpha: 0.15), borderRadius: BorderRadius.circular(8)),
                          child: const Text('Pro', style: TextStyle(color: AppColors.accent, fontSize: 10, fontWeight: FontWeight.w600)),
                        ),
                      ]),
                      const SizedBox(height: 4),
                      Text(m['description'] ?? '', maxLines: 2, overflow: TextOverflow.ellipsis, style: TextStyle(fontSize: 13, color: context.textSecondary)),
                    ])),
                    const SizedBox(width: 8),
                    Icon(Icons.chevron_right, color: isLocked ? context.textHint : context.textSecondary),
                  ]),
                ),
              ),
            ),
          );
        },
      ),
    );
  }

  Widget _buildProgress() {
    final xp = _progress?['totalXpEarned'] ?? 0;
    final modules = _progress?['modules'] as List? ?? [];
    int quizzes = 0;
    int total = 0;
    for (final m in modules) {
      quizzes += (m['completedQuizzes'] as num?)?.toInt() ?? 0;
      total += (m['totalQuizzes'] as num?)?.toInt() ?? 0;
    }
    final streak = _progress?['currentStreak'] ?? 0;

    return SingleChildScrollView(
      padding: const EdgeInsets.all(16),
      child: Column(children: [
        Card(child: Padding(
          padding: const EdgeInsets.all(20),
          child: Column(children: [
            Row(mainAxisAlignment: MainAxisAlignment.spaceEvenly, children: [
              _ProgressStat(Icons.bolt, '$xp', 'XP', AppColors.primary),
              _ProgressStat(Icons.quiz, '$quizzes/$total', 'Квизы', AppColors.accent),
              _ProgressStat(Icons.local_fire_department, '$streak', 'Стрик', AppColors.error),
            ]),
            if (total > 0) ...[
              const SizedBox(height: 20),
              LinearPercentIndicator(
                lineHeight: 10,
                percent: total > 0 ? (quizzes / total).clamp(0.0, 1.0) : 0,
                progressColor: AppColors.primary,
                backgroundColor: context.dividerColor,
                barRadius: const Radius.circular(5),
                animation: true,
                animationDuration: 800,
              ),
              const SizedBox(height: 8),
              Text('$quizzes из $total квизов пройдено', style: TextStyle(color: context.textSecondary, fontSize: 12)),
            ],
          ]),
        )),
        const SizedBox(height: 16),
        Card(child: Padding(
          padding: const EdgeInsets.all(16),
          child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
            Text('Как заработать XP', style: TextStyle(fontSize: 16, fontWeight: FontWeight.w600, color: context.textPrimary)),
            const SizedBox(height: 12),
            _XpRow(Icons.quiz, 'Пройти квиз', '+20 XP'),
            _XpRow(Icons.local_fire_department, 'Ежедневный стрик', '+5 XP'),
            _XpRow(Icons.assignment, 'Выполнить задание', '+10 XP'),
            _XpRow(Icons.flag, 'Достичь цели', '+50 XP'),
          ]),
        )),
      ]),
    );
  }

  Widget _buildAchievements() {
    if (_achievements == null || _achievements!.isEmpty) {
      return Center(child: Column(mainAxisSize: MainAxisSize.min, children: [
        Icon(Icons.emoji_events, size: 64, color: context.textHint),
        const SizedBox(height: 16),
        Text('Нет достижений', style: TextStyle(color: context.textSecondary)),
      ]));
    }
    return GridView.builder(
      padding: const EdgeInsets.all(16),
      gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(crossAxisCount: 3, mainAxisSpacing: 12, crossAxisSpacing: 12, childAspectRatio: 0.85),
      itemCount: _achievements!.length,
      itemBuilder: (_, i) {
        final a = _achievements![i];
        final unlocked = a['unlockedAt'] != null;
        final progress = (a['progress'] as num?)?.toInt() ?? 0;
        final required = (a['requiredProgress'] as num?)?.toInt() ?? 1;
        return TweenAnimationBuilder<double>(
          tween: Tween(begin: 0, end: 1),
          duration: Duration(milliseconds: 200 + i * 60),
          curve: Curves.easeOutCubic,
          builder: (_, val, child) => Opacity(opacity: val, child: Transform.scale(scale: 0.8 + 0.2 * val, child: child)),
          child: Card(
            child: Padding(
              padding: const EdgeInsets.all(12),
              child: Column(mainAxisAlignment: MainAxisAlignment.center, children: [
                Container(
                  width: 48, height: 48,
                  decoration: BoxDecoration(
                    color: (unlocked ? AppColors.accent : context.textHint).withValues(alpha: unlocked ? 0.2 : 0.1),
                    borderRadius: BorderRadius.circular(14),
                  ),
                  child: Icon(Icons.emoji_events, color: unlocked ? AppColors.accent : context.textHint, size: 24),
                ),
                const SizedBox(height: 8),
                Text(a['title'] ?? '', style: TextStyle(fontSize: 11, fontWeight: FontWeight.w600, color: unlocked ? context.textPrimary : context.textHint), maxLines: 2, overflow: TextOverflow.ellipsis, textAlign: TextAlign.center),
                const SizedBox(height: 4),
                if (!unlocked) Text('$progress/$required', style: TextStyle(fontSize: 10, color: context.textHint)),
                if (unlocked) const Text('Получено!', style: TextStyle(fontSize: 10, color: AppColors.success, fontWeight: FontWeight.w600)),
              ]),
            ),
          ),
        );
      },
    );
  }

  void _showLockedDialog() {
    showDialog(context: context, builder: (ctx) => AlertDialog(
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
      title: Row(children: [
        const Icon(Icons.lock, color: AppColors.accent),
        const SizedBox(width: 8),
        const Text('Требуется Pro'),
      ]),
      content: const Text('Этот квиз доступен только с подпиской KidBank Pro.\n\nОформите подписку, чтобы получить доступ ко всем образовательным материалам!'),
      actions: [
        TextButton(onPressed: () => Navigator.pop(ctx), child: const Text('Закрыть')),
        ElevatedButton(
          onPressed: () { Navigator.pop(ctx); Navigator.push(context, MaterialPageRoute(builder: (_) => const SubscriptionScreen())); },
          child: const Text('Оформить Pro'),
        ),
      ],
    ));
  }

  void _openMission(dynamic m) {
    Navigator.push(context, MaterialPageRoute(builder: (_) => _MissionPage(mission: m)));
  }
}

class _ProgressStat extends StatelessWidget {
  final IconData icon;
  final String value, label;
  final Color color;
  const _ProgressStat(this.icon, this.value, this.label, this.color);
  @override
  Widget build(BuildContext context) {
    return Column(children: [
      Container(
        width: 52, height: 52,
        decoration: BoxDecoration(color: color.withValues(alpha: 0.15), borderRadius: BorderRadius.circular(14)),
        child: Icon(icon, color: color, size: 24),
      ),
      const SizedBox(height: 8),
      Text(value, style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold, color: context.textPrimary)),
      Text(label, style: TextStyle(fontSize: 12, color: context.textSecondary)),
    ]);
  }
}

class _XpRow extends StatelessWidget {
  final IconData icon;
  final String text, xp;
  const _XpRow(this.icon, this.text, this.xp);
  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 4),
      child: Row(children: [
        Icon(icon, size: 20, color: AppColors.primary),
        const SizedBox(width: 8),
        Expanded(child: Text(text, style: TextStyle(color: context.textSecondary))),
        Text(xp, style: const TextStyle(fontWeight: FontWeight.w600, color: AppColors.primary)),
      ]),
    );
  }
}

class _MissionPage extends StatefulWidget {
  final dynamic mission;
  const _MissionPage({required this.mission});
  @override
  State<_MissionPage> createState() => _MissionPageState();
}

class _MissionPageState extends State<_MissionPage> {
  Map<String, dynamic>? _detail;
  bool _loading = true;

  @override
  void initState() { super.initState(); _load(); }

  Future<void> _load() async {
    try {
      final d = await context.read<ApiService>().get('education/missions/${widget.mission['id']}');
      setState(() { _detail = d; _loading = false; });
    } catch (_) { setState(() => _loading = false); }
  }

  void _openModule(dynamic moduleId) {
    Navigator.push(context, MaterialPageRoute(builder: (_) => _ModuleQuizPage(moduleId: moduleId.toString())));
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text(widget.mission['title'] ?? 'Квиз')),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : _detail == null
              ? Center(child: Text('Ошибка загрузки', style: TextStyle(color: context.textSecondary)))
              : SingleChildScrollView(
                  padding: const EdgeInsets.all(16),
                  child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                    if (_detail!['description'] != null) ...[
                      Text(_detail!['description'], style: TextStyle(color: context.textSecondary)),
                      const SizedBox(height: 20),
                    ],
                    if (_detail!['modules'] != null) ...(_detail!['modules'] as List).map((m) => Card(
                      margin: const EdgeInsets.only(bottom: 12),
                      child: Padding(
                        padding: const EdgeInsets.all(16),
                        child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                          Text(m['title'] ?? '', style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 16)),
                          const SizedBox(height: 4),
                          Text(m['description'] ?? '', style: TextStyle(color: context.textSecondary)),
                          const SizedBox(height: 8),
                          Row(children: [
                            Icon(Icons.quiz, size: 16, color: AppColors.primary),
                            const SizedBox(width: 4),
                            Text('${m['quizzesCompleted'] ?? 0}/${m['quizzesTotal'] ?? 0} квизов', style: TextStyle(fontSize: 12, color: context.textSecondary)),
                            const Spacer(),
                            if (m['isCompleted'] == true) const Text('Пройден', style: TextStyle(color: AppColors.success, fontSize: 12, fontWeight: FontWeight.w600))
                            else ElevatedButton(
                              onPressed: () => _openModule(m['id']),
                              style: ElevatedButton.styleFrom(padding: const EdgeInsets.symmetric(horizontal: 12)),
                              child: const Text('Пройти', style: TextStyle(fontSize: 12)),
                            ),
                          ]),
                        ]),
                      ),
                    )),
                  ]),
                ),
    );
  }
}

class _ModuleQuizPage extends StatefulWidget {
  final String moduleId;
  const _ModuleQuizPage({required this.moduleId});
  @override
  State<_ModuleQuizPage> createState() => _ModuleQuizPageState();
}

class _ModuleQuizPageState extends State<_ModuleQuizPage> {
  Map<String, dynamic>? _lesson;
  bool _loading = true;
  final Map<String, int> _answers = {};
  final Map<String, Map<String, dynamic>> _results = {};

  @override
  void initState() { super.initState(); _load(); }

  Future<void> _load() async {
    try {
      final d = await context.read<ApiService>().get('education/lessons/${widget.moduleId}');
      setState(() { _lesson = d; _loading = false; });
    } catch (_) { setState(() => _loading = false); }
  }

  Future<void> _submitQuiz(String quizId, int selectedOption) async {
    try {
      final result = await context.read<ApiService>().post('education/quiz/submit', body: {
        'quizId': quizId,
        'selectedOptionIndex': selectedOption,
      });
      setState(() {
        _results[quizId] = result is Map<String, dynamic> ? result : {};
      });
      if (mounted) {
        final correct = result is Map && result['isCorrect'] == true;
        final xpEarned = result is Map ? result['xpEarned'] ?? 0 : 0;
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(
          content: Text(correct ? 'Верно! +$xpEarned XP' : 'Неверно. ${result is Map ? result['explanation'] ?? '' : ''}'),
          backgroundColor: correct ? AppColors.success : AppColors.error,
        ));
      }
    } catch (e) {
      if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(e.toString())));
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text(_lesson?['title'] ?? 'Модуль')),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : _lesson == null
              ? Center(child: Text('Ошибка загрузки', style: TextStyle(color: context.textSecondary)))
              : SingleChildScrollView(
                  padding: const EdgeInsets.all(16),
                  child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                    if (_lesson!['content'] != null) ...[
                      Card(child: Padding(
                        padding: const EdgeInsets.all(16),
                        child: Text(_lesson!['content'], style: TextStyle(color: context.textPrimary, fontSize: 15, height: 1.5)),
                      )),
                      const SizedBox(height: 20),
                    ],
                    if (_lesson!['quizzes'] != null) ...(_lesson!['quizzes'] as List).asMap().entries.map((e) {
                      final q = e.value;
                      final qId = q['id']?.toString() ?? '';
                      final answered = _results.containsKey(qId);
                      final correct = answered && _results[qId]?['isCorrect'] == true;
                      return Card(
                        margin: const EdgeInsets.only(bottom: 12),
                        color: answered ? (correct ? AppColors.success.withValues(alpha: 0.05) : AppColors.error.withValues(alpha: 0.05)) : null,
                        child: Padding(
                          padding: const EdgeInsets.all(16),
                          child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                            Text('Вопрос ${e.key + 1}', style: TextStyle(fontSize: 12, color: context.textSecondary)),
                            const SizedBox(height: 4),
                            Text(q['question'] ?? '', style: const TextStyle(fontWeight: FontWeight.w600)),
                            const SizedBox(height: 8),
                            if (q['options'] != null) ...(q['options'] as List).asMap().entries.map((o) {
                              final optIdx = o.key;
                              final isCorrectOpt = answered && _results[qId]?['correctOptionIndex'] == optIdx;
                              return RadioListTile<int>(
                                value: optIdx,
                                groupValue: _answers[qId],
                                onChanged: answered ? null : (v) => setState(() => _answers[qId] = v!),
                                title: Text(
                                  o.value.toString(),
                                  style: TextStyle(color: isCorrectOpt ? AppColors.success : null, fontWeight: isCorrectOpt ? FontWeight.bold : null),
                                ),
                                activeColor: AppColors.primary,
                                dense: true,
                              );
                            }),
                            if (!answered && _answers.containsKey(qId))
                              Align(
                                alignment: Alignment.centerRight,
                                child: ElevatedButton(
                                  onPressed: () => _submitQuiz(qId, _answers[qId]!),
                                  child: const Text('Ответить'),
                                ),
                              ),
                            if (answered) ...[
                              const SizedBox(height: 8),
                              Text(
                                correct ? 'Правильно!' : 'Неправильно. ${_results[qId]?['explanation'] ?? ''}',
                                style: TextStyle(color: correct ? AppColors.success : AppColors.error, fontSize: 13, fontWeight: FontWeight.w500),
                              ),
                            ],
                          ]),
                        ),
                      );
                    }),
                  ]),
                ),
    );
  }
}
