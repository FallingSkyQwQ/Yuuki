// Export all types
export * from './Account';
export * from './GameProfile';
export * from './ModInfo';
export * from './Settings';

// Re-export Extension types with careful conflict resolution
export type { 
  Extension, 
  ExtensionManifest, 
  Permission, 
  PermissionType,
  ExtensionUIConfig,
  UIComponent,
  UILocation,
  ExtensionAPI,
  ExtensionEvent
} from './Extension';