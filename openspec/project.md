# Yuuki Launcher Project Context

## Purpose
构建一个跨平台的Minecraft启动器，提供现代化的用户体验，集成Modrinth资源管理，支持多种登录方式，并采用Rust+Flutter技术栈实现高性能和优美的LiquidGlass设计。

## Tech Stack
- **核心语言**: Rust (后端核心服务)
- **UI框架**: Flutter (跨平台界面)
- **FFI桥接**: flutter_rust_bridge
- **插件系统**: WASM宿主
- **数据存储**: YAML/JSON配置文件
- **搜索索引**: Tantivy (Rust全文搜索)

## Project Conventions

### Code Style
- **Rust**: 遵循标准Rust命名规范，使用snake_case，结构体使用CamelCase
- **Flutter/Dart**: 遵循Dart风格指南，使用lowerCamelCase变量名，UpperCamelCase类名
- **文件命名**: 使用kebab-case，动词-led命名法
- **代码组织**: 模块化设计，单一职责原则

### Architecture Patterns
- **分层架构**: UI层(Flutter) ↔ 核心服务层(Rust) ↔ 数据存储层
- **插件架构**: WASM沙箱环境，权限声明系统
- **事件驱动**: 生命周期事件系统(pre-download, post-download, pre-launch等)
- **响应式编程**: Flutter使用Riverpod状态管理

### Testing Strategy
- **Rust**: 单元测试 + 集成测试(使用模拟服务)
- **Flutter**: Widget测试 + Golden测试 + 集成测试
- **端到端**: Headless模式测试MC启动流程
- **性能基准**: 三档硬件配置测试(低/中/高)

### Git Workflow
- **主分支**: main
- **功能分支**: feature/[change-id]
- **提交规范**: 使用动词-led描述，关联change-id
- **代码审查**: 所有变更需要PR审查

## Domain Context
Minecraft启动器领域知识：
- 版本管理: Vanilla, Forge, NeoForge, Fabric, Quilt
- 资源平台: Modrinth (主要集成), BMCLAPI (镜像)
- 认证方式: Microsoft OAuth, 离线模式, LittleSkin (Yggdrasil)
- 资源类型: Mods, 资源包, 光影包
- 技术术语: Instance(实例), Profile(配置), Manifest(清单), Asset(资源)

## Important Constraints
- **平台支持**: Windows 10+, macOS 12+, Linux (Debian/Ubuntu/Arch/Fedora)
- **性能要求**: 冷启动≤1.5s, 大型整合包解析≤3s, 依赖解析≤1s
- **安全要求**: 令牌加密存储, 插件沙箱隔离, 供应链安全审计
- **合规要求**: 遥测默认关闭, 明示EULA/隐私政策
- **设计约束**: LiquidGlass材质, 动态形态变换, 渐进披露交互

## External Dependencies
- **Minecraft服务**: Mojang官方API, Microsoft认证服务
- **资源平台**: Modrinth API
- **镜像服务**: BMCLAPI
- **Java发行版**: Temurin, Zulu等
- **认证服务**: LittleSkin (Yggdrasil兼容)
