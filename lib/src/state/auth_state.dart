import 'dart:convert';

import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../auth/services/account_repository.dart';
import '../auth/services/littleskin_auth_service.dart';
import '../auth/services/microsoft_auth_service.dart';
import '../models/account_model.dart';

class AuthRepository {
  AuthRepository({
    AccountRepository? accountRepository,
    MicrosoftAuthService? microsoftAuth,
    LittleSkinAuthService? littleSkinAuth,
  })  : _accountRepository = accountRepository ?? AccountRepository(),
        _microsoftAuth = microsoftAuth ?? MicrosoftAuthService(),
        _littleSkinAuth = littleSkinAuth ?? LittleSkinAuthService();

  final AccountRepository _accountRepository;
  final MicrosoftAuthService _microsoftAuth;
  final LittleSkinAuthService _littleSkinAuth;

  Future<List<AccountModel>> listAccounts() => _accountRepository.loadAccounts();

  Future<AccountModel> addOfflineAccount(String username) => _accountRepository.addOfflineAccount(username);

  Future<MicrosoftDeviceCode> requestDeviceCode() => _microsoftAuth.requestDeviceCode();

  Future<AccountModel> completeDeviceLogin(MicrosoftDeviceCode code) async {
    final token = await _microsoftAuth.pollForToken(code.deviceCode);
    final displayName = await _microsoftAuth.fetchDisplayName(token.accessToken);
    final account = await _accountRepository.addMicrosoftAccount(displayName);
    await _accountRepository.storeToken(account.id, jsonEncode(token.toMap()));
    return account;
  }

  Future<MicrosoftTokenResponse> refreshToken(AccountModel account) async {
    final data = await _accountRepository.loadToken(account.id);
    if (data == null) throw MicrosoftAuthException('No stored token for ${account.username}');
    final parsed = jsonDecode(data) as Map<String, dynamic>;
    final token = MicrosoftTokenResponse.fromJson(parsed);
    final refreshed = await _microsoftAuth.refreshToken(token.refreshToken);
    await _accountRepository.storeToken(account.id, jsonEncode(refreshed.toMap()));
    return refreshed;
  }

  Future<AccountModel> loginLittleSkin(String server, String username, String password) async {
    final token = await _littleSkinAuth.authenticate(
      serverUrl: server,
      username: username,
      password: password,
    );
    final account = await _accountRepository.addLittleSkinAccount(token.profileName, server);
    await _accountRepository.storeToken(account.id, jsonEncode(token.toJson()));
    return account;
  }
}

final authRepositoryProvider = Provider<AuthRepository>((_) => AuthRepository());

final accountsProvider = FutureProvider<List<AccountModel>>((ref) {
  final repo = ref.watch(authRepositoryProvider);
  return repo.listAccounts();
});

final activeAccountProvider = StateNotifierProvider<ActiveAccountNotifier, AccountModel?>((_) => ActiveAccountNotifier());

final deviceCodeProvider = StateNotifierProvider<DeviceCodeNotifier, AsyncValue<MicrosoftDeviceCode?>>(
  (ref) => DeviceCodeNotifier(ref.watch(authRepositoryProvider)),
);

final littleSkinAuthProvider = StateNotifierProvider<LittleSkinAuthNotifier, AsyncValue<AccountModel?>>(
  (ref) => LittleSkinAuthNotifier(ref.watch(authRepositoryProvider)),
);

class DeviceCodeNotifier extends StateNotifier<AsyncValue<MicrosoftDeviceCode?>> {
  DeviceCodeNotifier(this._repository) : super(const AsyncValue.data(null));

  final AuthRepository _repository;
  MicrosoftDeviceCode? _latestCode;

  Future<void> requestDeviceCode() async {
    state = const AsyncValue.loading();
    try {
      final code = await _repository.requestDeviceCode();
      _latestCode = code;
      state = AsyncValue.data(code);
    } catch (error, stack) {
      state = AsyncValue.error(error, stack);
    }
  }

  Future<void> completeLogin() async {
    final code = _latestCode;
    if (code == null) {
      state = AsyncValue.error('No device code requested', StackTrace.current);
      return;
    }
    state = const AsyncValue.loading();
    try {
      await _repository.completeDeviceLogin(code);
      state = AsyncValue.data(code);
    } catch (error, stack) {
      state = AsyncValue.error(error, stack);
      rethrow;
    }
  }
}

class LittleSkinAuthNotifier extends StateNotifier<AsyncValue<AccountModel?>> {
  LittleSkinAuthNotifier(this._repository) : super(const AsyncValue.data(null));

  final AuthRepository _repository;

  Future<void> authenticate({
    required String server,
    required String username,
    required String password,
  }) async {
    state = const AsyncValue.loading();
    try {
      final account = await _repository.loginLittleSkin(server.trim(), username.trim(), password);
      state = AsyncValue.data(account);
    } catch (error, stack) {
      state = AsyncValue.error(error, stack);
      rethrow;
    }
  }
}

class ActiveAccountNotifier extends StateNotifier<AccountModel?> {
  ActiveAccountNotifier() : super(null);

  void select(AccountModel? account) => state = account;
}
