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
    /// FairyGUI 的 UI 画布组件，提供包资源的自动加载和依赖管理功能。
    /// </summary>
    /// <remarks>
    /// <code>
    /// 功能特性
    /// - 自动加载指定的 FairyGUI 包资源
    /// - 支持包依赖关系的处理
    /// - 提供组件索引功能，可通过名称获取子对象
    /// - 运行时和编辑器模式下的智能处理
    /// 
    /// 使用手册
    /// 1. 基本用法
    /// 
    /// 1.1 创建 UI 画布
    /// 
    ///     在场景中创建一个 GameObject，添加 UICanvas 组件
    ///     设置对应的 packageName 和 packagePath
    ///     指定 packageMani 引用对应的 UIManifest 预制体
    /// 
    /// 1.2 自定义加载逻辑
    /// 
    ///     可通过设置静态 Loader 委托来自定义包资源的加载逻辑
    /// 
    /// </code>
    /// </remarks>
    [AddComponentMenu("FairyGUI/UI Canvas")]
    public class UICanvas : UIPanel, XComp.IIndexable
    {
        /// <summary>
        /// 自定义包资源加载器，可用于实现自定义的包资源加载逻辑。
        /// </summary>
        public static Action<UICanvas> Loader;

        /// <summary>
        /// 包资源清单，用于保持对原始资源包的引用。
        /// </summary>
        public UIManifest packageMani; // keep reference of raw assetbundle

        /// <summary>
        /// 在组件唤醒时执行初始化，自动加载包资源。
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
        /// 加载包资源及其依赖项。
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
        /// 更新源数据时的处理，在编辑器模式下自动更新 packageMani 引用。
        /// </summary>
        /// <param name="data">更新的数据数组</param>
        protected override void OnUpdateSource(object[] data)
        {
            if (Application.isPlaying)
                return;
            base.OnUpdateSource(data);
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(packagePath)) packageMani = null;
            else packageMani = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>($"{packagePath}.prefab").GetComponent<UIManifest>();
#endif
        }

        /// <summary>
        /// 实现 IIndexable 接口，通过名称获取子对象。
        /// </summary>
        /// <param name="name">要查找的对象名称或路径</param>
        /// <param name="type">要返回的对象类型</param>
        /// <returns>找到的对象，如果未找到则返回 null</returns>
        public object Index(string name, Type type)
        {
            if (ui != null)
            {
                var ret = ui.GetChild(name);
                ret ??= ui.GetChildByPath(name);
                if (ret != null) return ret;
            }
            return XComp.Index((object)gameObject, name, type);
        }
    }
}
