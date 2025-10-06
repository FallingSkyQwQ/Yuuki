import { contextBridge, ipcRenderer } from 'electron';
import { IPC_CHANNELS } from '../shared/ipc-types';

// Expose protected methods that allow the renderer process to use
// the ipcRenderer without exposing the entire object
contextBridge.exposeInMainWorld('electronAPI', {
  // Basic IPC methods
  invoke: (channel: string, data?: any) => ipcRenderer.invoke(channel, data),
  on: (channel: string, listener: (data: any) => void) => {
    const subscription = (_event: any, data: any) => listener(data);
    ipcRenderer.on(channel, subscription);
    return () => ipcRenderer.removeListener(channel, subscription);
  },
  
  // Direct IPC channels for common operations
  getAppVersion: () => ipcRenderer.invoke(IPC_CHANNELS.APP_VERSION),
  quitApp: () => ipcRenderer.invoke(IPC_CHANNELS.APP_QUIT),
  
  // Theme methods
  getTheme: () => ipcRenderer.invoke(IPC_CHANNELS.THEME_GET),
  setTheme: (themeData: any) => ipcRenderer.invoke(IPC_CHANNELS.THEME_SET, themeData),
  resetTheme: () => ipcRenderer.invoke(IPC_CHANNELS.THEME_RESET),
  
  // Account methods
  getAccounts: () => ipcRenderer.invoke(IPC_CHANNELS.ACCOUNT_LIST),
  createAccount: (accountData: any) => ipcRenderer.invoke(IPC_CHANNELS.ACCOUNT_CREATE, accountData),
  
  // Profile methods
  getProfiles: () => ipcRenderer.invoke(IPC_CHANNELS.PROFILE_LIST),
  createProfile: (profileData: any) => ipcRenderer.invoke(IPC_CHANNELS.PROFILE_CREATE, profileData),
  
  // Game methods
  getGameStatus: () => ipcRenderer.invoke(IPC_CHANNELS.GAME_STATUS),
  
  // Utility methods
  platform: process.platform,
  arch: process.arch,
  versions: process.versions,
});

// TypeScript declarations for the exposed API
declare global {
  interface Window {
    electronAPI: {
      invoke: (channel: string, data?: any) => Promise<any>;
      on: (channel: string, listener: (data: any) => void) => () => void;
      getAppVersion: () => Promise<string>;
      quitApp: () => Promise<void>;
      getTheme: () => Promise<any>;
      setTheme: (themeData: any) => Promise<boolean>;
      resetTheme: () => Promise<any>;
      getAccounts: () => Promise<any[]>;
      createAccount: (accountData: any) => Promise<any>;
      getProfiles: () => Promise<any[]>;
      createProfile: (profileData: any) => Promise<any>;
      getGameStatus: () => Promise<any>;
      platform: string;
      arch: string;
      versions: any;
    };
  }
}