import { BrowserWindow } from 'electron';
import { IpcMainWrapper } from '../shared/ipc-wrapper';
import { IPC_CHANNELS } from '../shared/ipc-types';

/**
 * Register IPC handlers for main process
 */
export function registerIpcHandlers(mainWindow: BrowserWindow): void {
  console.log('Registering IPC handlers...');

  // App handlers
  IpcMainWrapper.handle(IPC_CHANNELS.APP_VERSION, async () => {
    return process.env.npm_package_version || '1.0.0';
  });

  IpcMainWrapper.handle(IPC_CHANNELS.APP_QUIT, async () => {
    mainWindow.close();
  });

  // Theme handlers
  IpcMainWrapper.handle(IPC_CHANNELS.THEME_GET, async () => {
    // TODO: Implement theme manager
    return {
      transparency: 80,
      accentColor: '#667eea',
      animations: true,
      fontSize: 'medium',
      colorScheme: 'auto',
    };
  });

  IpcMainWrapper.handle(IPC_CHANNELS.THEME_SET, async (event, themeData) => {
    // TODO: Implement theme manager
    console.log('Theme set:', themeData);
    return true;
  });

  IpcMainWrapper.handle(IPC_CHANNELS.THEME_RESET, async () => {
    // TODO: Implement theme manager
    return {
      transparency: 80,
      accentColor: '#667eea',
      animations: true,
      fontSize: 'medium',
      colorScheme: 'auto',
    };
  });

  // Account handlers (placeholders)
  IpcMainWrapper.handle(IPC_CHANNELS.ACCOUNT_LIST, async () => {
    return [];
  });

  IpcMainWrapper.handle(IPC_CHANNELS.ACCOUNT_CREATE, async (event, accountData: any) => {
    console.log('Create account:', accountData);
    return { 
      id: 'test-id', 
      username: accountData.username || 'test', 
      type: accountData.type || 'offline',
      uuid: 'test-uuid-12345',
      createdAt: new Date()
    };
  });

  // Profile handlers (placeholders)
  IpcMainWrapper.handle(IPC_CHANNELS.PROFILE_LIST, async () => {
    return [];
  });

  IpcMainWrapper.handle(IPC_CHANNELS.PROFILE_CREATE, async (event, profileData: any) => {
    console.log('Create profile:', profileData);
    return { 
      id: 'test-profile-id', 
      name: profileData.name || 'Test Profile', 
      version: profileData.version || '1.20.1',
      javaArgs: [],
      gameArgs: [],
      memory: { min: 1024, max: 2048 },
      mods: [],
      createdAt: new Date()
    };
  });

  // Game handlers (placeholders)
  IpcMainWrapper.handle(IPC_CHANNELS.GAME_STATUS, async () => {
    return { status: 'stopped' };
  });

  console.log('IPC handlers registered successfully');
}

/**
 * Unregister IPC handlers (cleanup)
 */
export function unregisterIpcHandlers(): void {
  // Remove all IPC handlers
  const { ipcMain } = require('electron');
  
  Object.values(IPC_CHANNELS).forEach(channel => {
    ipcMain.removeAllListeners(channel);
  });
  
  console.log('IPC handlers unregistered');
}