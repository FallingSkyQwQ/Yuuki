import 'dart:convert';

import 'package:http/http.dart' as http;

import '../models/modrinth_model.dart';

class ModrinthService {
  ModrinthService({http.Client? client}) : _client = client ?? http.Client();

  static const _base = 'https://api.modrinth.com/v2';
  static const _searchEndpoint = 'project';

  final http.Client _client;

  Future<List<ModrinthProject>> search(String query, {List<String>? loaders, int limit = 10}) async {
    final uri = Uri.parse('$_base/$_searchEndpoint').replace(queryParameters: {
      'query': query,
      'limit': limit.toString(),
      if (loaders != null && loaders.isNotEmpty) 'loaders': loaders.join(','),
    });
    final response = await _client.get(uri, headers: {'Accept': 'application/json'});
    if (response.statusCode != 200) {
      throw Exception('Modrinth search failed (${response.statusCode})');
    }
    final payload = jsonDecode(response.body) as Map<String, dynamic>;
    final hits = payload['hits'] as List<dynamic>? ?? [];
    return hits.cast<Map<String, dynamic>>().map(ModrinthProject.fromMap).toList();
  }

  Future<ModrinthVersion> fetchLatestVersion(String projectId) async {
    final uri = Uri.parse('$_base/project/$projectId/version').replace(queryParameters: {
      'limit': '1',
      'loaders': 'fabric,forge,quilt,neoforge,vanilla',
    });
    final response = await _client.get(uri);
    if (response.statusCode != 200) {
      throw Exception('Modrinth version fetch failed');
    }
    final list = jsonDecode(response.body) as List<dynamic>;
    final versionMap = list.first as Map<String, dynamic>;
    return ModrinthVersion.fromMap(versionMap);
  }
}
