# Yuuki Implementation Plan

## Stage 1: Project Structure and Core Infrastructure

**Goal**: Establish the foundational architecture with proper layering, dependency injection, database setup, and logging

**Success Criteria**:
- Solution compiles successfully with all layers ✅
- Dependency injection container is configured and functional ✅
- EF Core and SQLite database can be initialized ✅
- Logging system captures application events ✅
- Custom exception hierarchy implemented ✅

**Tests**:
- ✅ Solution compiles without errors
- ✅ All NuGet packages restored successfully
- ✅ Directory structure created for all layers
- ✅ ServiceProvider configures DI container
- ✅ DbContext configured for SQLite
- ✅ Serilog logging configured with file and console sinks
- ✅ Custom exception classes created

**Status**: Complete

### Tasks:
- [x] Check existing project structure
- [x] Create layered architecture directory structure
- [x] Configure dependency injection container and service registration
- [x] Set up Entity Framework Core and SQLite database
- [x] Configure logging system and error handling infrastructure

### Completed Components:
- Exception hierarchy: `YuukiException`, `AuthenticationException`, `DownloadException`, `LaunchException`
- Enum types: `ModLoaderType`, `ModPlatform`, `LaunchState`
- Data model: `GameInstance` entity
- `YuukiDbContext` with SQLite configuration
- `ServiceProvider` with DI, logging, and database initialization
- Updated `App.xaml.cs` with DI integration and global error handling

---

## Stage 2: Data Models and Data Access Layer

**Goal**: Implement complete data model entities and repository pattern for data access

**Success Criteria**:
- All entity models created and properly configured ✅
- DbContext includes all entities with relationships ✅
- Repository pattern implemented for data access ✅
- EF Core relationships properly configured ✅
- Solution compiles successfully ✅

**Tests**:
- ✅ Solution compiles without errors
- ✅ Entity models have proper properties and relationships
- ✅ DbContext configuration includes indexes and constraints
- ✅ Repository interfaces and implementations created
- ✅ Repositories registered in DI container

**Status**: Complete

### Tasks:
- [x] Create core data model classes
  - [x] GameInstance entity (updated with relationships)
  - [x] MinecraftVersion model
  - [x] ModInfo and related models (ModDependency, ModFile)
  - [x] InstalledMod entity
  - [x] LaunchSettings model
  - [x] Supporting enums (DependencyType)
- [x] Implement Entity Framework Core configuration
  - [x] Update YuukiDbContext with InstalledMod
  - [x] Configure entity relationships (GameInstance <-> InstalledMod)
  - [x] Add database indexes for performance
  - [x] Implement Repository pattern
  - [x] Create specialized repositories (GameInstanceRepository, InstalledModRepository)
  - [x] Register repositories in DI container

### Completed Components:
- **Entities**: `GameInstance` (updated), `InstalledMod`, `MinecraftVersion`, `ModInfo`, `LaunchSettings`
- **Supporting Models**: `ModLoaderCompatibility`, `ModDependency`, `ModFile`
- **Enums**: `DependencyType`
- **DbContext**: Updated with InstalledMod DbSet and relationship configuration
- **Repository Pattern**: `IRepository<T>`, `Repository<T>` base classes
- **Specialized Repositories**: `IGameInstanceRepository`, `IInstalledModRepository` with implementations
- **DI Registration**: All repositories registered as scoped services

---

## Stage 3: External API Service Integration

**Goal**: Implement API clients for Mojang, mod platforms, and mod loaders with retry policies

**Success Criteria**:
- Mojang API client implemented with version manifest parsing ✅
- Mod platform API client (Modrinth) implemented ✅
- Mod loader APIs (Fabric/Forge/NeoForge) integrated ✅
- HTTP retry policies configured with Polly ✅
- All API services registered in DI container ✅
- Solution compiles successfully ✅

**Tests**:
- ✅ Solution compiles without errors
- ✅ API response models properly defined
- ✅ Retry policies configured for resilience
- ✅ HttpClient properly configured with timeouts
- ✅ All API services registered as scoped/singleton

**Status**: Complete

### Tasks:
- [x] Implement Mojang API client
  - [x] Version manifest fetching
  - [x] Version detail parsing
  - [x] File download with progress tracking
  - [x] Retry policy with exponential backoff
- [x] Implement mod platform API clients
  - [x] Modrinth search API
  - [x] Modrinth project details API
  - [x] Modrinth version listing API
  - [x] File download with progress
- [x] Implement mod loader API integration
  - [x] Fabric Meta API for loader versions
  - [x] Forge promotions API
  - [x] NeoForge Maven metadata (stub)
- [x] Register all API services in DI container
  - [x] Configure HttpClient with timeouts
  - [x] Add Microsoft.Extensions.Http package

