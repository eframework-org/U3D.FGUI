// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System.Collections.Generic;
using UnityEngine;

namespace EFramework.FairyGUI
{
    /// <summary>
    /// FairyGUI 的 UI 资源清单组件，管理 UI 包资源及其依赖关系。
    /// </summary>
    /// <remarks>
    /// <code>
    /// 功能特性
    /// - 存储 FairyGUI 导出文档的路径信息
    /// - 记录 UI 包名称和路径
    /// - 管理 UI 包的依赖关系
    /// - 与 UICanvas 配合使用，支持运行时资源加载
    /// 
    /// 使用手册
    /// 1. 基本用法
    /// 
    /// 1.1 创建 UI 资源清单
    /// 
    ///     在 Project 窗口中选择目标文件夹，右键菜单选择 "Create/FairyGUI/UI Manifest"
    ///     选择包含 FairyGUI 导出文件的文档目录，系统将自动创建 UIManifest 预制体
    /// 
    /// 1.2 配合 UICanvas 使用
    /// 
    ///     在 UICanvas 组件中引用对应的 UIManifest 预制体
    ///     系统将自动加载 UI 包及其依赖
    /// 
    /// </code>
    /// </remarks>
    [AddComponentMenu("FairyGUI/UI Manifest")]
    public class UIManifest : MonoBehaviour
    {
        /// <summary>
        /// FairyGUI 导出文档的路径，用于导入和更新 UI 资源。
        /// </summary>
        public string DocsPath;

        /// <summary>
        /// UI 包的名称，与 FairyGUI 中定义的包名一致。
        /// </summary>
        public string PackageName;

        /// <summary>
        /// UI 包在项目中的路径，用于运行时加载。
        /// </summary>
        public string PackagePath;

        /// <summary>
        /// UI 包的依赖列表，包含其他 UIManifest 预制体和资源引用。
        /// </summary>
        public List<Object> Dependency;
    }
}
