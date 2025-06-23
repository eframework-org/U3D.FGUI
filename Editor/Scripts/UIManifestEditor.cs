// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using FairyGUI;
using FairyGUIEditor;
using EFramework.Utility;
using EFramework.Editor;

namespace EFramework.FairyGUI.Editor
{
    /// <summary>
    /// UIManifest 编辑器工具，为 UIManifest 组件提供自定义图标、创建、导入和依赖管理功能。
    /// </summary>
    /// <remarks>
    /// <code>
    /// 功能特性
    /// - 为 UIManifest 预制体在项目窗口中显示自定义图标
    /// - 提供 UIManifest 资源的创建与导入功能
    /// - 自动处理 UIManifest 资源的依赖关系
    /// - 支持从 FairyGUI 导出的文档中创建 UIManifest
    /// </code>
    /// 更多信息请参考模块文档。
    /// </remarks>
    public class UIManifestEditor
    {
        /// <summary>
        /// icon 是 UIManifest 的自定义图标，用于优化资源的显示。
        /// </summary>
        internal static Texture2D icon;

        /// <summary>
        /// manifest 维护了项目中所有的 UIManifest 实例。
        /// </summary>
        internal static List<string> manifests;

        /// <summary>
        /// CachingFile 是项目中 UIManifest 资源的路径索引缓存。
        /// </summary>
        internal const string CachingFile = "Library/UIManifest.db";

        /// <summary>
        /// watchers 维护了对所有 UIManifest 目录的监听。
        /// </summary>
        internal static Dictionary<string, FileSystemWatcher> watchers = new();

        /// <summary>
        /// skips 维护了 UIManifest 资源导入的忽略列表，避免循环导入。
        /// </summary>
        internal static Dictionary<string, bool> skips = new();