### Completed Components:
- **Mojang API**: `MojangApiService` with version manifest, details, and download support
- **Modrinth API**: `ModrinthApiService` with search, project details, and versions
- **Mod Loader APIs**: `ModLoaderApiService` for Fabric, Forge, NeoForge
- **API Models**: Complete response models for all APIs
  - `MojangApiModels` (VersionManifestResponse, VersionDetail, Library, etc.)
  - `ModrinthApiModels` (ModrinthSearchResponse, ModrinthProject, ModrinthVersion, etc.)
  - `ModLoaderApiModels` (FabricLoaderVersion, ForgePromos, etc.)
- **Resilience**: Polly retry policies with exponential backoff
- **DI Registration**: HttpClient factory pattern with proper configuration

---

## Stage 4: Microsoft Account Authentication System

**Goal**: Implement Microsoft account OAuth2.0 authentication with MSAL and Minecraft profile integration

**Success Criteria**:
- MSAL library integrated for Microsoft authentication ✅
- OAuth2.0 flow implemented with proper scopes ✅
- Xbox Live and XSTS authentication chain implemented ✅
- Minecraft profile retrieval working ✅
- User account data model with token storage ✅
- Account repository for multi-account management ✅
- All services registered in DI container ✅
- Solution compiles successfully ✅

**Tests**:
- ✅ Solution compiles without errors
- ✅ MSAL client properly configured
- ✅ Authentication flow chain complete (MS -> Xbox -> XSTS -> Minecraft)
- ✅ Token refresh logic implemented
- ✅ Account repository with active account management
- ✅ All authentication services registered

**Status**: Complete

### Tasks:
- [x] Add MSAL NuGet package (Microsoft.Identity.Client 4.67.1)
- [x] Create account data models
  - [x] UserAccount entity with token storage
  - [x] AccountType enum (Microsoft, Offline)
  - [x] AuthResult model for operation results
- [x] Implement MSAL authentication integration
  - [x] PublicClientApplication configuration
  - [x] Interactive authentication flow
  - [x] Xbox Live authentication
  - [x] XSTS authentication
  - [x] Minecraft authentication
  - [x] Profile retrieval from Minecraft API
- [x] Implement account management service
  - [x] Token refresh with silent authentication
  - [x] Sign out functionality
  - [x] Multi-account support
- [x] Create account repository
  - [x] Active account management
  - [x] Account lookup by UUID/email
  - [x] Last used ordering
- [x] Register services in DI container
- [x] Update DbContext with UserAccount entity

### Completed Components:
- **Account Models**: `UserAccount`, `AuthResult`, `AccountType` enum
- **Authentication Service**: `AccountManager` with full Microsoft/Xbox/Minecraft auth chain
- **Authentication Flow**:
  1. Microsoft account OAuth2.0 (MSAL)
  2. Xbox Live authentication
  3. XSTS token acquisition
  4. Minecraft authentication
  5. Profile retrieval
- **Repository**: `UserAccountRepository` with multi-account management
- **Database**: UserAccount entity with indexes on Uuid (unique), Email, IsActive
- **DI Registration**: AccountManager registered with HttpClient factory
- **Token Management**: Refresh token support with silent authentication

### Authentication Chain Details:
```
Microsoft Account (MSAL)
    ↓ Access Token
Xbox Live Authentication
    ↓ Xbox Token
XSTS Authentication
    ↓ XSTS Token + UserHash
Minecraft Authentication
    ↓ Minecraft Access Token
Minecraft Profile API
    ↓ Username, UUID, Skin
```

---

## Stage 5: Version Management System Implementation

**Goal**: Implement Minecraft version download, installation, validation, and management system

**Success Criteria**:
- Version download manager implemented ✅
- Version manifest fetching and parsing ✅
- Client JAR download with hash verification ✅
- Library download system ✅
- Asset download system ✅
- Version validation and deletion ✅
- All services registered in DI container ✅
- Solution compiles successfully ✅

**Tests**:
- ✅ Solution compiles without errors
- ✅ VersionManager interface and implementation created
- ✅ Download progress tracking implemented
- ✅ SHA1 hash verification for downloaded files
- ✅ Version directory structure management
- ✅ Registered in DI container

**Status**: Complete

### Tasks:
- [x] Create download progress tracking models
  - [x] DownloadProgress model with percentage calculation
  - [x] DownloadResult model with success/failure states
- [x] Implement VersionManager service
  - [x] GetAvailableVersionsAsync - Fetch version manifest
  - [x] DownloadVersionAsync - Download version with progress
  - [x] DeleteVersionAsync - Remove installed version
  - [x] ValidateVersionAsync - Verify version integrity
  - [x] GetInstalledVersionsAsync - List installed versions
- [x] Implement download pipeline
  - [x] Client JAR download with SHA1 verification
  - [x] Library dependency resolution and download
  - [x] Asset index download
  - [x] Mod loader installation (stub for future)
- [x] Register VersionManager in DI container
- [x] Compile and verify

