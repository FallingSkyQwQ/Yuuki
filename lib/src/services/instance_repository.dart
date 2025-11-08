import '../models/instance_model.dart';
import '../storage/instance_storage.dart';

class InstanceRepository {
  InstanceRepository({InstanceStorage? storage}) : _storage = storage ?? InstanceStorage();

  final InstanceStorage _storage;

  Future<List<InstanceModel>> loadInstances() => _storage.loadInstances();

  Future<InstanceModel> createInstance(InstanceModel instance) async {
    final instances = await loadInstances();
    instances.add(instance);
    await _storage.saveInstances(instances);
    return instance;
  }
}
