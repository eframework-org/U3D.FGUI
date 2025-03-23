# UIUtility

[![Version](https://img.shields.io/github/package-json/v/eframework-org/U3D.FGUI)](https://github.com/eframework-org/U3D.FGUI)
[![Downloads](https://img.shields.io/github/downloads/eframework-org/U3D.FGUI/total)](https://github.com/eframework-org/U3D.FGUI/releases)  

UIUtility 是 FairyGUI 的工具集类，提供了一系列简化 UI 组件操作的扩展方法，使 FairyGUI 组件更易于使用。

## 功能特性

- 快速索引功能：通过名称或路径快速获取 UICanvas 和 GComponent 中的子组件
- 显示状态控制：提供简便的方法控制 UI 组件的显示和隐藏
- 扩展方法设计：采用扩展方法设计，使用更符合直觉
- 链式调用支持：支持方法的链式调用，提高代码的简洁性

## 使用手册

### 1. 索引操作

#### 1.1 UICanvas 索引

通过 UICanvas 获取子组件：

```csharp
// 获取 UICanvas
var canvas = FindObjectOfType<UICanvas>();

// 通过路径获取按钮组件
var loginBtn = canvas.Index<GButton>("loginPanel.loginBtn");
```

#### 1.2 GComponent 索引

通过 GComponent 获取子组件：

```csharp
// 获取 GComponent
var panel = canvas.ui.GetChild("mainPanel").asCom;

// 通过名称获取按钮
var okBtn = panel.Index<GButton>("okBtn");
```

### 2. 显示状态控制

#### 2.1 设置组件显示状态

控制 UI 对象的显示状态：

```csharp
// 获取 GObject
var obj = panel.GetChild("notification");

// 显示组件
obj.SetActiveState(true);

// 隐藏组件
obj.SetActiveState(false);

// 链式调用
canvas.Index<GButton>("loginBtn")?.SetActiveState(true);
```

#### 2.2 设置容器显示状态

控制容器及其子对象的显示状态：

```csharp
// 获取容器
var container = panel.GetChild("container").asCom;

// 显示整个容器
container.SetActiveState(true);

// 隐藏整个容器
container.SetActiveState(false);
```

#### 2.3 设置子对象显示状态

通过路径控制子对象的显示状态：

```csharp
// 获取容器
var panel = canvas.ui.GetChild("mainPanel").asCom;

// 通过路径显示子对象
panel.SetActiveState("header.logo", true);

// 通过路径隐藏子对象
panel.SetActiveState("footer.copyright", false);
```

### 3. 综合示例

#### 3.1 登录界面示例

使用 UIUtility 操作登录界面：

```csharp
// 获取 UICanvas
var canvas = FindObjectOfType<UICanvas>();

// 获取登录面板
var loginPanel = canvas.Index<GComponent>("loginPanel");

// 设置面板可见
loginPanel.SetActiveState(true);

// 获取并操作输入框
var usernameInput = loginPanel.Index<GTextInput>("usernameInput");
var passwordInput = loginPanel.Index<GTextInput>("passwordInput");

// 获取并操作按钮
var loginBtn = loginPanel.Index<GButton>("loginBtn");
var registerBtn = loginPanel.Index<GButton>("registerBtn");

// 获取提示信息
var errorText = loginPanel.Index<GTextField>("errorText");
// 初始隐藏错误信息
errorText.SetActiveState(false);

// 在登录失败时显示错误
void OnLoginFailed(string errorMessage)
{
    errorText.text = errorMessage;
    errorText.SetActiveState(true);
}
```

## 常见问题

### 1. 找不到子对象

如果使用 Index 方法无法找到子对象：
- 检查路径名称是否正确（区分大小写）
- 确认子对象确实存在于指定的路径下
- 验证泛型类型是否与实际组件类型匹配

### 2. SetActiveState 不生效

如果 SetActiveState 方法不生效：
- 确认对象引用不为 null
- 检查是否有其他代码覆盖了可见性设置
- 对于嵌套组件，确保父容器也是可见的

### 3. 类型转换错误

使用 Index 方法时出现类型转换错误：
- 确保使用正确的类型参数，例如 GButton 而不是 Button
- 如果不确定组件类型，可以先使用 GObject 类型获取，再进行适当的转换
- 对于自定义组件，确保实现了正确的继承关系

更多问题，请查阅[问题反馈](../CONTRIBUTING.md#问题反馈)。

## 项目信息

- [更新记录](../CHANGELOG.md)
- [贡献指南](../CONTRIBUTING.md)
- [许可证](../LICENSE.md)
