import '../models/java_runtime_model.dart';
import '../storage/java_runtime_storage.dart';

class JavaRuntimeRepository {
  JavaRuntimeRepository({JavaRuntimeStorage? storage}) : _storage = storage ?? JavaRuntimeStorage();

  final JavaRuntimeStorage _storage;

  Future<List<JavaRuntimeModel>> loadRuntimes() => _storage.load();

  Future<void> saveRuntimes(List<JavaRuntimeModel> runtimes) => _storage.save(runtimes);
}
