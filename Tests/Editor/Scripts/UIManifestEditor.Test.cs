// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using EFramework.FairyGUI.Editor;
using UnityEditor;
using UnityEngine;
using EFramework.FairyGUI;
using UnityEngine.TestTools;
using System.Text.RegularExpressions;
using EFramework.Utility;
using System.IO;
using System.Collections.Generic;

public class TestUIManifestEditor
{
    const string TestDir = "Assets/Temp/TestManifestEditor";
    readonly string TestManifest = XFile.PathJoin(TestDir, "TestManifest.prefab");
    readonly string TestRawPath = XFile.PathJoin(TestDir, "RawPath");
    readonly string TestPackage1 = XFile.PathJoin(TestDir, "Package1/Package1.prefab");
    readonly string TestPackage2 = XFile.PathJoin(TestDir, "Package2/Package2.prefab");
    readonly string TestPackageRaw1 = XFile.PathJoin(TestDir, "Package1Raw");
    readonly string TestPackageRaw2 = XFile.PathJoin(TestDir, "Package2Raw");

    [OneTimeSetUp]
    public void Init()
    {
        if (!XFile.HasDirectory(TestDir)) XFile.CreateDirectory(TestDir);
    }

    [OneTimeTearDown]
    public void Cleanup()
    {
        if (XFile.HasDirectory(TestDir)) XFile.DeleteDirectory(TestDir);
    }

    [SetUp]
    public void Setup()
    {
        if (!XFile.HasDirectory(TestRawPath)) XFile.CreateDirectory(TestRawPath);
        var prefabDir = Path.GetDirectoryName(TestManifest);
        if (!XFile.HasDirectory(prefabDir)) XFile.CreateDirectory(prefabDir);
    }

    [TearDown]
    public void Reset()
    {
        if (XFile.HasDirectory(TestRawPath)) XFile.DeleteDirectory(TestRawPath);
        var prefabDir = Path.GetDirectoryName(TestManifest);
        if (XFile.HasDirectory(prefabDir)) XFile.DeleteDirectory(prefabDir);
    }

    [Test]
    public void OnInit()
    {
        // Arrange
        UIManifestEditor.icon = null;
        var originCount = EditorApplication.projectWindowItemOnGUI == null ? 0 : EditorApplication.projectWindowItemOnGUI.GetInvocationList()?.Length ?? 0;

        // Act
        UIManifestEditor.OnInit();

        // Assert
        Assert.IsNotNull(UIManifestEditor.icon, "icon应当被加载到");
        var addedCount = EditorApplication.projectWindowItemOnGUI.GetInvocationList().Length;
        Assert.AreEqual(originCount + 1, addedCount, "回调函数应当被注册");
    }

    [Test]
    public void Collect()
    {
        // Arrange
        UIManifestEditor.manifests = null;
        var go = new GameObject();
        go.AddComponent<UIManifest>();

        PrefabUtility.SaveAsPrefabAsset(go, TestManifest);
        GameObject.DestroyImmediate(go);
        LogAssert.Expect(LogType.Error, new Regex(@"UIManifestEditor\.Import: raw path doesn't exist: .*"));
        // Act
        UIManifestEditor.Collect(TestManifest);

        // Assert
        Assert.IsNotNull(UIManifestEditor.manifests, "manifests列表未初始化");
        Assert.Contains(TestManifest, UIManifestEditor.manifests, "应当收集到manifest");

        // 测试重复路径添加
        var originCount = UIManifestEditor.manifests.Count;
        UIManifestEditor.Collect(TestManifest);
        Assert.AreEqual(originCount, UIManifestEditor.manifests.Count, "重复路径应当被忽略");

        // Cleanup
        UIManifestEditor.manifests = null;
    }

    [Test]
    public void Create()
    {
        // 创建测试用的fui.bytes文件
        var fuiBytesPath = XFile.PathJoin(TestRawPath, "TestManifest_fui.bytes");
        XFile.SaveText(fuiBytesPath, "test content");

        // Act
        var asset = UIManifestEditor.Create(TestManifest, TestRawPath);

        // Assert
        Assert.IsNotNull(asset, "应该成功创建资产");
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(TestManifest);
        Assert.IsNotNull(prefab, "预制体应该存在");

        var manifest = prefab.GetComponent<UIManifest>();
        Assert.IsNotNull(manifest, "预制体应该包含UIManifest组件");
        Assert.AreEqual(TestRawPath, manifest.RawPath, "RawPath应该被正确设置");
    }

