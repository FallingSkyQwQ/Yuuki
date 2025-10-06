/**
 * Settings and configuration types
 */

export interface AppSettings {
  theme: ThemeSettings;
  launcher: LauncherSettings;
  modrinth: ModrinthSettings;
  download: DownloadSettings;
  game: GameSettings;
  monitor: MonitorSettings;
  extensions: ExtensionSettings;
}

export interface ThemeSettings {
  transparency: number; // 0-100
  accentColor: string;
  animations: boolean;
  fontSize: 'small' | 'medium' | 'large';
  colorScheme: 'auto' | 'light' | 'dark';
  customColors?: {
    primary?: string;
    secondary?: string;
    background?: string;
    surface?: string;
    text?: string;
    textSecondary?: string;
  };
}

export interface LauncherSettings {
  closeToTray: boolean;
  autoUpdate: boolean;
  javaPath?: string;
  language: string;
  checkUpdatesOnLaunch: boolean;
  enableBetaUpdates: boolean;
  showNews: boolean;
  startMinimized: boolean;
  windowBounds: WindowBounds;
}

export interface ModrinthSettings {
  apiKey?: string;
  cacheTimeout: number; // seconds
  preferredSource: 'modrinth' | 'curseforge';
  showSnapshots: boolean;
  maxConcurrentDownloads: number;
  downloadPath?: string;
}

export interface DownloadSettings {
  maxConcurrentDownloads: number;
  maxRetries: number;
  retryDelay: number; // milliseconds
  timeout: number; // milliseconds
  useProxy: boolean;
  proxyHost?: string;
  proxyPort?: number;
  proxyUsername?: string;
  proxyPassword?: string;
}

export interface GameSettings {
  defaultMemory: MemoryAllocation;
  defaultJavaArgs: string[];
  defaultGameArgs: string[];
  showGameLog: boolean;
  gameLogLevel: 'error' | 'warn' | 'info' | 'debug';
  autoCloseLauncher: boolean;
  keepLauncherOpen: boolean;
  customGameResolution?: {
    width: number;
    height: number;
  };
}

export interface MemoryAllocation {
  min: number; // MB
  max: number; // MB
}

export interface MonitorSettings {
  enabled: boolean;
  updateInterval: number; // milliseconds
  showOverlay: boolean;
  overlayPosition: 'top-left' | 'top-right' | 'bottom-left' | 'bottom-right';
  overlayOpacity: number; // 0-100
  performanceAlerts: boolean;
  cpuThreshold: number; // percentage
  memoryThreshold: number; // percentage
  storageThreshold: number; // GB
}

export interface ExtensionSettings {
  enabled: boolean;
  autoUpdate: boolean;
  allowUnsigned: boolean;
  maxConcurrentExtensions: number;
  extensionStoreUrl?: string;
  trustedPublishers: string[];
}

export interface WindowBounds {
  width: number;
  height: number;
  x?: number;
  y?: number;
  maximized?: boolean;
}

export interface UserPreferences {
  selectedAccount?: string;
  selectedProfile?: string;
  recentProfiles: string[];
  favoriteProfiles: string[];
  recentSearches: string[];
  dismissedNotifications: string[];
  lastUpdateCheck?: Date;
}

export interface AppConfig {
  version: string;
  buildNumber: number;
  releaseDate: Date;
  environment: 'development' | 'staging' | 'production';
  updateChannel: 'stable' | 'beta' | 'alpha';
  apiUrl: string;
  modrinthApiUrl: string;
  minecraftManifestUrl: string;
  supportUrl: string;
  bugReportUrl: string;
  privacyPolicyUrl: string;
}