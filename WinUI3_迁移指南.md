# SQLViewer WinUI 3 - 用户指南

## 概述

SQLViewer 已成功从 WinForms 重构为 WinUI 3 版本。所有原有功能均已保留，界面布局保持一致，并且已配置为可以在 Visual Studio 2022 中打包为 EXE 文件。

## 项目位置

- **原 WinForms 版本**: `SQLViewer/SQLViewer/` 目录
- **新 WinUI 3 版本**: `SQLViewerWinUI3/` 目录
- **解决方案文件**: `SQLViewerWinUI3.sln`

## 功能对照表

所有功能已完整迁移到 WinUI 3：

| 功能 | 原 WinForms | 新 WinUI 3 | 状态 |
|------|-------------|------------|------|
| 用户登录 | Form2 (FormLogin) | LoginDialog | ✅ 完成 |
| 主窗口 | Form1 | MainWindow | ✅ 完成 |
| 服务器选择 | ComboBox | ComboBox | ✅ 完成 |
| 数据库树形导航 | TreeView | TreeView | ✅ 完成 |
| 表数据查看 | TableViewer | TableViewer | ✅ 完成 |
| SQL 查询编辑器 | SqlEditer | SqlEditor | ✅ 完成 |
| 多标签页 | TabControl | TabView | ✅ 完成 |
| 字段自动补全 | ListBox | ListView | ✅ 完成 |
| 数据排序 | DataGridView | 自定义 Grid | ✅ 完成 |
| 分页查询 | 配置项 | 配置项 | ✅ 完成 |

## 在 Visual Studio 2022 中打开和构建

### 步骤 1: 安装必要组件

确保你的 Visual Studio 2022 已安装以下组件：

1. **.NET 桌面开发** 工作负载
2. **通用 Windows 平台开发** 工作负载
3. **Windows App SDK C# 模板** (可选组件)

如果没有安装，请运行 Visual Studio Installer 并添加这些组件。

### 步骤 2: 打开项目

1. 启动 **Visual Studio 2022**
2. 选择 **打开项目或解决方案**
3. 导航到项目目录，选择 `SQLViewerWinUI3.sln`
4. 点击 **打开**

### 步骤 3: 还原 NuGet 包

Visual Studio 会自动还原 NuGet 包。如果没有自动还原：

1. 右键点击解决方案
2. 选择 **还原 NuGet 包**

### 步骤 4: 选择目标平台

在工具栏中：
- **配置**: Release (用于发布) 或 Debug (用于调试)
- **平台**: x64 (推荐)、x86 或 ARM64

### 步骤 5: 构建项目

按 **Ctrl+Shift+B** 或选择 **生成 > 生成解决方案**

### 步骤 6: 运行项目

按 **F5** (调试模式) 或 **Ctrl+F5** (非调试模式)

## 打包为 EXE 文件

有两种打包方式：

### 方式 1: MSIX 包 (推荐 - Windows 10/11 原生)

这是 Windows 10/11 的推荐部署方式，支持自动更新和清洁卸载。

1. 在 Visual Studio 中右键点击 **SQLViewerWinUI3** 项目
2. 选择 **发布** > **创建应用程序包**
3. 选择发布目标：
   - **旁加载** - 用于企业内部分发
   - **Microsoft Store** - 用于公开发布
4. 选择 **是**，使用自动签名
5. 选择目标平台（建议选择 x64）
6. 配置包版本号和输出位置
7. 点击 **创建** 开始打包

完成后，MSIX 包将位于 `AppPackages` 文件夹中。

**安装 MSIX 包**:
- 双击 `.msixbundle` 文件或 `.msix` 文件
- 如果提示需要开发者模式，在 Windows 设置中启用
- 或使用 PowerShell: `Add-AppxPackage -Path "路径\App.msix"`

### 方式 2: 自包含发布 (传统 EXE)

如果需要传统的 EXE 文件（不需要 Windows Store 或 MSIX）：

