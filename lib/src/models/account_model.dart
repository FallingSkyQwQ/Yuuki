enum AccountType { microsoft, offline, littleskin }

class AccountModel {
  AccountModel({required this.id, required this.username, required this.type, required this.provider});

  final String id;
  final String username;
  final AccountType type;
  final String provider;

  factory AccountModel.offline({String? id, String? username}) => AccountModel(
        id: id ?? 'offline-${DateTime.now().millisecondsSinceEpoch}',
        username: username ?? 'Offline Player',
        type: AccountType.offline,
        provider: 'offline',
      );

  factory AccountModel.microsoft({required String username, String? id}) => AccountModel(
        id: id ?? 'msft-${DateTime.now().millisecondsSinceEpoch}',
        username: username,
        type: AccountType.microsoft,
        provider: 'Microsoft',
      );

  factory AccountModel.littleskin({required String username, required String server, String? id}) => AccountModel(
        id: id ?? 'ls-${DateTime.now().millisecondsSinceEpoch}',
        username: username,
        type: AccountType.littleskin,
        provider: 'LittleSkin ($server)',
      );

  factory AccountModel.fromMap(Map<String, dynamic> map) => AccountModel(
        id: map['id'] as String? ?? '',
        username: map['username'] as String? ?? '',
        type: _typeFromString(map['type'] as String? ?? 'offline'),
        provider: map['provider'] as String? ?? 'offline',
      );

  Map<String, dynamic> toMap() => {
        'id': id,
        'username': username,
        'type': type.name,
        'provider': provider,
      };

  static AccountType _typeFromString(String value) {
    return AccountType.values.firstWhere(
      (type) => type.name == value,
      orElse: () => AccountType.offline,
    );
  }
}
