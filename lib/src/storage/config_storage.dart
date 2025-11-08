import 'dart:io';

import '../models/config_models.dart';

class ConfigStorage {
  ConfigStorage({required File profileFile}) : _profileFile = profileFile;

  final File _profileFile;

  Future<ProfileModel> loadProfile() async {
    if (!await _profileFile.exists()) {
      return ProfileModel.defaultProfile();
    }
    final contents = await _profileFile.readAsString();
    return ProfileModel.fromYaml(contents);
  }

  Future<void> saveProfile(ProfileModel model) async {
    final yaml = model.toYaml();
    await _profileFile.writeAsString(yaml, flush: true);
  }
}
