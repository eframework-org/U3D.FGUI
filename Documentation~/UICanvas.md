# UICanvas

[![Version](https://img.shields.io/npm/v/org.eframework.u3d.fgui)](https://www.npmjs.com/package/org.eframework.u3d.fgui)
[![Downloads](https://img.shields.io/npm/dm/org.eframework.u3d.fgui)](https://www.npmjs.com/package/org.eframework.u3d.fgui)

UICanvas 拓展了 UIPanel 组件的功能，提供包资源的自动加载和依赖管理功能。

## 功能特性

- 依赖关系管理：自动处理包之间的依赖关系，确保资源按正确的顺序加载
- 组件快速索引：可通过名称或路径快速获取子对象，简化对象查找过程

## 使用手册

### 1. 挂载组件

通过编辑器挂载组件：

1. 点击 AddComponentMenu 按钮
2. 选择 "FairyGUI → UI Canvas"
3. 在 Inspector 中设置相关属性

### 2. 依赖加载

允许自定义包资源依赖的加载：

```csharp
// 设置全局的自定义加载器
UICanvas.Loader = (canvas) => {
    // 自定义加载逻辑
    Debug.Log($"正在加载包：{canvas.packageName}");
};
```

### 3. 快速索引

通过名称或路径获取 UI 组件：

```csharp
// 获取按钮组件
GButton button = canvas.Index<GButton>("panel.loginButton");

// 获取文本组件
GTextField text = canvas.Index<GTextField>("panel.welcomeText");

// 获取列表组件
GList list = canvas.Index<GList>("panel.itemList");
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
