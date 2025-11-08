import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../bridge/yuuki_bridge.dart';

final launcherStateProvider = StateNotifierProvider<LauncherStateNotifier, LauncherState>(
  (ref) => LauncherStateNotifier(),
);

final launcherTitleProvider = Provider<String>((_) => 'Yuuki Launcher');

@immutable
class LauncherState {
  final String statusMessage;
  final bool ready;
  final DateTime lastUpdated;

  const LauncherState({
    required this.statusMessage,
    required this.ready,
    required this.lastUpdated,
  });

  factory LauncherState.initial() => LauncherState(
        statusMessage: 'Awaiting launch',
        ready: false,
        lastUpdated: DateTime.now(),
      );

  LauncherState copyWith({String? statusMessage, bool? ready, DateTime? lastUpdated}) {
    return LauncherState(
      statusMessage: statusMessage ?? this.statusMessage,
      ready: ready ?? this.ready,
      lastUpdated: lastUpdated ?? this.lastUpdated,
    );
  }
}

class LauncherStateNotifier extends StateNotifier<LauncherState> {
  LauncherStateNotifier() : super(LauncherState.initial());

  Future<void> refreshStatus() async {
    state = state.copyWith(statusMessage: 'Querying core...', lastUpdated: DateTime.now());
    final message = await YuukiBridge.ping();
    state = state.copyWith(
      statusMessage: message,
      ready: message.contains('alive'),
      lastUpdated: DateTime.now(),
    );
  }

  Future<void> initializeCore() async {
    state = state.copyWith(statusMessage: 'Initializing core...');
    final ready = await YuukiBridge.initializeCore();
    state = state.copyWith(
      statusMessage: ready ? 'Core is warmed up' : 'Core initialization failed',
      ready: ready,
      lastUpdated: DateTime.now(),
    );
  }
}
