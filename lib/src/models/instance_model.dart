import 'package:uuid/uuid.dart';

class JavaParameters {
  JavaParameters({required this.heapSizeMb, required this.jvmArgs});

  final int heapSizeMb;
  final List<String> jvmArgs;

  factory JavaParameters.defaultFor(int ramMb) => JavaParameters(
        heapSizeMb: ramMb,
        jvmArgs: const ['-XX:+UseG1GC'],
      );

  factory JavaParameters.fromMap(Map<String, dynamic> map) => JavaParameters(
        heapSizeMb: map['heapSizeMb'] as int? ?? 2048,
        jvmArgs: List<String>.from(map['jvmArgs'] as List<dynamic>? ?? []),
      );

  Map<String, dynamic> toMap() => {
        'heapSizeMb': heapSizeMb,
        'jvmArgs': jvmArgs,
      };
}

enum InstanceLoader { vanilla, forge, fabric, quilt, neoforge }

class InstanceModel {
  InstanceModel({
    required this.id,
    required this.name,
    required this.version,
    required this.loader,
    required this.java,
    required this.modsDirectory,
  });

  final String id;
  final String name;
  final String version;
  final InstanceLoader loader;
  final JavaParameters java;
  final String modsDirectory;

  factory InstanceModel.create({
    required String name,
    required String version,
    InstanceLoader loader = InstanceLoader.vanilla,
    String? modsDirectory,
  }) {
    final id = const Uuid().v4();
    return InstanceModel(
      id: id,
      name: name,
      version: version,
      loader: loader,
      java: JavaParameters.defaultFor(4096),
      modsDirectory: modsDirectory ?? './instances/$id/mods',
    );
  }

  factory InstanceModel.fromMap(Map<String, dynamic> map) => InstanceModel(
        id: map['id'] as String,
        name: map['name'] as String,
        version: map['version'] as String,
        loader: InstanceLoader.values.firstWhere(
          (entry) => entry.name == (map['loader'] as String? ?? 'vanilla'),
          orElse: () => InstanceLoader.vanilla,
        ),
        java: JavaParameters.fromMap(map['java'] as Map<String, dynamic>? ?? {}),
        modsDirectory: map['modsDirectory'] as String? ?? './instances/default/mods',
      );

  Map<String, dynamic> toMap() => {
        'id': id,
        'name': name,
        'version': version,
        'loader': loader.name,
        'java': java.toMap(),
        'modsDirectory': modsDirectory,
      };
}
