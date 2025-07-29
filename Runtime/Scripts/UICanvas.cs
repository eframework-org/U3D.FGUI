// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;
using UnityEngine;
using FairyGUI;
using EFramework.Utility;

namespace EFramework.FairyGUI
{
    /// <summary>
    /// UICanvas 拓展了 UIPanel 组件的功能，提供包资源的自动加载和依赖管理功能。
    /// </summary>
    /// <remarks>
    /// <code>
    /// 功能特性
    /// - 依赖关系管理：自动处理包之间的依赖关系，确保资源按正确的顺序加载
    /// - 组件快速索引：可通过名称或路径快速获取子对象，简化对象查找过程
    /// 
    /// 使用手册
    /// 1. 挂载组件
    /// 
    /// 通过编辑器挂载组件：
    /// 
    ///     点击 AddComponentMenu 按钮
    ///     选择 "FairyGUI → UI Canvas"
    ///     在 Inspector 中设置相关属性
    /// 
    /// 2. 依赖加载
    /// 
    /// 允许自定义包资源依赖的加载：
    /// 
    ///     // 设置全局的自定义加载器
    ///     UICanvas.Loader = (canvas) => {
    ///         // 自定义加载逻辑
    ///         Debug.Log($"正在加载包：{canvas.packageName}");
    ///     };
    /// 
    /// 3. 快速索引
    /// 
    /// 通过名称或路径获取 UI 组件：
    /// 
    ///     // 获取按钮组件
    ///     var button = canvas.Index&lt;GButton&gt;("panel.loginButton");
    ///     
    ///     // 获取文本组件
    ///     var text = canvas.Index&lt;GTextField&gt;("panel.welcomeText");
    ///     
    ///     // 获取列表组件
    ///     var list = canvas.Index&lt;GList&gt;("panel.itemList");
    /// </code>
    /// 更多信息请参考模块文档。
    /// </remarks>
    [AddComponentMenu("FairyGUI/UI Canvas")]
    public class UICanvas : UIPanel, XComp.IIndexable
    {
        /// <summary>
        /// Loader 是自定义的包资源加载器，可用于实现自定义的包资源加载逻辑。
        /// </summary>
        public static Action<UICanvas> Loader;

        /// <summary>
        /// packageMani 是包资源的清单，用于保持对原始资源包的引用。
        /// </summary>
        public UIManifest packageMani; // keep reference of raw assetbundle

        /// <summary>
        /// Awake 在组件唤醒时执行初始化，自动加载包资源。
        /// </summary>
        protected virtual void Awake()
        {
            if (Application.isPlaying)
            {
                if (!string.IsNullOrEmpty(packagePath) && UIPackage.GetByName(packageName) == null)
                {
                    if (Loader != null) Loader(this);
                    else if (packageMani == null) UIPackage.AddPackage(packagePath);
                    else LoadPackage(packageMani);
                }
            }
        }

        /// <summary>
        /// LoadPackage 加载包资源及其依赖项。
        /// </summary>
        /// <param name="mani">要加载的 UI 资源清单</param>
        protected virtual void LoadPackage(UIManifest mani)
        {
            if (mani)
            {
                foreach (var dep in mani.Dependency)
                {
                    if (dep is GameObject)
                    {
                        var dmani = (dep as GameObject).GetComponent<UIManifest>();
                        if (dmani) LoadPackage(dmani);
                    }
                }
                UIPackage.AddPackage(mani.PackagePath);
            }
        }

        /// <summary>
        /// OnUpdateSource 更新源数据时的处理，在编辑器模式下自动更新 packageMani 引用。
        /// </summary>
        /// <param name="data">更新的数据数组</param>
        protected override void OnUpdateSource(object[] data)
        {
            if (Application.isPlaying) return;
            base.OnUpdateSource(data);

#if UNITY_EDITOR
            if (string.IsNullOrEmpty(packagePath)) packageMani = null;
            else packageMani = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>($"{packagePath}.prefab").GetComponent<UIManifest>();
#endif
        }

        /// <summary>
        /// Index 实现了 IIndexable 接口，通过名称获取子对象。
        /// </summary>
        /// <param name="name">要查找的对象名称或路径</param>
        /// <param name="type">要返回的对象类型</param>
        /// <returns>找到的对象，如果未找到则返回 null</returns>
        public object Index(string name, Type type)
        {
            if (string.IsNullOrEmpty(name)) return null;
            var wantsGObject = type == null || typeof(GObject).IsAssignableFrom(type);
            if (wantsGObject && ui != null)
            {
                var ret = ui.GetChild(name) ?? ui.GetChildByPath(name);
                if (ret == null) return null;
                return type == null || type.IsAssignableFrom(ret.GetType()) ? ret : null;
            }
            return type == null ? null : XComp.Index((object)gameObject, name, type);
        }
    }
}
