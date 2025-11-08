
# Yuuki 启动器（跨平台）— 完整规格说明 v1.0

## 0. 范围与约束（Based on your rules）

* 平台：Windows 10+/macOS 12+/主流 Linux（Debian/Ubuntu/Arch/Fedora）。
* 技术栈：**Rust（Core）+ Flutter（UI）**，FFI 互通。
* 资源来源与镜像：

  * **模组/资源包/光影：仅集成 Modrinth**（搜索、详情、下载、依赖/兼容性）。
  * **官方源 ⇄ BMCLAPI** 一键切换（版本清单、assets、libraries、客户端与映射文件下载等基建内容）。
* 登录支持：**Microsoft**、**离线**、**LittleSkin（Yggdrasil 兼容）**。
* 设计语言：**LiquidGlass** 统一材质 + 分层（内容/玻璃/交互）+ 动态形态变换 + 渐进披露；Sidebar/Tab 的 **动态收缩** 行为与高对比度/弱动效可达。  

---

## 1. 体验与信息架构（IA）

### 1.1 顶层导航

* **Sidebar（桌面宽屏）**：主页、实例、资源中心、下载、日志、设置；Sidebar 具折射/反射效果，滚动时淡化并“给内容让路”。
* **TabBar（窄窗/小屏）**：随滚动**动态收缩**，保留可达性（Icon-only）。
* 搜索（全局）：模糊 → Modrinth、本地实例、本地日志关键字联搜；快捷键 ⌘/Ctrl + K。

### 1.2 关键页面

* **主页（Hub）**：最近游玩、正在下载、公告/版本新闻、体检（Java/显卡驱动/网络镜像）。
* **实例（Profiles）**

  * 列表：卡片（LiquidGlass 材质，层级分明）。
  * 详情：版本/加载器、Java、内存/JVM 参数、Mod 集合、资源包、光影、存档/屏幕截图、导出/克隆/快捷修复。
* **资源中心（Modrinth 专区）**：分类为【模组｜资源包｜光影】；搜索/过滤（游戏版本、加载器、标签、依赖）；卡片信息（兼容性、下载量、更新日期、依赖树）。
* **下载面板**：并发队列、速度、源（官方/BMCLAPI）、失败重试/切源、SHA 校验。
* **日志与诊断**：实时流（级别过滤/关键词），崩溃报告（FAQ 映射）、环境体检结果与一键修复。
* **设置**：账户/登录、网络（代理、镜像切换策略/优先级）、外观（LiquidGlass 强度、色散/模糊级别、弱动效/高对比）、隐私（遥测默认关）、插件、同步（可选）。

> 交互原则：**内容优先、渐进披露与动态形态变换**（展开高级参数/切换视图/滚动收缩），避免干扰内容并保持流畅过渡。

---

## 2. 功能规格（Functional Spec）

### 2.1 登录与账户

* **Microsoft**：Device Code 流程 + 刷新令牌；本机安全存储（Windows Credential Manager/macOS Keychain/Linux Secret Service）。
* **离线**：本地档位（清晰标注“受限功能”）。
* **LittleSkin（Yggdrasil 兼容）**：可配置认证根（authserver/origin）；支持换肤资料同步（仅在用户明确授权时）。
* 多账户并存、快捷切换；账户云备份（可选，端到端加密）。

### 2.2 版本与实例

* **版本解析**：官方 manifest（稳定/快照/旧版）+ Forge/NeoForge/Fabric/Quilt；OptiFine/Iris（仅参数与可用性提示）。
* **实例（Profile）**：

  * 独立目录与依赖隔离；每实例可指定 **Java 版本**、最小/最大内存、分辨率、JVM/MC 启动参数。
  * **导入/导出**：打包为 `.zip`（包含 manifest 及 Modrinth 元数据），支持跨平台迁移。
  * **克隆**：一键复制实例含 Mod 集合与资源设置。

### 2.3 Java 管理

* 自动下载与选择（Temurin/Zulu 等发行版）；与实例绑定；哈希校验与回退镜像（BMCLAPI 可提供 JRE/JDK 镜像时使用）。

### 2.4 Modrinth 集成（唯一第三方内容源）

* **搜索**：按游戏版本、加载器、分类、标签、排序（下载量/热度/更新时间）。
* **详情**：简介、版本列表、兼容矩阵、依赖/冲突图、变更记录。
* **下载与更新**：解析最新兼容版本，自动拉取依赖；若冲突，提供“修复方案”。
* **仅支持类别**：**模组、资源包、光影**（Shaders）。

> 说明：**不集成 CurseForge/其他平台**，严格按你要求收敛。

### 2.5 镜像/源切换（官方 ⇄ BMCLAPI）

* 可切换项：**版本 manifest、客户端 jar、libraries、assets、索引与校验、（可选）Java 发行版**。
* 策略：

  * 手动切换（设置）或**智能切源**（下载失败/高延迟 → 自动回退并提示）。
  * 健康度探测：启动时对关键端点做 HEAD/延迟测试，生成优先级表（本地缓存 24h）。

### 2.6 启动与性能

