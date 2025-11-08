import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../models/version_manifest.dart';
import '../services/version_service.dart';

final versionServiceProvider = Provider<VersionService>((_) => VersionService());
final versionListProvider = FutureProvider<List<VersionDescriptor>>((ref) {
  final service = ref.watch(versionServiceProvider);
  return service.fetchVersions();
});

final loaderSelectionProvider = StateProvider<Set<String>>((_) => {'vanilla'});
