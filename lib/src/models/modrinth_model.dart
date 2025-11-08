class ModrinthProject {
  ModrinthProject({
    required this.id,
    required this.slug,
    required this.title,
    required this.description,
    required this.downloads,
    required this.icons,
    required this.categories,
  });

  final String id;
  final String slug;
  final String title;
  final String description;
  final int downloads;
  final Map<String, String> icons;
  final List<String> categories;

  factory ModrinthProject.fromMap(Map<String, dynamic> map) => ModrinthProject(
        id: map['id'] as String,
        slug: map['slug'] as String,
        title: map['title'] as String,
        description: map['description'] as String? ?? '',
        downloads: map['downloads'] as int? ?? 0,
        icons: (map['icon_url'] as String?) != null
            ? {'default': map['icon_url'] as String}
            : <String, String>{},
        categories: (map['categories'] as List<dynamic>? ?? []).cast<String>(),
      );
}

class ModrinthVersion {
  ModrinthVersion({
    required this.id,
    required this.name,
    required this.versionNumber,
    required this.loaders,
    required this.changelog,
  });

  final String id;
  final String name;
  final String versionNumber;
  final List<String> loaders;
  final String changelog;

  factory ModrinthVersion.fromMap(Map<String, dynamic> map) => ModrinthVersion(
        id: map['id'] as String,
        name: map['name'] as String,
        versionNumber: map['version_number'] as String,
        loaders: (map['loaders'] as List<dynamic>? ?? []).cast<String>(),
        changelog: map['changelog'] as String? ?? '',
      );
}
