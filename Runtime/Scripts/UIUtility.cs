// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using FairyGUI;
using EFramework.Utility;

namespace EFramework.FairyGUI
{
    /// <summary>
    /// FairyGUI 工具集，提供 UI 组件操作的扩展方法。
    /// </summary>
    /// <remarks>
    /// <code>
    /// 功能特性
    /// - 提供 UICanvas 和 GComponent 的快速索引功能
    /// - 提供设置 UI 组件显示状态的便捷方法
    /// - 简化 FairyGUI 组件的常用操作
    /// 
    /// 使用手册
    /// 1. 索引操作
    /// 
    /// 1.1 UICanvas 索引
    /// 
    ///     var button = canvas.Index<GButton>("panel.button");
    ///     
    /// 1.2 GComponent 索引
    /// 
    ///     var text = component.Index<GTextField>("text");
    /// 
    /// 2. 显示状态设置
    /// 
    /// 2.1 设置组件显示状态
    /// 
    ///     obj.SetActiveState(false); // 隐藏组件
    ///     
    /// 2.2 设置子组件显示状态
    /// 
    ///     component.SetActiveState("child.button", true); // 显示指定路径的子组件
    /// 
    /// </code>
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
