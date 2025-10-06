/**
 * Game profile types and interfaces
 */

export interface GameProfile {
  id: string;
  name: string;
  version: string;
  gameDirectory: string;
  javaPath?: string;
  javaArgs: string[];
  gameArgs: string[];
  memory: MemoryConfig;
  mods: ModInfo[];
  createdAt: Date;
  lastUsed?: Date;
  isActive: boolean;
}

export interface MemoryConfig {
  min: number; // MB
  max: number; // MB
  permGen?: number; // MB (for older Java versions)
}

export interface ProfileConfig {
  name: string;
  version: string;
  gameDirectory?: string;
  javaPath?: string;
  javaArgs?: string[];
  gameArgs?: string[];
  memory?: Partial<MemoryConfig>;
  mods?: ModInfo[];
}

export interface ModInfo {
  id: string;
  name: string;
  version: string;
  fileName: string;
  filePath: string;
  size: number;
  hash: string;
  source: ModSource;
  dependencies: string[];
  isEnabled: boolean;
  modrinthId?: string;
  curseforgeId?: string;
  description?: string;
  author?: string;
  createdAt: Date;
  updatedAt: Date;
}

export type ModSource = 'modrinth' | 'curseforge' | 'local' | 'manual';

export interface MinecraftVersion {
  id: string;
  type: 'release' | 'snapshot' | 'old_beta' | 'old_alpha';
  releaseTime: Date;
  url: string;
  complianceLevel?: number;
}

export interface JavaVersion {
  component: string;
  majorVersion: number;
}