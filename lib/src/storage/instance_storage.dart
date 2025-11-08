import 'dart:convert';
import 'dart:io';

import 'package:path/path.dart' as p;
import 'package:path_provider/path_provider.dart';

import '../models/instance_model.dart';

class InstanceStorage {
  static const _instancesFile = 'instances.json';

  Future<File> _localFile() async {
    final directory = await getApplicationSupportDirectory();
    final path = p.join(directory.path, _instancesFile);
    final file = File(path);
    await file.create(recursive: true);
    return file;
  }

  Future<List<InstanceModel>> loadInstances() async {
    final file = await _localFile();
    final contents = await file.readAsString();
    if (contents.isEmpty) return const [];
    final decoded = jsonDecode(contents);
    if (decoded is! List) return const [];
    return decoded
        .cast<Map<String, dynamic>>()
        .map(InstanceModel.fromMap)
        .toList();
  }

  Future<void> saveInstances(List<InstanceModel> instances) async {
    final file = await _localFile();
    final encoded = jsonEncode(instances.map((instance) => instance.toMap()).toList());
    await file.writeAsString(encoded, flush: true);
  }
}
