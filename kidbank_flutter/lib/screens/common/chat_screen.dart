import 'dart:async';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:intl/intl.dart';
import '../../core/api_service.dart';
import '../../core/theme.dart';
import '../../providers/auth_provider.dart';

class ChatScreen extends StatefulWidget {
  const ChatScreen({super.key});
  @override
  State<ChatScreen> createState() => _ChatScreenState();
}

class _ChatScreenState extends State<ChatScreen> {
  List<dynamic> _messages = [];
  bool _loading = true;
  final _msgCtrl = TextEditingController();
  final _scrollCtrl = ScrollController();
  Timer? _pollTimer;

  @override
  void initState() {
    super.initState();
    _load();
    _pollTimer = Timer.periodic(const Duration(seconds: 5), (_) => _poll());
  }

  Future<void> _poll() async {
    try {
      final data = await context.read<ApiService>().get('chat/history?pageNumber=1&pageSize=100');
      final items = data is List ? data : (data['items'] ?? []) as List;
      if (!mounted) return;
      final newList = items is List ? List<dynamic>.from(items) : <dynamic>[];
      if (newList.length != _messages.length) {
        setState(() { _messages = newList; });
        _scrollToBottom();
      }
    } catch (_) {}
  }

  Future<void> _load() async {
    try {
      final data = await context.read<ApiService>().get('chat/history?pageNumber=1&pageSize=100');
      final items = data is List ? data : (data['items'] ?? []) as List;
      setState(() { _messages = List<dynamic>.from(items); _loading = false; });
      _scrollToBottom();
    } catch (_) { setState(() => _loading = false); }
  }

