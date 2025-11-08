import 'dart:convert';
import 'dart:io';

import 'package:path/path.dart' as p;
import 'package:path_provider/path_provider.dart';

import '../models/java_runtime_model.dart';

class JavaRuntimeStorage {
  static const _fileName = 'java_runtimes.json';

  Future<File> _file() async {
    final dir = await getApplicationSupportDirectory();
    final path = p.join(dir.path, _fileName);
    final file = File(path);
    await file.create(recursive: true);
    return file;
  }

  Future<List<JavaRuntimeModel>> load() async {
    final file = await _file();
    final contents = await file.readAsString();
    if (contents.isEmpty) return [JavaRuntimeModel.defaultRuntime()];
    final decoded = jsonDecode(contents);
    if (decoded is! List) return [JavaRuntimeModel.defaultRuntime()];
    return decoded.cast<Map<String, dynamic>>().map(JavaRuntimeModel.fromMap).toList();
  }

  Future<void> save(List<JavaRuntimeModel> runtimes) async {
    final file = await _file();
    final encoded = jsonEncode(runtimes.map((runtime) => runtime.toMap()).toList());
    await file.writeAsString(encoded, flush: true);
  }
}
