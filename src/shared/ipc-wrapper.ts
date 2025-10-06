import { ipcMain, ipcRenderer, IpcMainInvokeEvent } from 'electron';
import { IpcChannel, IpcResponse } from './ipc-types';

/**
 * Type-safe IPC wrapper for main process
 */
export class IpcMainWrapper {
  /**
   * Handle an invoke event from renderer
   */
  static handle<T, R>(
    channel: IpcChannel,
    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    handler: (_event: IpcMainInvokeEvent, _data: T) => Promise<R> | R
  ): void {
    ipcMain.handle(channel, async (_event, data: T) => {
      try {
        const result = await handler(_event, data);
        return {
          success: true,
          data: result,
        } as IpcResponse<R>;
      } catch (error) {
        return {
          success: false,
          error: {
            code: 'IPC_ERROR',
            message: error instanceof Error ? error.message : String(error),
            details: error,
          },
        } as IpcResponse<R>;
      }
    });
  }

  /**
   * Send an event to renderer
   */
  static send<T>(window: any, channel: IpcChannel, data: T): void {
    window.webContents.send(channel, data);
  }
}

/**
 * Type-safe IPC wrapper for renderer process
 */
export class IpcRendererWrapper {
  /**
   * Invoke an event to main process
   */
  static async invoke<T, R>(channel: IpcChannel, data?: T): Promise<R> {
    try {
      const response: IpcResponse<R> = await ipcRenderer.invoke(channel, data);
      if (response.success) {
        if (response.data === undefined) {
          throw new Error(`No data returned from IPC channel: ${channel}`);
        }
        return response.data;
      } else {
        throw new Error(response.error?.message || `IPC error on channel: ${channel}`);
      }
    } catch (error) {
      throw new Error(`IPC invoke failed: ${error instanceof Error ? error.message : String(error)}`);
    }
  }

  /**
   * Listen for events from main process
   */
  static on<T>(channel: IpcChannel, listener: (data: T) => void): () => void {
    const wrappedListener = (_event: any, data: T) => listener(data);
    ipcRenderer.on(channel, wrappedListener);
    
    // Return unsubscribe function
    return () => {
      ipcRenderer.removeListener(channel, wrappedListener);
    };
  }

  /**
   * Listen for events from main process (one-time)
   */
  static once<T>(channel: IpcChannel, listener: (data: T) => void): void {
    const wrappedListener = (_event: any, data: T) => {
      listener(data);
    };
    ipcRenderer.once(channel, wrappedListener);
  }

  /**
   * Remove all listeners for a channel
   */
  static removeAllListeners(channel: IpcChannel): void {
    ipcRenderer.removeAllListeners(channel);
  }
}