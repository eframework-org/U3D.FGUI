# UICanvas

[![Version](https://img.shields.io/github/package-json/v/eframework-org/U3D.FGUI)](https://github.com/eframework-org/U3D.FGUI)
[![Downloads](https://img.shields.io/github/downloads/eframework-org/U3D.FGUI/total)](https://github.com/eframework-org/U3D.FGUI/releases)  

UICanvas 是 FairyGUI 的 UI 画布组件，提供包资源的自动加载和依赖管理功能，简化 FairyGUI 在 Unity 中的使用流程。

## 功能特性

- 自动加载包资源：在组件初始化时自动加载指定的 FairyGUI 包资源，无需手动调用加载代码
- 管理包依赖关系：自动处理包之间的依赖关系，确保资源按正确的顺序加载
- 提供组件索引功能：可通过名称或路径快速获取子对象，简化对象查找过程
- 区分运行模式处理：针对编辑器模式和运行时模式提供不同的行为逻辑

## 使用手册

### 1. 基本配置

#### 1.1 创建 UI 画布

在场景中创建 UI 画布的步骤：

```csharp
// 1. 在场景中创建一个 GameObject
GameObject canvasObject = new GameObject("UICanvas");

// 2. 添加 UICanvas 组件
UICanvas canvas = canvasObject.AddComponent<UICanvas>();

// 3. 设置包名称和路径
canvas.packageName = "YourPackageName";
canvas.packagePath = "Assets/YourPackagePath";

// 4. 设置包资源清单引用
canvas.packageMani = yourUIManifestReference;
```

也可以通过 Unity 编辑器直接创建：
1. 在 Hierarchy 窗口右键点击
2. 选择 "FairyGUI → UI Canvas"
3. 在 Inspector 中设置相关属性

#### 1.2 自定义加载逻辑

UICanvas 允许自定义包资源的加载逻辑：

```csharp
// 设置自定义加载器
UICanvas.Loader = (canvas) => {
    // 自定义加载逻辑
    Debug.Log($"正在加载包：{canvas.packageName}");
    
    // 例如，使用异步方式加载
    StartCoroutine(AsyncLoadPackage(canvas.packageMani));
    
    // 或使用资源管理系统加载
    ResourceManager.LoadPackage(canvas.packagePath);
};
```

### 2. 运行时访问

#### 2.1 快速索引子对象

通过名称或路径获取 UI 组件：

```csharp
// 获取按钮组件
GButton button = canvas.Index<GButton>("panel.loginButton");

// 获取文本组件
GTextField text = canvas.Index<GTextField>("panel.welcomeText");

// 获取列表组件
GList list = canvas.Index<GList>("panel.itemList");
```

#### 2.2 与 UIManifest 配合使用

UICanvas 可与 UIManifest 配合使用：

```csharp
// 加载包资源及其依赖
UIManifest manifest = Resources.Load<GameObject>("YourManifest").GetComponent<UIManifest>();
canvas.packageMani = manifest;

// 在 Awake 中会自动加载包及其依赖
```

## 常见问题

### 1. 包资源未正确加载

如果包资源未能正确加载，请检查：
- packageName 和 packagePath 是否正确设置
- packageMani 引用是否有效
- 是否有循环依赖问题

### 2. 子对象无法索引

如果无法通过 Index 方法找到子对象，请检查：
- 路径名称是否正确（区分大小写）
- 组件类型是否匹配
- UI 是否已经初始化完成

更多问题，请查阅[问题反馈](../CONTRIBUTING.md#问题反馈)。

## 项目信息

- [更新记录](../CHANGELOG.md)
- [贡献指南](../CONTRIBUTING.md)
- [许可证](../LICENSE.md)
