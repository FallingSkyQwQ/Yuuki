// Placeholder for flutter_rust_bridge generated bindings.
// Run the codegen with FRB_DART_OUTPUT_DIR="lib/src/bridge" to regenerate.

class YuukiAccount {
  const YuukiAccount({
    required this.id,
    required this.username,
    required this.accountType,
    required this.provider,
  });

  final String id;
  final String username;
  final String accountType;
  final String provider;
}

class YuukiDeviceCode {
  const YuukiDeviceCode({
    required this.userCode,
    required this.verificationUri,
    required this.deviceCode,
    required this.expiresIn,
    required this.message,
    required this.pollInterval,
  });

  final String userCode;
  final String verificationUri;
  final String deviceCode;
  final int expiresIn;
  final String message;
  final int pollInterval;
}

class YuukiTokenResponse {
  const YuukiTokenResponse({
    required this.accessToken,
    required this.refreshToken,
    required this.expiresIn,
  });

  final String accessToken;
  final String refreshToken;
  final int expiresIn;
}
