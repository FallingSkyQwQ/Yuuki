import 'dart:async';
import 'dart:math';
import 'dart:convert';

class LittleSkinToken {
  LittleSkinToken({
    required this.accessToken,
    required this.clientToken,
    required this.expiry,
    required this.profileName,
    required this.uuid,
  });

  final String accessToken;
  final String clientToken;
  final DateTime expiry;
  final String profileName;
  final String uuid;

  factory LittleSkinToken.fromJson(Map<String, dynamic> json) => LittleSkinToken(
        accessToken: json['accessToken'] as String,
        clientToken: json['clientToken'] as String,
        expiry: DateTime.parse(json['expiry'] as String),
        profileName: json['profileName'] as String,
        uuid: json['uuid'] as String,
      );

  Map<String, dynamic> toJson() => {
        'accessToken': accessToken,
        'clientToken': clientToken,
        'expiry': expiry.toUtc().toIso8601String(),
        'profileName': profileName,
        'uuid': uuid,
      };
}

class LittleSkinAuthException implements Exception {
  LittleSkinAuthException(this.message);

  final String message;

  @override
  String toString() => 'LittleSkinAuthException: $message';
}

class LittleSkinAuthService {
  LittleSkinAuthService({Duration? delay}) : _delay = delay ?? const Duration(milliseconds: 450);

  final Duration _delay;
  final _random = Random.secure();

  Future<LittleSkinToken> authenticate({
    required String serverUrl,
    required String username,
    required String password,
  }) async {
    await Future<void>.delayed(_delay);
    if (serverUrl.isEmpty) {
      throw LittleSkinAuthException('Server URL cannot be empty');
    }
    final uuid = _random.nextInt(1000000).toString().padLeft(6, '0');
    return LittleSkinToken(
      accessToken: base64UrlEncode(utf8.encode('$username:$uuid:${DateTime.now().millisecondsSinceEpoch}')),
      clientToken: base64UrlEncode(utf8.encode('$serverUrl:${password.hashCode}')),
      expiry: DateTime.now().add(const Duration(hours: 1)),
      profileName: username,
      uuid: uuid,
    );
  }
}
