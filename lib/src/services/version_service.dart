import 'dart:convert';

import 'package:http/http.dart' as http;

import '../models/version_manifest.dart';

class VersionService {
  VersionService({http.Client? client}) : _client = client ?? http.Client();

  final http.Client _client;

  Future<List<VersionDescriptor>> fetchVersions() async {
    final uri = Uri.parse('https://launchermeta.mojang.com/mc/game/version_manifest_v2.json');
    final response = await _client.get(uri);
    if (response.statusCode != 200) {
      throw Exception('Version manifest request failed');
    }
    final payload = jsonDecode(response.body) as Map<String, dynamic>;
    final versions = payload['versions'] as List<dynamic>? ?? [];
    return versions.cast<Map<String, dynamic>>().map(VersionDescriptor.fromMap).toList();
  }
}
