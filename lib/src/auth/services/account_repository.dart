import 'dart:convert';
import 'dart:io';

import 'package:path/path.dart' as p;
import 'package:path_provider/path_provider.dart';
import 'package:uuid/uuid.dart';

import '../../models/account_model.dart';
import '../../storage/encrypted_storage.dart';

class AccountRepository {
  AccountRepository({EncryptedStorage? encryptedStorage})
      : _crypto = encryptedStorage ?? EncryptedStorage();

  static const _accountsFile = 'accounts.json';

  final EncryptedStorage _crypto;

  Future<File> _file() async {
    final directory = await getApplicationSupportDirectory();
    final path = p.join(directory.path, _accountsFile);
    return File(path);
  }

  Future<List<AccountModel>> loadAccounts() async {
    final file = await _file();
    if (!await file.exists()) {
      return const [];
    }
    final contents = await file.readAsString();
    if (contents.isEmpty) return const [];
    final decoded = jsonDecode(contents);
    if (decoded is! List) return const [];
    return decoded
        .cast<Map<String, dynamic>>()
        .map(AccountModel.fromMap)
        .toList();
  }

  Future<void> saveAccounts(List<AccountModel> accounts) async {
    final file = await _file();
    await file.create(recursive: true);
    final encoded = jsonEncode(accounts.map((account) => account.toMap()).toList());
    await file.writeAsString(encoded, flush: true);
  }

  Future<AccountModel> upsertAccount(AccountModel account) async {
    final accounts = await loadAccounts();
    accounts.removeWhere((existing) => existing.id == account.id);
    accounts.add(account);
    await saveAccounts(accounts);
    return account;
  }

  Future<AccountModel> addOfflineAccount(String username) async {
    final account = AccountModel.offline(id: const Uuid().v4(), username: username);
    return upsertAccount(account);
  }

  Future<AccountModel> addMicrosoftAccount(String username) async {
    final account = AccountModel.microsoft(username: username, id: const Uuid().v4());
    return upsertAccount(account);
  }

  Future<AccountModel> addLittleSkinAccount(String username, String server) async {
    final account = AccountModel.littleskin(
      username: username,
      server: server,
      id: const Uuid().v4(),
    );
    return upsertAccount(account);
  }

  Future<String?> loadToken(String accountId) => _crypto.read('token:$accountId');

  Future<void> storeToken(String accountId, String token) => _crypto.write('token:$accountId', token);
}
