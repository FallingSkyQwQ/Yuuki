class JavaRuntimeModel {
  JavaRuntimeModel({
    required this.id,
    required this.path,
    required this.label,
    required this.version,
  });

  final String id;
  final String path;
  final String label;
  final String version;

  factory JavaRuntimeModel.defaultRuntime() => JavaRuntimeModel(
        id: 'default-runtime',
        path: '/usr/bin/java',
        label: 'Default Java',
        version: '17',
      );

  factory JavaRuntimeModel.fromMap(Map<String, dynamic> map) => JavaRuntimeModel(
        id: map['id'] as String,
        path: map['path'] as String,
        label: map['label'] as String,
        version: map['version'] as String,
      );

  Map<String, dynamic> toMap() => {
        'id': id,
        'path': path,
        'label': label,
        'version': version,
      };
}