* 启动链：Assets/Libraries 校验 → 并发下载（带 I/O 限流）→ Java 选择 → 参数装配（含加载器/OptiFine/Iris）→ 进程监控 → 崩溃采集（需同意）。
* JVM 预设：G1/ZGC、常见 flags；可视化推荐与基于内存的 Preset。
* **大整合包优化**：并行解析、差分更新、缓存重用。

### 2.7 日志/诊断/体检

* 实时日志（结构化 JSONL + 人类可读视图），过滤/书签/导出；崩溃日志自动打包（屏蔽 PII）。
* 体检：网络（官方/BMCLAPI 延迟）、磁盘/权限、显卡驱动、Java 安装、Mod 冲突快速扫描。

### 2.8 插件与脚本（可选）

* **插件宿主（WASM 优先）**：权限声明（fs/net/ui/launcher）；UI 扩展点（新页签/面板/卡片）。
* **生命周期事件**：pre-download/post-download、pre-launch/post-launch、profile-changed 等。
* **任务脚本**：定时备份、自动打包、上传（仅输出本地包，禁止直接推送到第三方社区以规避 ToS 风险）。

### 2.9 可访问性与本地化

* 全键盘可达、屏幕阅读、弱动效/高对比度；内容优先的渐进披露。
* 语言：简体中文/日文/英文首发；运行时可热更新词条。

### 2.10 隐私与合规

* 明示 EULA/隐私；遥测默认关闭；数据导出/删除；第三方服务清单。

---

## 3. 设计系统（与 designs.md 对齐的实现要点）

### 3.1 LiquidGlass 材质实现

* Flutter 侧实现**多层玻璃**：背景采样 + 模糊 + 高光 + 伪折射位移 + 轻度色散；按设备档位自动降级（低端仅模糊/高光）。
* **三层分离**：内容层/玻璃层/交互层，交互层所有控件悬浮且动态让出内容区域。
* **动态形态变换**：展开高级设置/切换视图/滚动收缩 Sidebar/Tab 的形变过渡与物理曲线。

### 3.2 导航与控件

* Sidebar 折射/反射背景、情境感知；TabBar 滚动收缩保可达性；控件圆角与硬件圆角呼应。
* 渐进披露：默认仅关键参数，展开后显高级选项（JVM/自定义参数/代理细粒度）。

---

## 4. 技术架构

### 4.1 进程与模块

* **UI 进程（Flutter）**：渲染、交互、A11y、本地化；持有状态（Riverpod/Flutter BLoC 二选一）。
* **Core 服务（Rust）**：网络并发下载、版本/加载器解析、镜像策略、安装器、启动管线、日志、插件宿主、Modrinth 适配器、认证适配器。
* **FFI 桥**：Dart <-> Rust：建议 **flutter_rust_bridge**（或 cbindgen + 手写 FFI）。
* **Sandboxes**：WASM 插件与 JS（可选，默认关）均处沙箱，权限白名单。

### 4.2 Rust Core 模块划分

```
crates/
  yuuki-core/           // Facade，暴露 FFI Safe API
  auth/                 // Microsoft/Offline/LittleSkin(Yggdrasil) 认证
  manifests/            // 版本/加载器解析、官方/BMCLAPI 源适配
  modrinth/             // Modrinth API 访问、搜索、依赖解析
  downloader/           // 并发下载、断点续传、校验、镜像策略
  launcher/             // 启动参数装配、进程监控、崩溃采集
  java-manager/         // JRE/JDK 管理
  diagnostics/          // 体检、日志聚合、崩溃分析
  plugins/host-wasm/    // WASM 宿主、事件总线、权限系统
  search/               // 本地索引（tantivy）与聚合
  store/                // 配置/缓存/加密（libsodium/age）
```

### 4.3 Dart FFI API（示意）

```ts
// Dart 假想接口（由 flutter_rust_bridge 生成）
class YuukiCore {
  Future<List<ProfileSummary>> listProfiles();
  Future<ProfileDetail> getProfile(String id);
  Future<void> createOrUpdateProfile(ProfileDetail data);
  Future<void> exportProfile(String id, String targetZip);
  Stream<DownloadEvent> installMinecraft(InstallPlan plan); // 版本/库/资源安装
  Future<AuthResult> loginMicrosoft(DeviceCodeOptions opts);
  Future<AuthResult> loginOffline(String username);
  Future<AuthResult> loginLittleSkin(String serverBase, String user, String pass);
  Future<SearchResult> modrinthSearch(SearchQuery q);
  Future<ResolveResult> modrinthResolveDependencies(List<ModSpec> mods);
  Future<void> setMirror(MirrorKind kind); // OFFICIAL | BMCLAPI
  Future<HealthReport> runDiagnostics();
  Stream<LogEvent> tailLogs(TailOptions opts);
}
```

### 4.4 数据模型（YAML/JSON）