        /// <summary>
        /// 项目窗口绘制回调，为 UIManifest 预制体添加自定义图标。
        /// </summary>
        /// <param name="guid">资源的 GUID</param>
        /// <param name="selectionRect">项目窗口中的绘制区域</param>
        internal static void OnProjectWindowItemOnGUI(string guid, Rect selectionRect)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.EndsWith("prefab") && AssetDatabase.LoadAssetAtPath<UIManifest>(path))
            {
                Rect iconRect = new Rect(selectionRect.x + selectionRect.width - 20, selectionRect.y, 16, 16);
                GUI.DrawTexture(iconRect, icon);
            }
        }

        /// <summary>
        /// 收集项目中所有 UIManifest 资源，用于依赖关系处理。
        /// </summary>
        /// <param name="path">要添加到收集列表中的 UIManifest 路径</param>
        /// <param name="cache">是否强制从所有资源中查找 UIManifest 实例</param>
        internal static void Collect(string path = null, bool cache = true)
        {
            manifests = new List<string>();

            var changed = false;
            var succeeded = false;
            if (cache && XFile.HasFile(CachingFile))
            {
                succeeded = true;
                try
                {
                    var lines = File.ReadAllLines(CachingFile);
                    foreach (var asset in lines)
                    {
                        if (string.IsNullOrEmpty(asset)) continue;
                        if (!XFile.HasFile(asset))
                        {
                            succeeded = false;
                            break;
                        }
                        var mani = AssetDatabase.LoadAssetAtPath<UIManifest>(asset);
                        if (mani == null)
                        {
                            succeeded = false;
                            break;
                        }
                        if (mani && !manifests.Contains(asset))
                        {
                            manifests.Add(asset);
                            Watch(mani.RawPath, asset);
                        }
                    }
                }
                catch (System.Exception e)
                {
                    XLog.Panic(e, $"Read caching file failed: {CachingFile}");
                    succeeded = false;
                }
            }

            if (!succeeded)
            {
                manifests.Clear(); // 清理并全量查找

                var assets = AssetDatabase.FindAssets("t:Prefab"); // 在OnInit中无法通过AssetDatabase.FindAssets查询
                for (var i = 0; i < assets.Length; i++)
                {
                    var asset = XFile.NormalizePath(AssetDatabase.GUIDToAssetPath(assets[i]));
                    var mani = AssetDatabase.LoadAssetAtPath<UIManifest>(asset);
                    if (mani && !manifests.Contains(asset))
                    {
                        manifests.Add(asset);
                        Watch(mani.RawPath, asset);
                    }
                }

                changed = true;
            }

            if (manifests.Count > 0) XLog.Debug("UIManifestEditor.Collect: find {0} manifest(s).", manifests.Count);

            if (!string.IsNullOrEmpty(path) && !manifests.Contains(path))
            {
                var mani = AssetDatabase.LoadAssetAtPath<UIManifest>(path);
                if (mani)
                {
                    manifests.Add(path);
                    changed = true;
                }
            }

            if (changed)
            {
                try
                {
                    File.WriteAllLines(CachingFile, manifests);
                    XLog.Debug("UIManifestEditor.Collect: write index caching into <a href=\"file:///{0}\">{1}</a>.", Path.GetFullPath(CachingFile), CachingFile);
                }
                catch (System.Exception e) { XLog.Panic(e, $"Write caching file failed: {CachingFile}"); }
            }
        }

        /// <summary>
        /// Watch 监控 UIManifest 原始路径是否变更，若发生变更，则调用 Import 重新导入之。
        /// </summary>
        /// <param name="rawPath"></param>
        /// <param name="assetPath"></param>
        internal static void Watch(string rawPath, string assetPath)
        {
            if (string.IsNullOrEmpty(rawPath) || string.IsNullOrEmpty(assetPath)) return;

            rawPath = XFile.NormalizePath(rawPath);
            if (!XFile.HasDirectory(rawPath)) return;

            if (!watchers.ContainsKey(rawPath))
            {
                var watcher = new FileSystemWatcher(rawPath)
                {
                    IncludeSubdirectories = false,
                    EnableRaisingEvents = true,
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size,
                };

                var maniName = Path.GetFileNameWithoutExtension(assetPath);
                var maniPrefix = maniName + "_";
                var maniFile = maniPrefix + "fui.bytes";
                var dirty = new List<string>();
                var expected = new List<string>();

                void proc(string path)
                {
                    if (path.Contains(maniPrefix))
                    {
                        dirty.Add(Path.GetFileName(path));
                        if (path.Contains(maniFile))
                        {
                            // 卸载原有的 Package，避免加载失败
                            if (UIPackage.GetByName(maniName) != null) UIPackage.RemovePackage(maniName);

                            var pkg = UIPackage.AddPackage(XFile.OpenFile(path), maniName, null);
                            if (pkg != null)
                            {
                                UIPackage.RemovePackage(maniName);
                                expected.Clear();
                                expected.Add(maniFile);

                                var itemsById = typeof(UIPackage).GetField("_itemsById", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(pkg) as Dictionary<string, PackageItem>;
                                foreach (var kvp in itemsById)
                                {
                                    var file = kvp.Value.file;
                                    if (!string.IsNullOrEmpty(file) && !expected.Contains(file))
                                    {
                                        expected.Add(file);
                                    }
                                }

                                // // manifest 文件的索引不为 1，则认定为非正常的导入流程，可能是外部的操作引起文件变更，如：Git 更新等
                                // var index = dirty.IndexOf(maniFile);
                                // if (index > 0 && index != 1)
                                // {
                                //     // 重置监听范围，这种方式不一定可靠，依赖 FairyGUI Editor 的生成顺序：manifest 文件的索引为 1
                                //     // 否则可能导致监听失效：外部变更，但是 dirty 未清除
                                //     dirty = dirty.GetRange(index - 1, dirty.Count - (index - 1));
                                //     XLog.Notice("invalid watch");
                                // }
                            }
                        }

                        // 等待所有依赖都被重新发布完成
                        if (expected.Count > 0 && expected.All(e => dirty.Contains(e)))
                        {
                            // 重置监控数据
                            dirty.Clear();
                            expected.Clear();

                            // 有概率出现 Unity Editor 资源导入失败：Could not create asset from Assets/xxxx: File could not be read 或出现导入两次的情况
                            // 原因1：FairyGUI Editor 未完全 Flush 文件，解决方案：延迟处理导入，XLoom.SetTimeout(() => Import(assetPath), 2000);
                            // 原因2：外部的操作（Git 更新）引起文件变更导致 dirty 监控不正确，引起提前导入，解决方案：用户重新发布/导入即可
                            Import(assetPath);
                        }
                    }
                }

                watcher.Created += (sender, args) => XLoom.RunInMain(() => proc(args.FullPath));
                watcher.Changed += (sender, args) => XLoom.RunInMain(() => proc(args.FullPath));
                watchers.Add(rawPath, watcher);
            }
        }

        /// <summary>
        /// 创建 UIManifest 的菜单项处理方法。
        /// </summary>
        [MenuItem("Assets/Create/FairyGUI/UI Manifest")]
        internal static void Create()
        {
            if (Selection.assetGUIDs.Length == 0) XLog.Error("UIManifestEditor.Create: non selection.");
            else
            {
                var path = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);
                if (string.IsNullOrEmpty(path)) XLog.Error("UIManifestEditor.Create: selection path is empty.");
                else
                {
                    var rawPath = EditorUtility.SaveFolderPanel("Select Raw Path", XEnv.ProjectPath, null);
                    if (string.IsNullOrEmpty(rawPath)) XLog.Warn("UIManifestEditor.Create: raw path is empty.");
                    else
                    {
                        rawPath = XFile.NormalizePath(Path.GetRelativePath(XEnv.ProjectPath, rawPath));
                        if (!XFile.HasDirectory(rawPath)) XLog.Error("UIManifestEditor.Create: raw path doesn't exist: {0}", rawPath);
                        else
                        {
                            var fs = Directory.GetFiles(rawPath);
                            if (!Directory.GetFiles(rawPath).Any(file => Path.GetFileName(file).EndsWith("_fui.bytes")))
                            {
                                XLog.Error("UIManifestEditor.Create: mani desc like xxx_fui.bytes was not found: {0}", rawPath);
                            }
                            else
                            {
                                var maniPath = AssetDatabase.CreateFolder(path, Path.GetFileName(rawPath));
                                if (string.IsNullOrEmpty(maniPath)) XLog.Error("UIManifestEditor.Create: create manifest folder error: {0}", maniPath);
                                else
                                {
                                    maniPath = AssetDatabase.GUIDToAssetPath(maniPath);
                                    var asset = Create(XFile.PathJoin(maniPath, Path.GetFileName(maniPath) + ".prefab"), rawPath);
                                    if (asset) Selection.activeObject = asset;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 创建 UIManifest 预制体资源。
        /// </summary>
        /// <param name="maniPath">UIManifest 预制体的保存路径</param>
        /// <param name="rawPath">FairyGUI 导出素材的路径</param>
        /// <returns>创建的 UIManifest 资源对象</returns>
        public static Object Create(string maniPath, string rawPath)
        {
            var go = new GameObject();
            go.AddComponent<UIManifest>().RawPath = rawPath;

            var asset = PrefabUtility.SaveAsPrefabAsset(go, maniPath);
            Object.DestroyImmediate(go);
            AssetDatabase.Refresh();

            Watch(rawPath, maniPath);

            return asset;
        }

        /// <summary>
        /// 导入 UIManifest 资源，处理其依赖关系。
        /// </summary>
        /// <param name="path">UIManifest 资源路径</param>
        /// <param name="visited">已访问的 UIManifest 路径列表，用于检测循环依赖</param>
        /// <returns>导入是否成功</returns>
        public static bool Import(string path, List<string> visited = null)
        {
            path = XFile.NormalizePath(path);
            var mani = AssetDatabase.LoadAssetAtPath<UIManifest>(path);
            if (mani == null)
            {
                XLog.Error("UIManifestEditor.Import: null manifest at: {0}", path);
                return false;
            }

            if (!XFile.HasDirectory(mani.RawPath))
            {
                XLog.Error("UIManifestEditor.Import: raw path doesn't exist: {0}", mani.RawPath);
                return false;
            }

            Collect(path: path);

            visited ??= new List<string>();
            if (visited.Contains(path)) return true;
            visited.Add(path);

            var maniPath = Path.GetDirectoryName(path);
            var maniName = Path.GetFileNameWithoutExtension(path);

            var ofiles = Directory.GetFiles(maniPath);
            foreach (var of in ofiles)
            {
                var nm = Path.GetFileNameWithoutExtension(of);
                var ext = Path.GetExtension(of);
                if (!nm.StartsWith(maniName + "_") || ext == ".meta") continue;
                XFile.DeleteFile(of);
            }

            XFile.CopyDirectory(mani.RawPath, maniPath, ".meta");
            AssetDatabase.Refresh();
            var nfiles = Directory.GetFiles(maniPath);

            var oldPackageName = mani.PackageName;
            var oldPackagePath = mani.PackagePath;
            var oldDependency = mani.Dependency;
            mani.PackageName = Path.GetFileNameWithoutExtension(path);
            mani.PackagePath = path.Replace(".prefab", "");
            mani.Dependency = new List<Object>();
            foreach (var nf in nfiles)
            {
                var nm = Path.GetFileNameWithoutExtension(nf);
                var ext = Path.GetExtension(nf);
                if (!nm.StartsWith(maniName + "_") || ext == ".meta") continue;
                var dep = AssetDatabase.LoadAssetAtPath<Object>(nf);
                var ip = AssetImporter.GetAtPath(nf);
                if (ip is TextureImporter)
                {
                    var tip = ip as TextureImporter;
                    if (tip.mipmapEnabled)
                    {
                        tip.mipmapEnabled = false;
                        AssetDatabase.ImportAsset(nf);
                    }
                }
                mani.Dependency.Add(dep);
            }

            // 卸载原有的 Package，避免加载失败
            if (UIPackage.GetByName(maniName) != null) UIPackage.RemovePackage(maniName);

            var pkg = UIPackage.AddPackage(XFile.PathJoin(maniPath, maniName)); // 查找依赖项
            if (pkg != null)
            {
                UIPackage.RemovePackage(maniName);
                foreach (var dep in pkg.dependencies)
                {
                    var dname = dep["name"];
                    var dpname = dname + ".prefab";
                    var sig = false;
                    for (var i = 0; i < manifests.Count; i++)
                    {
                        var temp = manifests[i];
                        if (temp.EndsWith(dpname)) // Package name 是唯一的
                        {
                            if (visited.Contains(temp))
                            {
                                XLog.Warn("UIManifestEditor.Import: manifest: {0}'s dependency: {1} has cycle reference, please check it.", maniName, dname);
                                sig = true;
                                break;
                            }
                            else if (Import(temp, visited))
                            {
                                var dobj = AssetDatabase.LoadAssetAtPath<Object>(temp);
                                mani.Dependency.Add(dobj);
                                sig = true;
                                break;
                            }
                        }
                    }
                    if (!sig) XLog.Error("UIManifestEditor.Import: manifest: {0}'s dependency: {1} was not found, please create it and import again.", maniName, dname);
                    else
                    {
                        XLog.Debug("UIManifestEditor.Import: manifest: {0}'s dependency: {1} has been imported.", maniName, dname);
                        EditorToolSet.ReloadPackages();
                    }
                }
            }

            var dirty = oldPackageName != mani.PackageName ||
            oldPackagePath != mani.PackagePath || oldDependency == null || oldDependency.Count != mani.Dependency.Count;
            if (!dirty)
            {
                // 校验文件内容是否一致
                foreach (var dep in mani.Dependency)
                {
                    if (dep is GameObject gdep && gdep && gdep.GetComponent<UIManifest>()) continue; // 忽略 UIManifest 引用
                    var dst = AssetDatabase.GetAssetPath(dep);
                    var src = XFile.PathJoin(mani.RawPath, Path.GetFileName(dst));
                    if (XFile.FileMD5(src) != XFile.FileMD5(dst)) // 对比文件的 MD5
                    {
                        dirty = true;
                        break;
                    }
                }
            }

            if (dirty)
            {
                var go = PrefabUtility.SavePrefabAsset(mani.gameObject);
                AssetDatabase.Refresh();
                if (icon) EditorGUIUtility.SetIconForObject(go, icon);
                skips[path] = true;
            }

            XLog.Debug("UIManifestEditor.Import: import <a href=\"file:///{0}\">{1}</a> from <a href=\"file:///{2}\">{3}</a> succeeded.", Path.GetFullPath(path), path, Path.GetFullPath(mani.RawPath), mani.RawPath);
            return true;
        }

        /// <summary>
        /// 资源事件监听器，处理 UIManifest 资源的导入和更新事件。
        /// </summary>
        internal class Listener : AssetPostprocessor, XEditor.Event.Internal.OnEditorInit, XEditor.Event.Internal.OnEditorLoad
        {
            /// <summary>
            /// OnPostprocessAllAssets 处理所有资源的后处理事件，导入 UIManifest 资源。
            /// </summary>
            /// <param name="importedAssets">导入的资源路径</param>
            /// <param name="deletedAssets">删除的资源路径</param>
            /// <param name="movedAssets">移动的资源路径</param>
            /// <param name="movedFromAssetPaths">资源移动前的路径</param>
            static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
            {
                foreach (var path in importedAssets)
                {
                    if (skips.ContainsKey(path)) { skips.Remove(path); continue; }
                    if (path.EndsWith(".prefab") && AssetDatabase.LoadAssetAtPath<UIManifest>(path))
                    {
                        Import(path);
                    }
                }

                foreach (var path in movedAssets)
                {
                    if (skips.ContainsKey(path)) { skips.Remove(path); continue; }
                    if (path.EndsWith(".prefab") && AssetDatabase.LoadAssetAtPath<UIManifest>(path))
                    {
                        Import(path);
                    }
                }
            }

            int XEditor.Event.Callback.Priority { get; }

            bool XEditor.Event.Callback.Singleton { get; }

            /// <summary>
            /// OnEditorInit 事件回调对项目中的 UIManifest 进行收集并进行文件校验。
            /// </summary>
            /// <param name="args"></param>
            void XEditor.Event.Internal.OnEditorInit.Process(params object[] args)
            {
                XLog.Debug("UIManifestEditor.OnInit: check and reload all manifest(s).");
                Collect(cache: false);
                foreach (var asset in manifests)
                {
                    var mani = AssetDatabase.LoadAssetAtPath<UIManifest>(asset);
                    if (mani)
                    {
                        if (XFile.HasDirectory(mani.RawPath))
                        {
                            // 校验文件内容是否一致
                            var dirty = false;
                            foreach (var dep in mani.Dependency)
                            {
                                if (dep is GameObject gdep && gdep && gdep.GetComponent<UIManifest>()) continue; // 忽略 UIManifest 引用
                                var dst = AssetDatabase.GetAssetPath(dep);
                                var src = XFile.PathJoin(mani.RawPath, Path.GetFileName(dst));
                                if (XFile.FileMD5(src) != XFile.FileMD5(dst)) // 对比文件的 MD5
                                {
                                    dirty = true;
                                    break;
                                }
                            }
                            if (dirty) Import(asset);
                        }
                        else XLog.Error("UIManifestEditor.OnInit: raw path doesn't exist, please check it: ", mani.RawPath);
                    }
                }
            }

            /// <summary>
            /// OnEditorLoad 事件回调处理了文件视图的绘制监听，并重新建立对 UIManifest 的监听。
            /// </summary>
            /// <param name="args"></param>
            void XEditor.Event.Internal.OnEditorLoad.Process(params object[] args)
            {
                // 加载图标
                var pkg = XEditor.Utility.FindPackage();
                icon = AssetDatabase.LoadAssetAtPath<Texture2D>($"Packages/{pkg.name}/Editor/Resources/Icon/Manifest.png");

                // 监听绘制
                if (icon) EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemOnGUI;

                // 监听变更
                Collect();
            }
        }
    }
}
