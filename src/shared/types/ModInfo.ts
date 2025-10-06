/**
 * Mod and Modrinth types and interfaces
 */

export interface ModrinthProject {
  id: string;
  slug: string;
  title: string;
  description: string;
  categories: string[];
  clientSide: 'required' | 'optional' | 'unsupported';
  serverSide: 'required' | 'optional' | 'unsupported';
  body: string;
  status: 'approved' | 'rejected' | 'draft' | 'unlisted' | 'archived' | 'processing' | 'withheld';
  requestedStatus?: string;
  additionalCategories: string[];
  issuesUrl?: string;
  sourceUrl?: string;
  wikiUrl?: string;
  discordUrl?: string;
  donationUrls?: DonationLink[];
  projectType: 'mod' | 'modpack' | 'resourcepack';
  downloads: number;
  iconUrl?: string;
  color?: number;
  threadId?: string;
  monetizationStatus?: 'monetized' | 'demonetized' | 'force-demonetized';
  team: string;
  published: Date;
  updated: Date;
  approved?: Date;
  followers: number;
  license: License;
  versions: string[];
  gameVersions: string[];
  loaders: ModLoader[];
  gallery?: GalleryImage[];
}

export interface ModrinthVersion {
  id: string;
  projectId: string;
  authorId: string;
  featured: boolean;
  name: string;
  versionNumber: string;
  changelog: string;
  changelogUrl?: string;
  datePublished: Date;
  downloads: number;
  versionType: 'release' | 'beta' | 'alpha';
  files: ModrinthFile[];
  dependencies: ModDependency[];
  gameVersions: string[];
  loaders: ModLoader[];
}

export interface ModrinthFile {
  hashes: {
    sha512: string;
    sha1: string;
  };
  url: string;
  filename: string;
  primary: boolean;
  size: number;
  fileType?: 'required-resource-pack' | 'optional-resource-pack';
}

export interface ModDependency {
  versionId?: string;
  projectId?: string;
  fileName?: string;
  dependencyType: 'required' | 'optional' | 'incompatible' | 'embedded';
}

export interface ModrinthSearchResult {
  hits: ModrinthProject[];
  offset: number;
  limit: number;
  totalHits: number;
}

export interface ModSearchFilters {
  query?: string;
  facets?: string[][];
  offset?: number;
  limit?: number;
  index?: 'relevance' | 'downloads' | 'follows' | 'newest' | 'updated';
}

export interface ModUpdate {
  modId: string;
  currentVersion: string;
  latestVersion: string;
  changelog?: string;
  downloadUrl?: string;
  isRequired: boolean;
}

export interface ModpackInfo {
  id: string;
  name: string;
  version: string;
  minecraftVersion: string;
  modCount: number;
  description: string;
  author: string;
  downloads: number;
  iconUrl?: string;
  createdAt: Date;
  updatedAt: Date;
}

// Supporting types
export interface DonationLink {
  id: string;
  platform: string;
  url: string;
}

export interface License {
  id: string;
  name: string;
  url?: string;
}

export interface GalleryImage {
  url: string;
  featured: boolean;
  title?: string;
  description?: string;
  created: Date;
  ordering?: number;
}

export type ModLoader = 'fabric' | 'forge' | 'quilt' | 'rift' | 'neoForge';