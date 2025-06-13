# UIManifest

[![Version](https://img.shields.io/npm/v/org.eframework.u3d.fgui)](https://www.npmjs.com/package/org.eframework.u3d.fgui)
[![Downloads](https://img.shields.io/npm/dm/org.eframework.u3d.fgui)](https://www.npmjs.com/package/org.eframework.u3d.fgui)
[![DeepWiki](https://img.shields.io/badge/DeepWiki-Explore-blue)](https://deepwiki.com/eframework-org/U3D.FGUI)

UIManifest 实现了 FairyGUI 导出素材的清单管理功能，用于控制 UI 包资源及其依赖关系。

## 功能特性

- 存储素材清单：记录 UI 包的名称、路径、素材列表等信息
- 依赖关系管理：存储和管理 UI 包之间的依赖引用
- 自动导入流程：监听资源导入事件触发自动化导入流程

## 使用手册

### 1. 创建清单

通过编辑器创建清单：

1. 在 `Project` 窗口中选择目标文件夹
2. 右键 `Create/FairyGUI/UI Manifest`
3. 选择 `FairyGUI` 导出文件的素材目录
4. 编辑器将自动创建 `UIManifest` 预制体

### 2. 资源导入

支持自动和手动两种方式导入清单：

1. 监听资源导入事件触发自动化导入流程
2. 右键 `UIManifest` 资源并 `Reimport`

## 常见问题

### 1. 资源导入失败

如果 UIManifest 导入失败，请检查：
- RawPath 是否正确指向 FairyGUI 的导出目录
- 导出目录中是否包含有效的 FairyGUI 文件（如 _fui.bytes 文件）
- 检查是否存在循环依赖（A 依赖 B，B 又依赖 A）

### 2. 文件监听导入失败

- 问题现象：有概率出现 Unity Editor 资源导入失败：Could not create asset from Assets/xxxx: File could not be read 或出现导入两次的情况
- 问题原因：FairyGUI Editor 未完全 Flush 文件或者外部的操作（Git 更新）引起文件变更导致 dirty 监控不正确
- 解决方案：重新发布/导入 UIManifest 资源

更多问题，请查阅[问题反馈](../CONTRIBUTING.md#问题反馈)。

## 项目信息

- [更新记录](../CHANGELOG.md)
- [贡献指南](../CONTRIBUTING.md)
- [许可证](../LICENSE.md)
