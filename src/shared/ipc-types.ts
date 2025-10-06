/**
 * IPC event channels and payload types
 */

// Main process events
export const IPC_CHANNELS = {
  // App events
  APP_VERSION: 'app:version',
  APP_QUIT: 'app:quit',
  
  // Theme events
  THEME_GET: 'theme:get',
  THEME_SET: 'theme:set',
  THEME_RESET: 'theme:reset',
  
  // Account events
  ACCOUNT_LIST: 'account:list',
  ACCOUNT_CREATE: 'account:create',
  ACCOUNT_UPDATE: 'account:update',
  ACCOUNT_DELETE: 'account:delete',
  ACCOUNT_SWITCH: 'account:switch',
  
  // Auth events
  AUTH_MICROSOFT_LOGIN: 'auth:microsoft:login',
  AUTH_MICROSOFT_REFRESH: 'auth:microsoft:refresh',
  AUTH_MICROSOFT_LOGOUT: 'auth:microsoft:logout',
  AUTH_OFFLINE_CREATE: 'auth:offline:create',
  
  // Profile events
  PROFILE_LIST: 'profile:list',
  PROFILE_CREATE: 'profile:create',
  PROFILE_UPDATE: 'profile:update',
  PROFILE_DELETE: 'profile:delete',
  PROFILE_EXPORT: 'profile:export',
  PROFILE_IMPORT: 'profile:import',
  
  // Game events
  GAME_LAUNCH: 'game:launch',
  GAME_STATUS: 'game:status',
  GAME_KILL: 'game:kill',
  GAME_LOG: 'game:log',
  
  // Download events
  DOWNLOAD_START: 'download:start',
  DOWNLOAD_PAUSE: 'download:pause',
  DOWNLOAD_RESUME: 'download:resume',
  DOWNLOAD_CANCEL: 'download:cancel',
  DOWNLOAD_PROGRESS: 'download:progress',
  DOWNLOAD_COMPLETE: 'download:complete',
  DOWNLOAD_ERROR: 'download:error',
  
  // Modrinth events
  MODRINTH_SEARCH: 'modrinth:search',
  MODRINTH_PROJECT: 'modrinth:project',
  MODRINTH_VERSIONS: 'modrinth:versions',
  MODRINTH_DOWNLOAD: 'modrinth:download',
  
  // Modpack events
  MODPACK_INSTALL: 'modpack:install',
  MODPACK_PROGRESS: 'modpack:progress',
  MODPACK_EXPORT: 'modpack:export',
  
  // Monitor events
  MONITOR_START: 'monitor:start',
  MONITOR_STOP: 'monitor:stop',
  MONITOR_DATA: 'monitor:data',
  
  // Extension events
  EXTENSION_LIST: 'extension:list',
  EXTENSION_INSTALL: 'extension:install',
  EXTENSION_UNINSTALL: 'extension:uninstall',
  EXTENSION_ENABLE: 'extension:enable',
  EXTENSION_DISABLE: 'extension:disable',
} as const;

export type IpcChannel = typeof IPC_CHANNELS[keyof typeof IPC_CHANNELS];

// Common interfaces
export interface IpcResponse<T = any> {
  success: boolean;
  data?: T;
  error?: {
    code: string;
    message: string;
    details?: any;
  };
}

export interface IpcProgress {
  id: string;
  percentage: number;
  status: 'pending' | 'downloading' | 'paused' | 'completed' | 'error';
  speed?: number;
  remainingTime?: number;
  message?: string;
}