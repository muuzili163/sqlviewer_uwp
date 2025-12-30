# SQLViewer WinUI 3

这是SQLViewer应用程序的WinUI 3版本，从WinForms重构而来。

## 功能

所有功能与原WinForms版本保持一致：

- ✅ 用户登录
- ✅ 服务器和数据库选择
- ✅ 数据库和表的树形导航
- ✅ 表数据查看和查询
- ✅ 自定义SQL查询编辑器
- ✅ 字段名自动补全
- ✅ 数据排序和过滤
- ✅ 多标签页支持
- ✅ 分页查询

## 技术栈

- **.NET 8.0**
- **WinUI 3** (Windows App SDK 1.6)
- **Newtonsoft.Json** 用于JSON处理
- **Windows 10** (版本 17763 或更高)

## 项目结构

```
SQLViewerWinUI3/
├── App.xaml                    # 应用程序入口
├── App.xaml.cs
├── MainWindow.xaml             # 主窗口 (对应 Form1)
├── MainWindow.xaml.cs
├── Package.appxmanifest        # MSIX 包清单
├── Helpers/
│   ├── ConfigHelper.cs         # 配置管理
│   └── HttpUtils.cs            # HTTP 工具类
└── Views/
    ├── LoginDialog.cs          # 登录对话框 (对应 Form2)
    ├── TableViewer.xaml        # 表查看器 (对应 TableViewer)
    ├── TableViewer.xaml.cs
    ├── SqlEditor.xaml          # SQL编辑器 (对应 SqlEditer)
    └── SqlEditor.xaml.cs
```

## 如何构建

### 前置要求

1. **Windows 10/11** (版本 17763 或更高)
2. **Visual Studio 2022** (版本 17.0 或更高)，需要安装：
   - .NET 桌面开发工作负载
   - 通用 Windows 平台开发工作负载
   - Windows App SDK C# 模板

### 在 Visual Studio 2022 中构建

1. 打开 `SQLViewerWinUI3.sln`
2. 选择目标平台（x64 推荐）
3. 按 F5 运行或 Ctrl+Shift+B 构建

### 打包为 EXE

有两种方式将应用程序打包为可执行文件：

#### 方法 1: MSIX 包（推荐）

1. 右键点击 `SQLViewerWinUI3` 项目
2. 选择 "发布" -> "创建应用程序包"
3. 选择 "旁加载" 或 "Microsoft Store"
4. 按照向导完成打包
5. 输出将在 `AppPackages` 文件夹中

#### 方法 2: 自包含部署

在项目目录中运行：

```powershell
dotnet publish -c Release -r win-x64 --self-contained
```

输出将在 `bin\Release\net8.0-windows10.0.19041.0\win-x64\publish\` 文件夹中。

## 主要变更

### UI 框架
- **WinForms** → **WinUI 3 / XAML**
- **DataGridView** → 自定义 Grid 布局
- **RichTextBox** → **RichEditBox**

### 配置管理
- **ConfigurationManager** → **Windows.Storage.ApplicationData**

### 对话框
- **MessageBox** → **ContentDialog**
- **Form.ShowDialog()** → **ContentDialog.ShowAsync()**

### 布局
- 保持与原WinForms版本相同的布局和用户体验
- 使用 XAML 的 Grid、StackPanel 等布局容器

## 已知限制

- SQL 自动补全功能已实现基础版本，不包含完整的语法分析
- 图标资源需要替换为实际的PNG图像
- 某些高级WinForms控件被简化为WinUI 3等效实现

## 下一步

如需添加更多功能：

1. 完善 SQL 编辑器的语法高亮
2. 添加更多的键盘快捷键
3. 改进数据网格的性能（使用虚拟化）
4. 添加主题支持（浅色/深色模式）

## 许可证

与原项目保持一致