1. 打开 **开发者 PowerShell** 或 **命令提示符**
2. 导航到项目目录:
   ```powershell
   cd SQLViewerWinUI3
   ```
3. 运行发布命令:
   ```powershell
   dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=false
   ```

输出位置: `bin\Release\net8.0-windows10.0.19041.0\win-x64\publish\`

**注意**: 这种方式会生成多个文件，你需要分发整个 `publish` 文件夹。主 EXE 文件是 `SQLViewerWinUI3.exe`。

### 方式 3: 单文件发布

如果想要单个 EXE 文件：

```powershell
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

这会创建一个较大的单个 EXE 文件，但更易于分发。

## 配置和数据存储

### WinForms 版本
- 使用 `app.config` / `ConfigurationManager`
- 配置存储在应用程序目录

### WinUI 3 版本
- 使用 `Windows.Storage.ApplicationData`
- 配置存储在用户的 LocalState 文件夹
- 位置: `%LOCALAPPDATA%\Packages\SQLViewerWinUI3_xxx\LocalState\`

**迁移配置**: 配置不会自动迁移。用户首次启动时需要重新登录。

## 主要技术变更

### UI 框架
- **WinForms 控件** → **WinUI 3 / XAML 控件**
- **Form** → **Window**
- **UserControl** → **UserControl** (XAML)
- **MessageBox** → **ContentDialog**

### 数据显示
- **DataGridView** → 自定义 **Grid** 布局
  - 支持列排序
  - 支持动态数据绑定
  - 性能优化的渲染

### 编辑器
- **RichTextBox** → **RichEditBox**
- 保留了文本编辑和格式化功能

### HTTP 客户端
- **HttpWebRequest** → **HttpClient**
- 更现代的异步 API

## 已知差异和限制

1. **SQL 自动补全**: 基础功能已实现，复杂的语法分析功能已简化
2. **图标和图像**: 当前使用占位符图像，可以替换为实际的品牌图标
3. **DataGrid 性能**: 对于超大数据集（>10000 行），建议使用分页
4. **双击打开表**: WinUI 3 TreeView 使用单击激活，与双击效果相同

## 性能优化建议

1. **分页**: 继续使用分页功能，避免一次加载过多数据
2. **虚拟化**: 大数据集时考虑启用虚拟化（未来优化）
3. **异步操作**: 所有网络请求都是异步的，不会阻塞 UI

## 故障排除

### 构建失败: "找不到 Windows SDK"
- 确保安装了 **Windows 10 SDK** (版本 17763 或更高)
- 在 Visual Studio Installer 中安装

### 运行时错误: "应用程序无法启动"
- 确保目标机器是 **Windows 10 17763** 或更高版本
- 检查是否安装了 **Windows App Runtime**

### 包签名错误
- 使用开发者自签名证书
- 或在设置中启用 **开发者模式**

### 配置丢失
- WinUI 3 版本使用独立的配置存储
- 首次运行时需要重新登录

## 后续改进建议

1. **添加真实图标**: 替换 `Assets` 文件夹中的占位符图像
2. **主题支持**: 添加浅色/深色主题切换
3. **键盘快捷键**: 实现更多快捷键支持
4. **SQL 语法高亮**: 增强 SQL 编辑器的语法高亮
5. **导出功能**: 添加数据导出为 CSV/Excel 功能

## 技术支持

如有问题或建议，请参考:
- 项目 README: `SQLViewerWinUI3/README.md`
- WinUI 3 文档: https://learn.microsoft.com/windows/apps/winui/
- Windows App SDK: https://learn.microsoft.com/windows/apps/windows-app-sdk/

## 总结

SQLViewer 已成功从 WinForms 迁移到 WinUI 3，保持了所有核心功能和用户界面布局。新版本利用了现代 Windows UI 框架的优势，提供了更好的性能和更现代的外观。

项目已配置完成，可以在 Visual Studio 2022 中直接打开、构建和打包为 EXE 文件。
