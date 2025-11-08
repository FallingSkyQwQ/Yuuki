class VersionDescriptor {
  VersionDescriptor({
    required this.id,
    required this.type,
    required this.url,
  });

  final String id;
  final String type;
  final String url;

  factory VersionDescriptor.fromMap(Map<String, dynamic> map) => VersionDescriptor(
        id: map['id'] as String,
        type: map['type'] as String,
        url: map['url'] as String,
      );
}
