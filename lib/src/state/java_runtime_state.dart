import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../models/java_runtime_model.dart';
import '../services/java_runtime_repository.dart';

class JavaRuntimeNotifier extends StateNotifier<List<JavaRuntimeModel>> {
  JavaRuntimeNotifier(this._repository) : super(const []) {
    _load();
  }

  final JavaRuntimeRepository _repository;

  Future<void> _load() async {
    state = await _repository.loadRuntimes();
  }

  Future<void> addRuntime(JavaRuntimeModel runtime) async {
    final next = [...state, runtime];
    await _repository.saveRuntimes(next);
    state = next;
  }
}

final javaRuntimeRepositoryProvider = Provider<JavaRuntimeRepository>((_) => JavaRuntimeRepository());
final javaRuntimeProvider = StateNotifierProvider<JavaRuntimeNotifier, List<JavaRuntimeModel>>(
  (ref) => JavaRuntimeNotifier(ref.watch(javaRuntimeRepositoryProvider)),
);