  void _scrollToBottom() {
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (_scrollCtrl.hasClients) _scrollCtrl.animateTo(_scrollCtrl.position.maxScrollExtent, duration: const Duration(milliseconds: 300), curve: Curves.easeOut);
    });
  }

  Future<void> _send() async {
    final text = _msgCtrl.text.trim();
    if (text.isEmpty) return;
    _msgCtrl.clear();
    try {
      await context.read<ApiService>().post('chat/send', body: {'content': text});
      _load();
    } catch (e) {
      if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(e.toString())));
    }
  }

  Future<void> _sendMoney() async {
    List<dynamic> kids;
    try {
      kids = await context.read<ApiService>().get('families/kids') as List;
    } catch (_) { return; }
    if (!mounted || kids.isEmpty) return;

    final amountCtrl = TextEditingController();
    String? selectedKidId = kids.length == 1 ? kids.first['id'] : null;
    String? selectedKidName = kids.length == 1 ? kids.first['firstName'] : null;

    final confirmed = await showDialog<bool>(
      context: context,
      builder: (ctx) => StatefulBuilder(builder: (ctx, setDS) => AlertDialog(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
        title: const Text('Отправить деньги'),
        content: Column(mainAxisSize: MainAxisSize.min, children: [
          if (kids.length > 1) DropdownButtonFormField<String>(
            decoration: const InputDecoration(labelText: 'Кому', prefixIcon: Icon(Icons.person)),
            items: kids.map<DropdownMenuItem<String>>((k) => DropdownMenuItem(value: k['id'], child: Text('${k['firstName']} ${k['lastName']}'))).toList(),
            onChanged: (v) => setDS(() { selectedKidId = v; selectedKidName = kids.firstWhere((k) => k['id'] == v)['firstName']; }),
          ),
          if (kids.length > 1) const SizedBox(height: 12),
          TextField(controller: amountCtrl, decoration: const InputDecoration(labelText: 'Сумма (BYN)', prefixIcon: Icon(Icons.attach_money)), keyboardType: TextInputType.number),
        ]),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx, false), child: const Text('Отмена')),
          ElevatedButton(onPressed: selectedKidId != null ? () => Navigator.pop(ctx, true) : null, child: const Text('Отправить')),
        ],
      )),
    );
    if (confirmed == true && selectedKidId != null) {
      try {
        final amount = double.tryParse(amountCtrl.text) ?? 0;
        await context.read<ApiService>().post('accounts/deposit', body: {
          'kidId': selectedKidId,
          'amount': amount,
          'currency': 'BYN',
        });
        await context.read<ApiService>().post('chat/send', body: {'content': '💸 Отправлено $amount BYN для $selectedKidName'});
        _load();
      } catch (e) {
        if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(e.toString())));
      }
    }
  }

  Future<void> _requestMoney() async {
    final amountCtrl = TextEditingController();
    final noteCtrl = TextEditingController();
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
        title: Row(children: [
          Container(
            width: 40, height: 40,
            decoration: BoxDecoration(color: AppColors.accent.withValues(alpha: 0.15), borderRadius: BorderRadius.circular(10)),
            child: const Icon(Icons.request_page, color: AppColors.accent, size: 20),
          ),
          const SizedBox(width: 12),
          const Text('Запросить деньги'),
        ]),
        content: Column(mainAxisSize: MainAxisSize.min, children: [
          TextField(controller: amountCtrl, decoration: const InputDecoration(labelText: 'Сумма (BYN)', prefixIcon: Icon(Icons.attach_money)), keyboardType: TextInputType.number),
          const SizedBox(height: 12),
          TextField(controller: noteCtrl, decoration: const InputDecoration(labelText: 'На что? (необязательно)', prefixIcon: Icon(Icons.note)), maxLines: 2),
        ]),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx, false), child: const Text('Отмена')),
          ElevatedButton(onPressed: () => Navigator.pop(ctx, true), child: const Text('Запросить')),
        ],
      ),
    );
    if (confirmed == true) {
      try {
        final amount = double.tryParse(amountCtrl.text) ?? 0;
        final note = noteCtrl.text.trim();
        await context.read<ApiService>().post('moneyrequests', body: {
          'amount': amount,
          'currency': 'BYN',
          'note': note.isNotEmpty ? note : 'Запрос из чата',
        });
        final noteText = note.isNotEmpty ? ' на $note' : '';
        await context.read<ApiService>().post('chat/send', body: {'content': '🙏 Запрос: $amount BYN$noteText'});
        _load();
        if (mounted) ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Запрос отправлен!')));
      } catch (e) {
        if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(e.toString())));
      }
    }
  }

  @override
  void dispose() { _pollTimer?.cancel(); _msgCtrl.dispose(); _scrollCtrl.dispose(); super.dispose(); }

  @override
  Widget build(BuildContext context) {
    final myId = context.read<AuthProvider>().userId;
    final isParent = context.read<AuthProvider>().isParent;
    return Scaffold(
      appBar: AppBar(
        title: const Text('Семейный чат'),
        actions: [
          if (isParent) IconButton(
            icon: const Icon(Icons.attach_money),
            tooltip: 'Отправить деньги',
            onPressed: _sendMoney,
          ),
          if (!isParent) IconButton(
            icon: const Icon(Icons.request_page),
            tooltip: 'Запросить деньги',
            onPressed: _requestMoney,
          ),
        ],
      ),
      body: Column(children: [
        Expanded(
          child: _loading
              ? const Center(child: CircularProgressIndicator())
              : _messages.isEmpty
                  ? Center(child: Column(mainAxisSize: MainAxisSize.min, children: [
                      Icon(Icons.chat_bubble_outline, size: 64, color: context.textHint),
                      const SizedBox(height: 16),
                      Text('Нет сообщений', style: TextStyle(color: context.textSecondary)),
                      const SizedBox(height: 8),
                      Text('Начните общение!', style: TextStyle(color: context.textHint)),
                    ]))
                  : ListView.builder(
                      controller: _scrollCtrl,
                      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                      itemCount: _messages.length,
                      itemBuilder: (_, i) => _buildMessage(_messages[i], myId),
                    ),
        ),
        Container(
          padding: const EdgeInsets.fromLTRB(16, 8, 8, 8),
          decoration: BoxDecoration(
            color: Theme.of(context).colorScheme.surface,
            boxShadow: [BoxShadow(color: Colors.black.withValues(alpha: 0.1), blurRadius: 10, offset: const Offset(0, -2))],
          ),
          child: SafeArea(
            child: Row(children: [
              Expanded(child: TextField(
                controller: _msgCtrl,
                decoration: InputDecoration(
                  hintText: 'Сообщение...',
                  border: OutlineInputBorder(borderRadius: BorderRadius.circular(24), borderSide: BorderSide.none),
                  filled: true,
                  fillColor: context.surfaceVariant,
                  contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
                ),
                textInputAction: TextInputAction.send,
                onSubmitted: (_) => _send(),
              )),
              const SizedBox(width: 8),
              Container(
                decoration: const BoxDecoration(shape: BoxShape.circle, color: AppColors.primary),
                child: IconButton(icon: const Icon(Icons.send, color: Colors.white, size: 20), onPressed: _send),
              ),
            ]),
          ),
        ),
      ]),
    );
  }

  Widget _buildMessage(dynamic msg, String? myId) {
    final isMine = msg['senderId'] == myId;
    final name = msg['senderName'] ?? '';
    final content = msg['content'] ?? '';
    final rawTime = msg['createdAt'];
    final time = rawTime != null ? DateFormat('HH:mm').format(DateTime.parse(rawTime.toString()).toLocal()) : '';
    final isMoneyMsg = content.startsWith('💸') || content.startsWith('🙏');

    return Padding(
      padding: const EdgeInsets.only(bottom: 8),
      child: Row(
        mainAxisAlignment: isMine ? MainAxisAlignment.end : MainAxisAlignment.start,
        crossAxisAlignment: CrossAxisAlignment.end,
        children: [
          if (!isMine) ...[
            CircleAvatar(
              radius: 16,
              backgroundColor: AppColors.primary.withValues(alpha: 0.15),
              child: Text(name.isNotEmpty ? name[0] : '?', style: const TextStyle(fontSize: 12, color: AppColors.primary, fontWeight: FontWeight.bold)),
            ),
            const SizedBox(width: 8),
          ],
          Flexible(
            child: Container(
              padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 10),
              decoration: BoxDecoration(
                color: isMine
                    ? AppColors.primary.withValues(alpha: 0.85)
                    : isMoneyMsg
                        ? AppColors.accent.withValues(alpha: 0.15)
                        : context.surfaceVariant,
                borderRadius: BorderRadius.only(
                  topLeft: const Radius.circular(16),
                  topRight: const Radius.circular(16),
                  bottomLeft: Radius.circular(isMine ? 16 : 4),
                  bottomRight: Radius.circular(isMine ? 4 : 16),
                ),
              ),
              child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                if (!isMine) Text(name, style: TextStyle(fontSize: 11, fontWeight: FontWeight.w600, color: isMine ? Colors.white70 : AppColors.primary)),
                Text(content, style: TextStyle(color: isMine ? Colors.white : context.textPrimary)),
                const SizedBox(height: 2),
                Text(time, style: TextStyle(fontSize: 10, color: isMine ? Colors.white60 : context.textHint)),
              ]),
            ),
          ),
        ],
      ),
    );
  }
}
