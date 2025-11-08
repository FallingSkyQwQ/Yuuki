import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../models/instance_model.dart';
import '../services/instance_repository.dart';

class InstanceManager extends StateNotifier<List<InstanceModel>> {
  InstanceManager(this._repository) : super(const []) {
    _load();
  }

  final InstanceRepository _repository;

  Future<void> _load() async {
    final data = await _repository.loadInstances();
    state = data;
  }

  Future<void> addInstance(InstanceModel instance) async {
    await _repository.createInstance(instance);
    await _load();
  }
}

final instanceRepositoryProvider = Provider<InstanceRepository>((_) => InstanceRepository());
final instanceManagerProvider = StateNotifierProvider<InstanceManager, List<InstanceModel>>(
  (ref) => InstanceManager(ref.watch(instanceRepositoryProvider)),
);
