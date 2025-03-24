// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using FairyGUI;
using EFramework.Utility;

namespace EFramework.FairyGUI
{
    /// <summary>
    /// UIUtility 是一个 FairyGUI 的工具函数集，提供了一系列简化 UI 组件操作的扩展方法。
    /// </summary>
    /// <remarks>
    /// <code>
    /// 功能特性
    /// - 快速索引功能：通过名称或路径快速获取 UI 组件的子对象
    /// - 显示状态控制：提供简便的方法控制 UI 组件的显示和隐藏
    /// - 扩展方法支持：采用扩展方法设计，函数调用更为直观
    /// 
    /// 使用手册
    /// 1. 索引操作
    /// 
    /// 1.1 UICanvas 索引
    /// 
    ///     // 获取 UICanvas
    ///     var canvas = FindObjectOfType&lt;UICanvas&gt;();
    ///     
    ///     // 通过路径获取按钮组件
    ///     var loginBtn = canvas.Index&lt;GButton&gt;("loginPanel.loginBtn");
    ///     
    /// 1.2 GComponent 索引
    /// 
    ///     // 获取 GComponent
    ///     var panel = canvas.ui.GetChild("mainPanel").asCom;
    ///     
    ///     // 通过名称获取按钮
    ///     var okBtn = panel.Index&lt;GButton&gt;("okBtn");
    /// 
    /// 2. 状态控制
    /// 
    /// 2.1 设置组件显示状态
    /// 
    ///     // 获取 GObject
    ///     var obj = panel.GetChild("notification");
    ///     
    ///     // 显示组件
    ///     obj.SetActiveState(true);
    ///     
    ///     // 隐藏组件
    ///     obj.SetActiveState(false);
    ///     
    ///     // 链式调用
    ///     canvas.Index&lt;GButton&gt;("loginBtn")?.SetActiveState(true);
    ///     
    /// 2.2 设置容器显示状态
    /// 
    ///     // 获取容器
    ///     var container = panel.GetChild("container").asCom;
    ///     
    ///     // 显示整个容器
    ///     container.SetActiveState(true);
    ///     
    ///     // 隐藏整个容器
    ///     container.SetActiveState(false);
    ///     
    /// 2.3 设置子对象显示状态
    /// 
    ///     // 获取容器
    ///     var panel = canvas.ui.GetChild("mainPanel").asCom;
    ///     
    ///     // 通过路径显示子对象
    ///     panel.SetActiveState("header.logo", true);
    ///     
    ///     // 通过路径隐藏子对象
    ///     panel.SetActiveState("footer.copyright", false);
    /// </code>
    /// 更多信息请参考模块文档。
    /// </remarks>
    public static class UIUtility
    {
        /// <summary>
        /// 通过名称或路径获取 UICanvas 中的指定类型组件。
        /// </summary>
        /// <typeparam name="T">要获取的组件类型</typeparam>
        /// <param name="panel">UICanvas 实例</param>
        /// <param name="name">组件名称或路径</param>
        /// <returns>找到的组件实例，未找到则返回 null</returns>
        public static T Index<T>(this UICanvas panel, string name) where T : class { if (panel) return XComp.Index<T>(panel.gameObject, name); else return null; }

        /// <summary>
        /// 通过名称或路径获取 GComponent 中的指定类型组件。
        /// </summary>
        /// <typeparam name="T">要获取的组件类型</typeparam>
        /// <param name="comp">GComponent 实例</param>
        /// <param name="name">组件名称或路径</param>
        /// <returns>找到的组件实例，未找到则返回 null</returns>
        public static T Index<T>(this GComponent comp, string name) where T : class { if (comp != null) return comp.GetChildByPath(name) as T; else return null; }

        /// <summary>
        /// 设置 UI 对象的显示状态。
        /// </summary>
        /// <param name="rootObj">UI 对象</param>
        /// <param name="active">是否显示，true 为显示，false 为隐藏</param>
        public static void SetActiveState(this GObject rootObj, bool active) { rootObj.visible = active; }

        /// <summary>
        /// 设置 UI 容器的显示状态。
        /// </summary>
        /// <param name="rootObj">UI 容器</param>
        /// <param name="active">是否显示，true 为显示，false 为隐藏</param>
        public static void SetActiveState(this GComponent rootObj, bool active) { rootObj.visible = active; }

        /// <summary>
        /// 设置 UI 容器中指定路径子对象的显示状态。
        /// </summary>
        /// <param name="rootObj">UI 容器</param>
        /// <param name="path">子对象路径</param>
        /// <param name="active">是否显示，true 为显示，false 为隐藏</param>
        public static void SetActiveState(this GComponent rootObj, string path, bool active) { rootObj.GetChildByPath(path).visible = active; }
    }
}