* `accounts.json`：`{id,type: microsoft|offline|littleskin, tokens?, serverBase?}`
* `profiles/*.yml`：

  ```yml
  id: snowpack
  name: Snowpack S1
  game:
    version: 1.20.1
    loader: fabric  # forge|neoforge|quilt|vanilla
  java:
    runtime: temurin-17
    xms: 2g
    xmx: 6g
    jvmArgs: ["-XX:+UseG1GC"]
  resources:
    mods: [ {projectId: "...", versionId: "..."} ]
    resourcePacks: [ ... ]
    shaders: [ ... ]   # 以上全部来源于 Modrinth
  graphics:
    preset: balanced
    resolution: 1600x900
  ```
* `settings.yml`：镜像/代理/外观（LiquidGlass 强度、色散级别、弱动效）、隐私、下载并发/限速。
* `locks/*.lock`：下载/安装互斥。
* `plugins/*.toml`：manifest、权限、入口、UI 扩展点。

### 4.5 源适配与策略

* **MirrorKind**：`OFFICIAL` / `BMCLAPI`
* **切换点**：版本清单、assets 索引/对象、libraries、客户端 jar、映射文件、（可选）Java 发行版。
* 健康度：延迟/可用性打分（本地缓存 24h）；下载失败 n 次→自动回退并记录“镜像事件”。

### 4.6 下载器

* 多任务并发、RPS/带宽自适应、断点续传、分块校验（SHA1/256）、错误分级重试（可替换源）。
* 队列可视化 + CLI 诊断输出（开发模式）。

---

## 5. 安全与合规

* 令牌加密保存，机密只进系统凭据库；插件默认无网络/文件权限，需声明并获用户授权。
* 供应链安全：Cargo/Flutter 依赖审计、SBOM 生成；发布包签名（Windows MSIX 签名、macOS Notarization）。
* 数据最小化：崩溃包脱敏，禁默认遥测；显式的第三方服务与条款列表。

---

## 6. 性能目标（SLO）

* App 冷启动 ≤ 1.5s（缓存命中）；UI 过渡稳定 60–120Hz（弱动效模式降级）。
* 大型整合包（>300 mods）解析 ≤ 3s；依赖解析 ≤ 1s（常见场景）。
* 并发下载对 50MB/s 链路可线性接近上限（CPU 占用可控，磁盘 I/O 限流）。

---

## 7. 构建与发布

* Windows：MSIX；自动更新（差分）；代码签名。
* macOS：.dmg/.pkg + Notarization；Sparkle 风格增量。
* Linux：AppImage/Flatpak；提供 AUR/DNF/DEB 脚本。
* 应用内渠道：Stable / Preview，可一键回滚上个版本。

---

## 8. QA 与可观测性

* **测试**：

  * Rust：单测/集成（假服务 + 文件系统沙盒）。
  * Flutter：Widget/Golden/集成（Headless 渲染基准）。
  * 端到端：Headless 启动 MC、模拟下载/断网/镜像切换/崩溃注入。
* **日志**：结构化 + 人读；崩溃打包含系统/驱动/Java/Mod 列表。
* **基准**：三档硬件（低/中/高）+ 三平台，验证材质回退策略与帧率。

---

## 9. 项目里程碑（建议）

* **M0 架构雏形（2–3 周）**：Rust Core 骨架 + Flutter Shell + FFI + LiquidGlass 最小落地（材质/分层/动态收缩雏形）。 
* **M1 版本/下载器（4–6 周）**：官方/BMCLAPI 源适配、并发下载/校验、版本安装（Vanilla+Fabric/Forge）。
* **M2 Modrinth（4–5 周）**：搜索/详情/依赖解析/安装更新，仅 Mod/资源包/光影；实例导出。
* **M3 启动与诊断（3–4 周）**：启动链、崩溃采集、体检/日志流、Java 管理。
* **M4 登录与账户（2–3 周）**：Microsoft/离线/LittleSkin；多账户/云备份（可选）。
* **M5 插件与发布（3–4 周）**：WASM 宿主 + UI 扩展点；三端打包与自动更新。
* **RC 打磨（2–3 周）**：A11y/本地化/性能与功耗。

---

## 10. 风险与回退

* **材质渲染开销**：在低端 GPU 上强度降级（仅模糊 + 高光），禁用色散；提供“极简外观”。
* **镜像政策变化**：镜像列表与切换点可配置；故障自动回退官方源。
* **Mod 依赖复杂**：图模型与“修复方案”推荐；失败可一键生成“最小复现包”。

---

## 11. 交付产物（首批）

* Flutter 组件库：`GlassScaffold/GlassCard/GlassSidebar/GlassTabBar`（含动态收缩/渐进披露）。
* Rust Core API（FFI）与示例：Profile CRUD、Modrinth Search & Install、Mirror 切换、启动管线最小闭环。
* 配置/数据格式样例与迁移脚本。
* E2E 脚本：一键创建实例 → 安装（BMCLAPI）→ 拉取 Modrinth 模组 → 启动 → 输出诊断报告。

---

### 12. 语言与框架结论（确认）

* **核心语言：Rust**——并发下载、安装/校验、沙箱宿主与跨平台文件系统最合适。
* **UI：Flutter**——动画/Shader/跨平台一致性最佳，更易还原 LiquidGlass、动态收缩与内容分层体验。 

