// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System.Collections.Generic;
using UnityEngine;

namespace EFramework.FairyGUI
{
    /// <summary>
    /// UIManifest 实现了 FairyGUI 导出素材的清单管理功能，用于控制 UI 包资源及其依赖关系。
    /// </summary>
    /// <remarks>
    /// <code>
    /// 功能特性
    /// - 存储素材清单：记录 UI 包的名称、路径、素材列表等信息
    /// - 依赖关系管理：存储和管理 UI 包之间的依赖引用
    /// - 自动导入流程：监听资源导入事件触发自动化导入流程
    /// 
    /// 使用手册
    /// 1. 创建清单
    /// 
    /// 通过编辑器创建清单：
    /// 
    ///     1. 在 Project 窗口中选择目标文件夹
    ///     2. 右键 Create/FairyGUI/UI Manifest
    ///     3. 选择 FairyGUI 导出文件的素材目录
    /// 
    /// 2. 资源导入
    /// 
    /// 支持自动和手动两种方式导入清单：
    /// 
    ///     1. 监听资源导入事件触发自动化导入流程
    ///     2. 右键 UIManifest 资源并 Reimport
    /// </code>
    /// 更多信息请参考模块文档。
    /// </remarks>
    [AddComponentMenu("FairyGUI/UI Manifest")]
    public class UIManifest : MonoBehaviour
    {
        /// <summary>
        /// RawPath 是 FairyGUI 导出素材的路径，用于导入和更新 UI 资源清单。
        /// </summary>
        public string RawPath;

        /// <summary>
        /// PackageName 是 UI 包的名称，与 FairyGUI 中定义的包名一致。
        /// </summary>
        public string PackageName;

        /// <summary>
        /// PackagePath 是 UI 包在项目中的路径，用于运行时加载。
        /// </summary>
        public string PackagePath;

        /// <summary>
        /// Dependency 是 UI 包的依赖列表，包含其他 UIManifest 预制体和资源引用。
        /// </summary>
        public List<Object> Dependency;
    }
}
