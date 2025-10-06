/**
 * Account types and interfaces
 */

export interface BaseAccount {
  id: string;
  username: string;
  uuid: string;
  type: AccountType;
  createdAt: Date;
  lastUsed?: Date;
}

export interface MicrosoftAccount extends BaseAccount {
  type: 'microsoft';
  accessToken: string;
  refreshToken: string;
  expiresAt: Date;
  xboxToken?: string;
  minecraftToken?: string;
}

export interface OfflineAccount extends BaseAccount {
  type: 'offline';
  skinPath?: string;
}

export type Account = MicrosoftAccount | OfflineAccount;
export type AccountType = 'microsoft' | 'offline';

export interface AccountCreateRequest {
  type: AccountType;
  username?: string;
  accessToken?: string;
  refreshToken?: string;
  expiresAt?: Date;
}

export interface AccountUpdateRequest {
  username?: string;
  accessToken?: string;
  refreshToken?: string;
  expiresAt?: Date;
  skinPath?: string;
}