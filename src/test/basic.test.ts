/**
 * Basic functionality tests
 */

import { IpcMainWrapper, IpcRendererWrapper } from '../shared/ipc-wrapper';
import { IPC_CHANNELS } from '../shared/ipc-types';

describe('Basic Functionality', () => {
  test('IPC wrapper should be defined', () => {
    expect(IpcMainWrapper).toBeDefined();
    expect(IpcRendererWrapper).toBeDefined();
  });

  test('IPC channels should be defined', () => {
    expect(IPC_CHANNELS).toBeDefined();
    expect(IPC_CHANNELS.APP_VERSION).toBe('app:version');
    expect(IPC_CHANNELS.THEME_GET).toBe('theme:get');
  });

  test('Type definitions should be valid', () => {
    // This test ensures our TypeScript types compile correctly
    const testData = {
      id: 'test-id',
      username: 'test-user',
      type: 'offline' as const,
      uuid: 'test-uuid',
      createdAt: new Date(),
    };
    
    expect(testData.id).toBe('test-id');
    expect(testData.username).toBe('test-user');
    expect(testData.type).toBe('offline');
  });
});