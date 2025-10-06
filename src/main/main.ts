import { app, BrowserWindow } from 'electron';
import path from 'path';
import { registerIpcHandlers, unregisterIpcHandlers } from './ipc-handlers';

// Declare global variables provided by webpack
declare const MAIN_WINDOW_VITE_DEV_SERVER_URL: string | undefined;
declare const MAIN_WINDOW_VITE_NAME: string;

// Fallback values in case webpack doesn't provide them
const DEV_SERVER_URL = typeof MAIN_WINDOW_VITE_DEV_SERVER_URL !== 'undefined'
  ? MAIN_WINDOW_VITE_DEV_SERVER_URL
  : process.env.NODE_ENV === 'development'
    ? 'http://localhost:9000'
    : undefined;

const WINDOW_NAME = typeof MAIN_WINDOW_VITE_NAME !== 'undefined'
  ? MAIN_WINDOW_VITE_NAME
  : 'main_window';

// Handle creating/removing shortcuts on Windows when installing/uninstalling.
if (require('electron-squirrel-startup')) {
  app.quit();
}

let mainWindow: BrowserWindow | null = null;

const createWindow = (): void => {
  // Create the browser window.
  mainWindow = new BrowserWindow({
    width: 1200,
    height: 800,
    minWidth: 800,
    minHeight: 600,
    webPreferences: {
      preload: path.join(__dirname, '../renderer/main_window/preload.js'),
      contextIsolation: true,
      nodeIntegration: false,
      sandbox: false, // Required for preload script to work properly
    },
    titleBarStyle: 'hiddenInset',
    frame: false,
    transparent: true,
    backgroundColor: '#00000000',
    show: false, // Don't show until ready-to-show
  });

  // Load the index.html of the app.
  console.log('Loading app URL...');
  console.log('DEV_SERVER_URL:', DEV_SERVER_URL);
  console.log('WINDOW_NAME:', WINDOW_NAME);
  try {
    if (DEV_SERVER_URL) {
      console.log('Loading from dev server:', DEV_SERVER_URL);
      mainWindow.loadURL(DEV_SERVER_URL);
    } else {
      console.log('Loading from file');
      mainWindow.loadFile(path.join(__dirname, `../renderer/${WINDOW_NAME}/index.html`));
    }
  } catch (error) {
    console.error('Error loading app URL:', error);
    // Fallback to local file
    mainWindow.loadFile(path.join(__dirname, '../renderer/main_window/index.html'));
  }

  // Show window when ready to prevent visual flash
  mainWindow.once('ready-to-show', () => {
    console.log('Window is ready to show');
    mainWindow?.show();
    console.log('Window shown successfully');

    // Open DevTools in development
    if (process.env.NODE_ENV === 'development') {
      console.log('Opening DevTools...');
      mainWindow?.webContents.openDevTools();
    }
  });

  // Handle window closed
  mainWindow.on('closed', () => {
    console.log('Window closed');
    mainWindow = null;
  });

  // Handle navigation errors
  mainWindow.webContents.on('did-fail-load', (event, errorCode, errorDescription) => {
    console.error('Failed to load:', errorCode, errorDescription);
  });

  // Handle page load
  mainWindow.webContents.on('did-finish-load', () => {
    console.log('Page finished loading');
  });

  // Register IPC handlers
  console.log('Registering IPC handlers...');
  registerIpcHandlers(mainWindow);
  console.log('IPC handlers registered successfully');

  // Set Content Security Policy for security
  mainWindow.webContents.session.webRequest.onHeadersReceived((details, callback) => {
    callback({
      responseHeaders: {
        ...details.responseHeaders,
        'Content-Security-Policy': [
          "default-src 'self';",
          "script-src 'self' 'unsafe-inline' 'unsafe-eval';",
          "style-src 'self' 'unsafe-inline';",
          "img-src 'self' data: https:;",
          "connect-src 'self' https:;",
          "font-src 'self' data:;"
        ].join(' ')
      }
    });
  });
};

// App event handlers
app.whenReady().then(() => {
  console.log('App is ready, creating window...');
  createWindow();

  // On macOS, re-create a window when the dock icon is clicked
  app.on('activate', () => {
    console.log('App activated');
    if (BrowserWindow.getAllWindows().length === 0) {
      console.log('No windows found, creating new window');
      createWindow();
    }
  });
});

app.on('ready', () => {
  console.log('Electron app is fully ready');
});

app.on('window-all-closed', () => {
  // On macOS, don't quit the app when window is closed
  if (process.platform !== 'darwin') {
    app.quit();
  }
});

app.on('before-quit', () => {
  // Cleanup IPC handlers
  unregisterIpcHandlers();
});

// Security: Prevent new window creation
app.on('web-contents-created', (_, webContents) => {
  webContents.setWindowOpenHandler(({ url }) => {
    // Prevent new windows from opening
    return { action: 'deny' };
  });
});

// Handle certificate errors
app.on('certificate-error', (event, _webContents, url, _error, _certificate, callback) => {
  // In development, ignore certificate errors for localhost
  if (process.env.NODE_ENV === 'development' && (url as string).includes('localhost')) {
    event.preventDefault();
    callback(true);
  } else {
    callback(false);
  }
});