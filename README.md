# SQLViewer UWP (WPF Version)

SQLViewer的WPF版本 - 一个用于互联网SQL审计的桌面应用程序

## 项目说明

这是SQLViewer从WinForm重构为现代WPF (Windows Presentation Foundation) 应用程序的版本。WPF使用XAML进行UI设计，提供了类似UWP的现代化界面体验。

## 功能特性

### 核心功能
- ✅ 用户登录认证
- ✅ 服务器实例选择
- ✅ 数据库/表的树形浏览
- ✅ 多标签页支持
- ✅ 表数据查看器
  - 数据网格显示
  - 列排序
  - 条件过滤
  - 字段自动补全
  - 表结构查看
  - 总数计算
- ✅ SQL查询编辑器
  - 多数据库支持
  - SQL语法提示
  - 智能自动补全
  - 上下文感知建议
  - Ctrl+Enter执行查询
  - Ctrl+Space手动触发建议

### 技术亮点
- 异步HTTP通信 (使用HttpClient)
- JSON配置存储
- 智能SQL上下文分析
- 元数据缓存机制
- 响应式UI设计

## 系统要求

- Windows 10/11
- .NET 8.0 SDK
- Visual Studio 2022 (可选，用于开发)

## 构建项目

```bash
# 克隆仓库
git clone https://github.com/muuzili163/sqlviewer_uwp.git
cd sqlviewer_uwp

# 还原NuGet包
cd SQLViewerUWP
dotnet restore

# 构建项目
dotnet build

# 运行应用程序
dotnet run
```

## 使用说明

### 1. 登录
首次启动应用程序时，会提示您登录。请向项目管理员获取测试凭据。

### 2. 浏览数据库
1. 从顶部下拉框选择服务器实例
2. 在左侧树形视图中展开数据库节点
3. 双击表名打开表查看器

### 3. 查看表数据
- **查询**: 在条件框输入WHERE条件，点击"查询"按钮
- **排序**: 点击列标题进行排序，再次点击切换升序/降序
- **自动补全**: 在条件框输入时，会自动提示字段名
- **表结构**: 点击"表结构"按钮查看DDL
- **计算总数**: 点击"计算总数"获取表的总记录数

### 4. 自定义SQL查询
1. 点击顶部"新建查询"按钮
2. 选择数据库和表
3. 在编辑器中输入SQL语句
4. 按 **Ctrl+Enter** 执行查询
5. 按 **Ctrl+Space** 手动触发代码建议

### 5. SQL自动补全功能
- 输入 `.` 后自动显示表的字段
- 在 `FROM` 后显示表名建议
- 在 `SELECT` 后显示列名建议
- 在 `WHERE`/`ON` 后显示带别名的字段
- 使用上下箭头选择建议
- 按 **Enter** 或 **Tab** 接受建议
- 按 **Esc** 关闭建议列表

## 项目结构

```
SQLViewerUWP/
├── App.xaml                    # 应用程序入口
├── MainWindow.xaml             # 主窗口(等同于Form1)
├── LoginDialog.xaml            # 登录对话框(等同于FormLogin)
├── TableViewerControl.xaml    # 表查看器控件(等同于TableViewer)
├── SqlEditorControl.xaml      # SQL编辑器控件(等同于SqlEditer)
├── ConfigHelper.cs             # 配置管理(JSON存储)
├── HttpUtils.cs                # HTTP通信工具类
└── Editor/                     # SQL编辑器支持类
    ├── SuggestType.cs          # 建议类型枚举
    ├── SuggestItem.cs          # 建议项
    ├── SqlClause.cs            # SQL子句枚举
    ├── SqlContext.cs           # SQL上下文
    ├── SqlContextAnalyzer.cs   # SQL上下文分析器
    ├── ISqlMetadataProvider.cs # 元数据提供者接口
    ├── DefaultSqlMetadataProvider.cs # 默认元数据提供者
    ├── ISqlSuggestResolver.cs  # 建议解析器接口
    └── SqlSuggestResolver.cs   # 建议解析器实现
```

## 与WinForm版本的区别

| 特性 | WinForm版 | WPF版 |
|------|-----------|-------|
| UI框架 | Windows Forms | WPF (XAML) |
| HTTP库 | HttpWebRequest | HttpClient |
| 配置存储 | App.config | JSON文件 |
| 异步模式 | Task | async/await |
| UI响应性 | 一般 | 优秀 |
| 可扩展性 | 有限 | 高 |

## 开发说明

### 添加新功能
1. 在XAML中设计UI
2. 在代码后置文件中实现逻辑
3. 使用async/await进行异步操作
4. 更新本README

### 调试
在Visual Studio中按F5启动调试，或使用：
```bash
dotnet run
```

### 发布
```bash
dotnet publish -c Release -r win-x64 --self-contained
```

## 已知问题和限制

- 需要网络连接到 https://sql-out.sdcreditech.com
- 配置文件存储在 `%LocalAppData%\SQLViewerUWP\config.json`

## 贡献

欢迎提交Issue和Pull Request！

## 许可证

与原WinForm版本相同

## 更新日志

### v2.0.0 (WPF版本)
- ✅ 完整重构为WPF应用
- ✅ 实现所有WinForm版功能
- ✅ 添加智能SQL提示系统
- ✅ 优化异步处理性能
- ✅ 现代化UI设计

### v1.0.0 (WinForm版本)
- 原始WinForm实现