### Completed Components:
- **Models**: `DownloadProgress`, `DownloadResult`
- **Version Manager**: `IVersionManager`, `VersionManager`
- **Download Features**:
  - Version manifest fetching from Mojang API
  - Client JAR download with progress tracking
  - SHA1 hash verification for integrity
  - Library dependency resolution with OS rules
  - Asset index download
  - Version JSON persistence
- **File Management**:
  - Version directory: `%LocalAppData%\Yuuki\minecraft\versions\{versionId}\`
  - Libraries directory: `%LocalAppData%\Yuuki\minecraft\libraries\`
  - Assets directory: `%LocalAppData%\Yuuki\minecraft\assets\`
- **Progress Reporting**: IProgress<DownloadProgress> for real-time UI updates
- **DI Registration**: VersionManager registered as scoped service

### Implementation Details:
```
VersionManager Download Pipeline:
1. Fetch version manifest from Mojang
2. Get version details (JSON)
3. Download client JAR with SHA1 verification
4. Download required libraries (with OS filtering)
5. Download asset index
6. Install mod loader (if specified - stub)
7. Save version JSON locally
```

**File Locations**:
- VersionManager.cs:114-241 - Download pipeline implementation
- VersionManager.cs:315-334 - Library OS rules filtering
- VersionManager.cs:383-388 - SHA1 hash computation

---

## Stage 6: Mod Management System Implementation

**Goal**: Implement comprehensive mod management including search, install, update, and compatibility checking

**Success Criteria**:
- Unified mod search interface implemented ✅
- Mod download and installation system ✅
- Mod compatibility checking ✅
- Mod enable/disable toggle ✅
- Mod update notification and one-click update ✅
- All services registered in DI container ✅
- Solution compiles successfully ✅

**Tests**:
- ✅ Solution compiles without errors
- ✅ ModManager interface and implementation created
- ✅ Search integrated with Modrinth API
- ✅ Install/uninstall mod operations
- ✅ Enable/disable mod toggle (file renaming)
- ✅ Update checking and auto-update
- ✅ Compatibility checking with version and loader
- ✅ Registered in DI container

**Status**: Complete

### Tasks:
- [x] Create unified mod search interface
  - [x] SearchModsAsync with platform support
  - [x] Integration with Modrinth API
- [x] Implement mod download and installation
  - [x] InstallModAsync with progress tracking
  - [x] Download from Modrinth with hash verification
  - [x] Save mod to instance mods directory
  - [x] Create InstalledMod database record
- [x] Add mod compatibility checking
  - [x] CheckCompatibilityAsync
  - [x] Minecraft version compatibility
  - [x] Mod loader compatibility
  - [x] Dependency checking (basic)
- [x] Implement mod enable/disable toggle
  - [x] ToggleModAsync
  - [x] File renaming (.jar <-> .jar.disabled)
  - [x] Update database status
- [x] Implement mod update notification and one-click update
  - [x] CheckForUpdatesAsync
  - [x] UpdateModAsync with progress
  - [x] Version comparison logic
- [x] Register ModManager in DI container
- [x] Compile and verify

### Completed Components:
- **Interface**: `IModManager` with 8 public methods
- **Implementation**: `ModManager` (597 lines)
- **Supporting Models**:
  - `ModUpdateInfo` - Update notification details
  - `ModCompatibilityResult` - Compatibility check results
- **Core Features**:
  1. **Search**: Modrinth mod search with filters
  2. **Install**: Download mod, verify hash, save to mods dir
  3. **Uninstall**: Delete mod file and database record
  4. **Toggle**: Enable/disable by file renaming
  5. **Update Check**: Compare installed vs latest versions
  6. **Update**: One-click update to latest compatible version
  7. **Compatibility**: Check Minecraft version, mod loader, dependencies
  8. **List**: Get all installed mods for instance
- **File Management**:
  - Mods directory: `%LocalAppData%\Yuuki\instances\{instance_id}\mods\`
  - Enable/disable via `.disabled` suffix
  - SHA1 hash verification
- **Progress Reporting**: IProgress<DownloadProgress> for UI updates
- **DI Registration**: ModManager registered as scoped service

### Implementation Details:
```
ModManager Operation Flow:
1. Search: Modrinth API -> ModInfo list
2. Install: Download -> Verify hash -> Save file -> Create DB record
3. Toggle: Rename file (.jar <-> .jar.disabled) -> Update DB
4. Update Check: Fetch versions -> Compare -> Return update list
5. Update: Uninstall old -> Install new
6. Compatibility: Check MC version + loader + dependencies
```

**File Locations**:
- ModManager.cs:123-150 - Search implementation
- ModManager.cs:152-267 - Install with progress and hash verification
- ModManager.cs:315-369 - Toggle enable/disable
- ModManager.cs:371-436 - Update checking
- ModManager.cs:438-491 - One-click update
- ModManager.cs:493-564 - Compatibility checking

---

## Next Stages (Planned):

### Stage 7: Game Launch Core System Implementation
