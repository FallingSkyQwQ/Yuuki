import 'dart:async';

import 'bridge_stub.dart';

/// TODO: Replace this shim with the real flutter_rust_bridge bindings (see
/// lib/src/bridge/generated). The codegen will expose classes such as
/// `YuukiRustApi` that can be used to drive the runtime.
class YuukiBridge {
  YuukiBridge._();

  static Future<String> ping() async {
    try {
      await Future<void>.delayed(const Duration(milliseconds: 120));
      return 'Yuuki core is alive (stub)';
    } catch (error) {
      return 'Bridge error: $error';
    }
  }

  static Future<bool> initializeCore() async {
    await Future<void>.delayed(const Duration(milliseconds: 200));
    return true;
  }

  static Future<List<YuukiAccount>> listAccounts() async {
    await Future<void>.delayed(const Duration(milliseconds: 140));
    return List<YuukiAccount>.from(_mockAccounts);
  }

  static Future<YuukiDeviceCode> startDeviceLogin(String provider) async {
    await Future<void>.delayed(const Duration(milliseconds: 250));
    final codeSuffix = DateTime.now().millisecondsSinceEpoch % 100000;
    return YuukiDeviceCode(
      userCode: '$provider-${codeSuffix.toString().padLeft(5, '0')}',
      verificationUri: 'https://microsoft.com/devicelogin',
      deviceCode: 'device-$codeSuffix',
      expiresIn: 600,
      pollInterval: 5,
      message: 'Enter the code on the Microsoft device login page.',
    );
  }

  static Future<YuukiTokenResponse?> refreshToken(String accountId) async {
    await Future<void>.delayed(const Duration(milliseconds: 180));
    final now = DateTime.now().millisecondsSinceEpoch;
    return YuukiTokenResponse(
      accessToken: 'atk-$accountId-$now',
      refreshToken: 'rtk-$accountId-$now',
      expiresIn: 900,
    );
  }

  static Future<YuukiAccount> addOfflineAccount(String username) async {
    await Future<void>.delayed(const Duration(milliseconds: 240));
    final newAccount = YuukiAccount(
      id: 'offline-${DateTime.now().millisecondsSinceEpoch}',
      username: username,
      accountType: 'offline',
      provider: 'offline',
    );
    _mockAccounts.add(newAccount);
    return newAccount;
  }
}

final _mockAccounts = <YuukiAccount>[
  const YuukiAccount(
    id: 'local.offline',
    username: 'Offline Player',
    accountType: 'offline',
    provider: 'offline',
  ),
  const YuukiAccount(
    id: 'msft:abc',
    username: 'Microsoft Tester',
    accountType: 'microsoft',
    provider: 'Microsoft',
  ),
];
