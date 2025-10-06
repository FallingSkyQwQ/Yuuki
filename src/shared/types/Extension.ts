/**
 * Extension types and interfaces
 */

export interface ExtensionManifest {
  id: string;
  name: string;
  version: string;
  description: string;
  author: string;
  entryPoint: string;
  permissions: Permission[];
  dependencies?: string[];
  optionalDependencies?: string[];
  ui?: ExtensionUIConfig;
  apiVersion: string;
  minimumLauncherVersion: string;
}

export interface Extension {
  id: string;
  name: string;
  version: string;
  description: string;
  author: string;
  enabled: boolean;
  installedAt: Date;
  updatedAt: Date;
  manifest: ExtensionManifest;
  path: string;
  permissions: Permission[];
}

export interface Permission {
  type: PermissionType;
  description?: string;
  required?: boolean;
}

export type PermissionType = 
  | 'filesystem:read'
  | 'filesystem:write'
  | 'filesystem:full'
  | 'network:request'
  | 'network:websocket'
  | 'game:profile:read'
  | 'game:profile:write'
  | 'game:launch'
  | 'game:monitor'
  | 'account:read'
  | 'account:write'
  | 'mod:read'
  | 'mod:write'
  | 'mod:download'
  | 'settings:read'
  | 'settings:write'
  | 'ui:component'
  | 'ui:notification'
  | 'system:info'
  | 'system:process'
  | 'extension:manage';

export interface ExtensionUIConfig {
  components?: UIComponent[];
  settings?: ExtensionSettings;
  themes?: ExtensionTheme[];
}

export interface UIComponent {
  id: string;
  type: 'panel' | 'button' | 'menu' | 'toolbar' | 'dialog';
  name: string;
  location: UILocation;
  icon?: string;
  description?: string;
}

export type UILocation = 
  | 'sidebar'
  | 'toolbar'
  | 'game:launcher'
  | 'game:console'
  | 'settings:extension'
  | 'mod:manager'
  | 'profile:editor';

export interface ExtensionSettings {
  schema: SettingsSchema;
  defaults?: Record<string, any>;
}

export interface SettingsSchema {
  [key: string]: SettingDefinition;
}

export interface SettingDefinition {
  type: 'string' | 'number' | 'boolean' | 'array' | 'object' | 'select' | 'color';
  label: string;
  description?: string;
  default?: any;
  required?: boolean;
  options?: string[];
  min?: number;
  max?: number;
  step?: number;
  pattern?: string;
}

export interface ExtensionTheme {
  id: string;
  name: string;
  colors: {
    primary?: string;
    secondary?: string;
    accent?: string;
    background?: string;
    surface?: string;
    text?: string;
    textSecondary?: string;
  };
}

export interface ExtensionAPI {
  // Data access
  getGameProfiles(): Promise<GameProfile[]>;
  getInstalledMods(_profileId: string): Promise<any[]>;
  getCurrentAccount(): Promise<Account | null>;
  getSettings(): Promise<AppSettings>;
  
  // Actions
  launchGame(_profileId: string): Promise<void>;
  installMod(_modId: string, _profileId: string): Promise<void>;
  showNotification(_message: string, _type: 'info' | 'success' | 'warning' | 'error'): void;
  
  // Events
  on(_event: ExtensionEvent, _callback: (data: any) => void): void;
  off(_event: ExtensionEvent, _callback: (data: any) => void): void;
  
  // UI
  registerUIComponent(_component: UIComponent): void;
  unregisterUIComponent(_componentId: string): void;
}

export type ExtensionEvent = 
  | 'game:launched'
  | 'game:exited'
  | 'game:crashed'
  | 'account:switched'
  | 'profile:created'
  | 'profile:updated'
  | 'profile:deleted'
  | 'mod:installed'
  | 'mod:removed'
  | 'theme:changed'
  | 'settings:changed';



// Import related types
import { GameProfile } from './GameProfile';
import { Account } from './Account';
import { AppSettings } from './Settings';