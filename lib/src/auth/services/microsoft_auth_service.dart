import 'dart:async';
import 'dart:convert';

import 'package:http/http.dart' as http;

const _microsoftClientId = '5cde052b-0132-4f10-a523-186d372365ff';
const _microsoftScope = 'offline_access openid profile User.Read';
const _authorizationTenant = 'consumers';

class MicrosoftDeviceCode {
  MicrosoftDeviceCode({
    required this.userCode,
    required this.verificationUri,
    required this.deviceCode,
    required this.expiresIn,
    required this.message,
    required this.interval,
  });

  final String userCode;
  final String verificationUri;
  final String deviceCode;
  final int expiresIn;
  final String message;
  final int interval;
}

class MicrosoftTokenResponse {
  MicrosoftTokenResponse({
    required this.accessToken,
    required this.refreshToken,
    required this.expiresIn,
    required this.scope,
    required this.tokenType,
  });

  final String accessToken;
  final String refreshToken;
  final int expiresIn;
  final String scope;
  final String tokenType;

  factory MicrosoftTokenResponse.fromJson(Map<String, dynamic> json) => MicrosoftTokenResponse(
        accessToken: json['access_token'] as String,
        refreshToken: json['refresh_token'] as String,
        expiresIn: json['expires_in'] as int,
        scope: json['scope'] as String,
        tokenType: json['token_type'] as String,
      );

  Map<String, dynamic> toMap() => {
        'access_token': accessToken,
        'refresh_token': refreshToken,
        'expires_in': expiresIn,
        'scope': scope,
        'token_type': tokenType,
      };
}

class MicrosoftAuthException implements Exception {
  MicrosoftAuthException(this.message, [this.code]);

  final String message;
  final String? code;

  @override
  String toString() => 'MicrosoftAuthException(code: $code, message: $message)';
}

class MicrosoftAuthService {
  MicrosoftAuthService({http.Client? client}) : _client = client ?? http.Client();

  final http.Client _client;
  final Uri _deviceCodeUri = Uri.https(
    'login.microsoftonline.com',
    '/$_authorizationTenant/oauth2/v2.0/devicecode',
  );
  final Uri _tokenUri = Uri.https(
    'login.microsoftonline.com',
    '/$_authorizationTenant/oauth2/v2.0/token',
  );
  final Uri _profileUri = Uri.https('graph.microsoft.com', '/v1.0/me');

  Future<MicrosoftDeviceCode> requestDeviceCode() async {
    final response = await _client.post(_deviceCodeUri, body: {
      'client_id': _microsoftClientId,
      'scope': _microsoftScope,
    });
    if (response.statusCode != 200) {
      throw MicrosoftAuthException('Failed to request device code (${response.statusCode})');
    }
    final data = jsonDecode(response.body) as Map<String, dynamic>;
    return MicrosoftDeviceCode(
      userCode: data['user_code'] as String,
      verificationUri: data['verification_uri'] as String,
      deviceCode: data['device_code'] as String,
      expiresIn: data['expires_in'] as int,
      message: data['message'] as String,
      interval: data['interval'] as int,
    );
  }

  Future<MicrosoftTokenResponse> pollForToken(String deviceCode) async {
    while (true) {
      final response = await _client.post(_tokenUri, body: {
        'grant_type': 'urn:ietf:params:oauth:grant-type:device_code',
        'client_id': _microsoftClientId,
        'device_code': deviceCode,
      });
      final data = jsonDecode(response.body) as Map<String, dynamic>;
      if (response.statusCode == 200) {
        return MicrosoftTokenResponse.fromJson(data);
      }
      final error = data['error'] as String? ?? 'unknown_error';
      final interval = (data['interval'] as int?) ?? 5;
      switch (error) {
        case 'authorization_pending':
          await Future.delayed(Duration(seconds: interval));
          continue;
        case 'slow_down':
          await Future.delayed(Duration(seconds: interval + 5));
          continue;
        default:
          throw MicrosoftAuthException(data['error_description'] as String? ?? 'Authorization failed', error);
      }
    }
  }

  Future<MicrosoftTokenResponse> refreshToken(String refreshToken) async {
    final response = await _client.post(_tokenUri, body: {
      'grant_type': 'refresh_token',
      'client_id': _microsoftClientId,
      'refresh_token': refreshToken,
    });
    if (response.statusCode != 200) {
      throw MicrosoftAuthException('Refresh failed (${response.statusCode})');
    }
    final data = jsonDecode(response.body) as Map<String, dynamic>;
    return MicrosoftTokenResponse.fromJson(data);
  }

  Future<String> fetchDisplayName(String accessToken) async {
    final response = await _client.get(_profileUri, headers: {
      'Authorization': 'Bearer $accessToken',
      'Accept': 'application/json',
    });
    if (response.statusCode != 200) {
      return 'Microsoft User';
    }
    final data = jsonDecode(response.body) as Map<String, dynamic>;
    return data['displayName'] as String? ?? data['userPrincipalName'] as String? ?? 'Microsoft User';
  }
}
