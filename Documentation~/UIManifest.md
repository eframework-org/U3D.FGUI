# UIManifest

[![Version](https://img.shields.io/github/package-json/v/eframework-org/U3D.FGUI)](https://github.com/eframework-org/U3D.FGUI)
[![Downloads](https://img.shields.io/github/downloads/eframework-org/U3D.FGUI/total)](https://github.com/eframework-org/U3D.FGUI/releases)  

UIManifest 是 FairyGUI 的 UI 资源清单组件，用于管理 UI 包资源及其依赖关系，简化 FairyGUI 资源的导入和使用流程。

## 功能特性

- 存储包资源信息：记录 FairyGUI 导出文档路径、包名称和路径，便于运行时加载
- 管理依赖关系：存储和管理 UI 包之间的依赖引用，确保资源按正确顺序加载
- 自动资源处理：支持通过编辑器工具自动导入和更新 UI 资源
- 与 UICanvas 集成：为 UICanvas 提供资源引用和依赖管理支持

## 使用手册

### 1. 创建与导入

#### 1.1 创建 UI 资源清单

通过编辑器菜单创建 UIManifest：

```csharp
// 通过菜单创建
// 1. 在 Project 窗口中选择目标文件夹
// 2. 右键选择 "Create/FairyGUI/UI Manifest"
// 3. 选择包含 FairyGUI 导出文件的文档目录
```

### 2. 资源管理

#### 2.1 资源路径设置

UIManifest 的主要属性：

```csharp
// 获取现有的 UIManifest
UIManifest manifest = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/YourUI/Package.prefab").GetComponent<UIManifest>();

// 设置 FairyGUI 导出路径
manifest.DocsPath = "Assets/YourUI/ExportedDocs";

// 包名称和路径通常由导入过程自动设置
// 但也可以手动设置
manifest.PackageName = "YourPackage";
manifest.PackagePath = "Assets/YourUI/Package";

// 保存修改
EditorUtility.SetDirty(manifest.gameObject);
AssetDatabase.SaveAssets();
```

#### 2.2 依赖管理

UIManifest 自动处理依赖关系：

```csharp
// 查看依赖列表
foreach (var dependency in manifest.Dependency)
{
    if (dependency is GameObject)
    {
        UIManifest depManifest = (dependency as GameObject).GetComponent<UIManifest>();
        Debug.Log($"依赖包: {depManifest.PackageName}");
    }
}

// 依赖关系会在导入过程中自动建立
// 无需手动设置
```

### 3. 与 UICanvas 配合使用

#### 3.1 资源引用

在 UICanvas 中引用 UIManifest：

```csharp
// 获取 UICanvas
UICanvas canvas = FindObjectOfType<UICanvas>();

// 设置 UIManifest 引用
canvas.packageMani = manifest;

// UICanvas 会在初始化时自动加载 UIManifest 及其依赖
```

#### 3.2 编辑器集成

在编辑器模式下，UIManifest 在 Project 窗口中显示自定义图标：

```csharp
// UIManifest 的自定义图标由 UIManifestEditor 处理
// 无需手动设置
```

## 常见问题

### 1. 导入失败

如果 UIManifest 导入失败，请检查：
- DocsPath 是否正确指向 FairyGUI 的导出目录
- 导出目录中是否包含有效的 FairyGUI 文件（如 _fui.bytes 文件）
- 是否有足够的权限访问目录和文件

### 2. 依赖关系错误

如果出现依赖关系问题：
- 检查是否存在循环依赖（A 依赖 B，B 又依赖 A）
- 确认所有依赖的 UIManifest 都已正确创建和导入
- 重新导入所有相关的 UIManifest 以更新依赖关系

### 3. 资源更新问题

更新 FairyGUI 资源后：
- 确保重新导出 FairyGUI 文件到指定的 DocsPath
- 在 Unity 中重新导入 UIManifest（可通过右键 UIManifest 并选择 "Reimport"）
- 如果自动导入没有触发，可以手动调用 UIManifestEditor.Import() 方法

更多问题，请查阅[问题反馈](../CONTRIBUTING.md#问题反馈)。

## 项目信息

- [更新记录](../CHANGELOG.md)
- [贡献指南](../CONTRIBUTING.md)
- [许可证](../LICENSE.md)
