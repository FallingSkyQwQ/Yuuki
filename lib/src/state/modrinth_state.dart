import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../models/modrinth_model.dart';
import '../services/modrinth_service.dart';

class ModrinthSearchNotifier extends StateNotifier<AsyncValue<List<ModrinthProject>>> {
  ModrinthSearchNotifier(this._service) : super(const AsyncValue.data([]));

  final ModrinthService _service;

  Future<void> search(String query, {List<String>? loaders}) async {
    if (query.isEmpty) {
      state = const AsyncValue.data([]);
      return;
    }
    state = const AsyncValue.loading();
    try {
      final results = await _service.search(query, loaders: loaders);
      state = AsyncValue.data(results);
    } catch (error, stack) {
      state = AsyncValue.error(error, stack);
    }
  }
}

final modrinthServiceProvider = Provider<ModrinthService>((_) => ModrinthService());
final modrinthSearchProvider = StateNotifierProvider<ModrinthSearchNotifier, AsyncValue<List<ModrinthProject>>>(
  (ref) => ModrinthSearchNotifier(ref.watch(modrinthServiceProvider)),
);
