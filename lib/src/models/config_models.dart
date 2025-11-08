import 'package:yaml/yaml.dart' as yaml;

/// Represents the loader families supported by Yuuki.
enum LoaderFamily { vanilla, forge, fabric, quilt, neoForge }

/// Java runtime descriptor.
class JavaRuntime {
  JavaRuntime({required this.path, required this.minimumVersion});

  final String path;
  final String minimumVersion;

  factory JavaRuntime.fromMap(Map<String, dynamic> map) {
    return JavaRuntime(
      path: map['path'] as String? ?? '',
      minimumVersion: map['minimumVersion'] as String? ?? '17',
    );
  }

  Map<String, dynamic> toMap() => {
        'path': path,
        'minimumVersion': minimumVersion,
      };
}

/// User-visible settings per profile.
class ProfileSettings {
  ProfileSettings({required this.ramMb, required this.javaFlags});

  final int ramMb;
  final List<String> javaFlags;

  factory ProfileSettings.fromMap(Map<String, dynamic> map) => ProfileSettings(
        ramMb: map['ramMb'] as int? ?? 4096,
        javaFlags: (map['javaFlags'] as List<dynamic>? ?? []).map((entry) => entry.toString()).toList(),
      );

  Map<String, dynamic> toMap() => {
        'ramMb': ramMb,
        'javaFlags': javaFlags,
      };
}

/// The surface representation for a profile configuration.
class ProfileModel {
  ProfileModel({
    required this.id,
    required this.name,
    required this.loader,
    required this.java,
    required this.settings,
  }) {
    assert(id.isNotEmpty);
    assert(name.isNotEmpty);
  }

  final String id;
  final String name;
  final LoaderFamily loader;
  final JavaRuntime java;
  final ProfileSettings settings;

  factory ProfileModel.fromYaml(String yamlString) {
    final parsed = _normalize(yaml.loadYaml(yamlString));
    if (parsed is! Map<String, dynamic>) {
      throw FormatException('Expected a YAML map for profile');
    }
    return ProfileModel.fromMap(parsed);
  }

  factory ProfileModel.fromMap(Map<String, dynamic> map) {
    return ProfileModel(
      id: map['id'] as String? ?? 'default',
      name: map['name'] as String? ?? 'Default',
      loader: _loaderFromString(map['loader'] as String? ?? 'vanilla'),
      java: JavaRuntime.fromMap(map['java'] as Map<String, dynamic>? ?? {}),
      settings: ProfileSettings.fromMap(map['settings'] as Map<String, dynamic>? ?? {}),
    );
  }

  static LoaderFamily _loaderFromString(String loader) {
    return LoaderFamily.values.firstWhere(
      (value) => value.name.toLowerCase() == loader.toLowerCase(),
      orElse: () => LoaderFamily.vanilla,
    );
  }

  Map<String, dynamic> toMap() => {
        'id': id,
        'name': name,
        'loader': loader.name,
        'java': java.toMap(),
        'settings': settings.toMap(),
      };

  String toYaml() => _mapToYaml(toMap());

  factory ProfileModel.defaultProfile() => ProfileModel(
        id: 'default',
        name: 'Default Profile',
        loader: LoaderFamily.vanilla,
        java: JavaRuntime(
          path: '/usr/lib/jvm/temurin/bin/java',
          minimumVersion: '17',
        ),
        settings: ProfileSettings(
          ramMb: 4096,
          javaFlags: ['-XX:+UseG1GC'],
        ),
      );
}

String _mapToYaml(Map<String, dynamic> map, [int indent = 0]) {
  final buffer = StringBuffer();
  final indentStr = '  ' * indent;
  for (final entry in map.entries) {
    final value = entry.value;
    if (value is Map<String, dynamic>) {
      buffer.writeln('$indentStr$entry.key:');
      buffer.write(_mapToYaml(value, indent + 1));
    } else if (value is Iterable) {
      buffer.writeln('$indentStr${entry.key}:');
      for (final item in value) {
        buffer.writeln('$indentStr  - ${_formatScalar(item)}');
      }
    } else {
      buffer.writeln('$indentStr$entry.key: ${_formatScalar(value)}');
    }
  }
  return buffer.toString();
}

dynamic _normalize(dynamic value) {
  if (value is yaml.YamlMap) {
    return {
      for (final entry in value.entries)
        entry.key.toString(): _normalize(entry.value),
    };
  }
  if (value is yaml.YamlList) {
    return value.map(_normalize).toList();
  }
  return value;
}

String _formatScalar(Object? value) => value?.toString() ?? '';
