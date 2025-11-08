# Authentication Capability Specification

## ADDED Requirements

### Requirement: Microsoft Account Login
系统应支持Microsoft账户登录，使用设备代码流程，并安全存储刷新令牌。

#### Scenario: Successful Microsoft login
- **WHEN** 用户选择Microsoft登录
- **THEN** 系统显示设备代码和用户代码
- **AND** 用户可以在Microsoft网站完成认证
- **AND** 系统获取并安全存储访问令牌和刷新令牌

#### Scenario: Token refresh
- **WHEN** 访问令牌过期
- **THEN** 系统自动使用刷新令牌获取新的访问令牌
- **AND** 更新本地存储的令牌

### Requirement: Offline Account Login
系统应支持离线账户登录，允许用户使用任意用户名进行本地游戏。

#### Scenario: Create offline account
- **WHEN** 用户选择离线登录并输入用户名
- **THEN** 系统创建本地离线账户
- **AND** 显示"受限功能"提示

### Requirement: LittleSkin Account Login
系统应支持LittleSkin（Yggdrasil兼容）账户登录，允许用户配置认证服务器。

#### Scenario: Configure LittleSkin server
- **WHEN** 用户输入LittleSkin服务器地址和凭据
- **THEN** 系统验证服务器兼容性
- **AND** 支持换肤资料同步（需用户授权）

### Requirement: Multi-Account Management
系统应支持多个账户并存和快速切换。

#### Scenario: Account switching
- **WHEN** 用户添加多个账户
- **THEN** 系统显示账户列表
- **AND** 允许一键切换当前活动账户

### Requirement: Secure Token Storage
系统应使用平台原生安全存储机制保护用户凭据。

#### Scenario: Windows credential storage
- **WHEN** 在Windows系统上
- **THEN** 使用Windows Credential Manager存储令牌

#### Scenario: macOS keychain storage
- **WHEN** 在macOS系统上
- **THEN** 使用macOS Keychain存储令牌

#### Scenario: Linux secret service
- **WHEN** 在Linux系统上
- **THEN** 使用Secret Service API存储令牌
