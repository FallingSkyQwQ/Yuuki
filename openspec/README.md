# Yuuki Launcher OpenSpec Configuration

## 项目概述
本项目使用OpenSpec进行规格驱动的开发管理，基于spec.md中的详细要求配置了完整的Minecraft启动器规格体系。

## 配置的能力规格

### 1. 认证管理 (authentication)
- Microsoft账户登录（设备代码流程）
- 离线账户登录
- LittleSkin（Yggdrasil兼容）登录
- 多账户管理和快速切换
- 安全的令牌存储（平台原生机制）

### 2. 实例管理 (instance-management)
- 实例创建和配置（游戏版本、加载器、Java设置）
- 实例导入/导出（跨平台迁移）
- 实例克隆
- 实例资源管理（Mod、资源包、光影）
- 实例启动参数和性能优化配置

### 3. Modrinth集成 (modrinth-integration)
- Modrinth搜索功能（模组、资源包、光影）
- 内容详情展示（版本列表、兼容性矩阵）
- 下载和安装（自动依赖处理）
- 更新管理（更新通知、批量更新）
- 内容分类和标签过滤
- API集成（速率限制、错误处理）

### 4. 下载管理 (download-management)
- 并发下载管理
- 镜像源管理（官方/BMCLAPI切换）
- 源健康监控（延迟、可用性）
- 下载验证（SHA校验）
- 断点续传和智能重试
- 下载进度可视化
- 带宽管理和I/O限流

### 5. 启动器核心 (launcher-core)
- Minecraft启动流程（资源校验、下载、Java选择）
- 进程监控和管理
- 性能优化（JVM预设、大整合包优化）
- 启动诊断
- 多实例管理
- 启动历史和统计

### 6. UI设计 (ui-design)
- LiquidGlass材质实现（多层玻璃、模糊、高光）
- 三层分离架构（内容层/玻璃层/交互层）
- 动态形态变换（展开/收缩动画）
- 导航组件（Sidebar/TabBar）
- 渐进披露交互模式
- 可访问性支持（键盘导航、屏幕阅读器）
- 高对比度模式
- 响应式设计

## 技术架构
- **核心语言**: Rust（后端服务）
- **UI框架**: Flutter（跨平台界面）
- **FFI桥接**: flutter_rust_bridge
- **插件系统**: WASM宿主
- **数据存储**: YAML/JSON配置
- **搜索索引**: Tantivy

## 项目约定
- 代码风格：Rust使用snake_case，Flutter使用lowerCamelCase
- 文件命名：kebab-case，动词-led命名法
- 架构模式：分层架构、插件架构、事件驱动
- 测试策略：单元测试 + 集成测试 + 端到端测试
- Git工作流：功能分支，动词-led提交描述

## 使用说明

### 查看规格
```bash
openspec list --specs                    # 列出所有规格
openspec show <spec-name> --type spec    # 查看具体规格
```

### 验证规格
```bash
openspec validate --strict               # 验证所有规格
```

### 创建变更
```bash
# 按照AGENTS.md中的三阶段工作流创建变更提案
mkdir -p openspec/changes/<change-id>/specs/<capability>
# 创建proposal.md, tasks.md, 和规格增量文件
```

## 规格文件结构
```
openspec/
├── project.md              # 项目上下文和约定
├── AGENTS.md              # OpenSpec使用说明
├── specs/                 # 当前规格（已部署）
│   ├── authentication/
│   ├── instance-management/
│   ├── modrinth-integration/
│   ├── download-management/
│   ├── launcher-core/
│   └── ui-design/
└── changes/               # 变更提案
    └── archive/          # 已完成的变更
```

## 验证状态
✅ 所有核心能力规格已创建
✅ 规格格式符合OpenSpec要求
✅ 项目配置完整
✅ 可以使用openspec工具进行管理

## 下一步
基于这些规格，可以开始：
1. 创建具体的变更提案来实现功能
2. 按照三阶段工作流进行开发
3. 使用规格驱动开发确保功能符合要求