    [Test]
    public void Import()
    {
        // 测试manifest不存在的情况
        LogAssert.Expect(LogType.Error, new Regex(@"UIManifestEditor\.Import: null manifest at: .*"));
        var go = new GameObject();
        var nonExistentPath = "Assets/Temp/NonManifest.prefab";
        PrefabUtility.SaveAsPrefabAsset(go, nonExistentPath);
        var result = UIManifestEditor.Import(nonExistentPath);
        Assert.IsFalse(result, "当manifest不存在时应当返回False");
        Object.DestroyImmediate(go);
        XFile.DeleteFile(nonExistentPath);

        // 创建一个manifest，但设置一个不存在的RawPath
        LogAssert.Expect(LogType.Error, new Regex(@"UIManifestEditor\.Import: raw path doesn't exist: .*"));
        go = new GameObject();
        var nonRawManifest = go.AddComponent<UIManifest>();
        nonRawManifest.RawPath = "Assets/Temp/NonRawPath";
        PrefabUtility.SaveAsPrefabAsset(go, TestManifest);    // 保存预制体触发Import
        Object.DestroyImmediate(go);
        Assert.IsFalse(result, "当RawPath不存在时应当返回False");

        // 创建一个manifest，并设置一个正确的RawPath
        // 创建测试用的fui.bytes文件
        var fuiBytesPath = XFile.PathJoin(TestRawPath, "TestManifest_fui.bytes");
        XFile.SaveText(fuiBytesPath, "test content");
        go = new GameObject();
        go.AddComponent<UIManifest>().RawPath = TestRawPath;
        PrefabUtility.SaveAsPrefabAsset(go, TestManifest);
        Object.DestroyImmediate(go);

        result = UIManifestEditor.Import(TestManifest);
        Assert.IsTrue(result, "Import应当成功");

        // 验证文件是否被复制
        var prefabDir = Path.GetDirectoryName(TestManifest);
        var copiedFuiPath = XFile.PathJoin(prefabDir, "TestManifest_fui.bytes");
        Assert.IsTrue(XFile.HasFile(copiedFuiPath), "文件应该被复制到预制体目录");

        // 验证Manifest属性是否被更新
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(TestManifest);
        var correctManifest = prefab.GetComponent<UIManifest>();
        Assert.AreEqual("TestManifest", correctManifest.PackageName, "PackageName应该被正确设置");
        Assert.AreEqual(TestManifest.Replace(".prefab", ""), correctManifest.PackagePath, "PackagePath应该被正确设置");
    }

    [Test]
    public void Dependency()
    {
        if (!XFile.HasDirectory(TestPackageRaw1)) XFile.CreateDirectory(TestPackageRaw1);
        if (!XFile.HasDirectory(TestPackageRaw2)) XFile.CreateDirectory(TestPackageRaw2);
        var packagePath = EFramework.Editor.XEditor.Utility.FindPackage().assetPath;
        var package1Path = XFile.PathJoin(packagePath, "Tests/Runtime/Resources/Package1_fui.bytes");
        var package2Path = XFile.PathJoin(packagePath, "Tests/Runtime/Resources/Package2_fui.bytes");
        if (XFile.HasFile(package1Path)) XFile.CopyFile(package1Path, XFile.PathJoin(TestPackageRaw1, "Package1_fui.bytes"));
        if (XFile.HasFile(package2Path)) XFile.CopyFile(package2Path, XFile.PathJoin(TestPackageRaw2, "Package2_fui.bytes"));
        var prefab1Dir = Path.GetDirectoryName(TestPackage1);
        var prefab2Dir = Path.GetDirectoryName(TestPackage2);
        if (!XFile.HasDirectory(prefab1Dir)) XFile.CreateDirectory(prefab1Dir);
        if (!XFile.HasDirectory(prefab2Dir)) XFile.CreateDirectory(prefab2Dir);

        LogAssert.Expect(LogType.Error, new Regex(@"UIManifestEditor\.Import: manifest: .* dependency:.* was not found, please create it and import again.*"));
        LogAssert.Expect(LogType.Log, new Regex(@"UIManifestEditor\.Import: manifest: .* dependency: .* has cycle reference, please check it.*"));
        // 创建Package1
        var go1 = new GameObject("Package1");
        var manifest1 = go1.AddComponent<UIManifest>();
        manifest1.RawPath = TestPackageRaw1;
        manifest1.PackageName = "Package1";
        manifest1.PackagePath = TestPackage1.Replace(".prefab", "");
        PrefabUtility.SaveAsPrefabAsset(go1, TestPackage1);
        Object.DestroyImmediate(go1);

        // 创建Package2
        var go2 = new GameObject("Package2");
        var manifest2 = go2.AddComponent<UIManifest>();
        manifest2.RawPath = TestPackageRaw2;
        manifest2.PackageName = "Package2";
        manifest2.PackagePath = TestPackage2.Replace(".prefab", "");
        PrefabUtility.SaveAsPrefabAsset(go2, TestPackage2);
        Object.DestroyImmediate(go2);

        // 设置依赖关系，使Package1依赖Package2
        go1 = AssetDatabase.LoadAssetAtPath<GameObject>(TestPackage1);
        manifest1 = go1.GetComponent<UIManifest>();
        manifest1.Dependency = new List<Object> { AssetDatabase.LoadAssetAtPath<GameObject>(TestPackage2) };
        PrefabUtility.SavePrefabAsset(go1);

        // 设置依赖关系，使Package2依赖Package1
        go2 = AssetDatabase.LoadAssetAtPath<GameObject>(TestPackage2);
        manifest2 = go2.GetComponent<UIManifest>();
        manifest2.Dependency = new List<Object> { AssetDatabase.LoadAssetAtPath<GameObject>(TestPackage1) };
        PrefabUtility.SavePrefabAsset(go2);

        // 收集路径
        UIManifestEditor.Collect(TestPackage1);
        UIManifestEditor.Collect(TestPackage2);

        var result = UIManifestEditor.Import(TestPackage1);
        Assert.IsTrue(result, "导入应该成功，尽管有循环依赖");
    }
}
#endif
