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
    const string TEST_MANIFEST_PATH = "Assets/Temp/TestManifest/TestManifest.prefab";
    const string TEST_RAW_PATH = "Assets/Temp/RawPath";
    const string PACKAGE1_PATH = "Assets/Temp/Package1/Package1.prefab";
    const string PACKAGE2_PATH = "Assets/Temp/Package2/Package2.prefab";
    const string PACKAGE1_RAW_PATH = "Assets/Temp/Package1Raw";
    const string PACKAGE2_RAW_PATH = "Assets/Temp/Package2Raw";

    [SetUp]
    public void Setup()
    {
        if (!XFile.HasDirectory(TEST_RAW_PATH)) XFile.CreateDirectory(TEST_RAW_PATH);
        var prefabDir = Path.GetDirectoryName(TEST_MANIFEST_PATH);
        if (!XFile.HasDirectory(prefabDir)) XFile.CreateDirectory(prefabDir);

    }

    [TearDown]
    public void Reset()
    {
        if (XFile.HasDirectory(TEST_RAW_PATH)) XFile.DeleteDirectory(TEST_RAW_PATH);
        var prefabDir = Path.GetDirectoryName(TEST_MANIFEST_PATH);
        if (XFile.HasDirectory(prefabDir)) XFile.DeleteDirectory(prefabDir);
    }

    [Test]
    public void OnInit()
    {
        // Arrange
        UIManifestEditor.icon = null;
        var originCount = EditorApplication.projectWindowItemOnGUI.GetInvocationList().Length;

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

        PrefabUtility.SaveAsPrefabAsset(go, TEST_MANIFEST_PATH);
        GameObject.DestroyImmediate(go);
        LogAssert.Expect(LogType.Error, new Regex(@"UIManifestEditor\.Import: raw path doesn't exist: .*"));
        // Act
        UIManifestEditor.Collect(TEST_MANIFEST_PATH);

        // Assert
        Assert.IsNotNull(UIManifestEditor.manifests, "manifests列表未初始化");
        Assert.Contains(TEST_MANIFEST_PATH, UIManifestEditor.manifests, "应当收集到manifest");

        // 测试重复路径添加
        var originCount = UIManifestEditor.manifests.Count;
        UIManifestEditor.Collect(TEST_MANIFEST_PATH);
        Assert.AreEqual(originCount, UIManifestEditor.manifests.Count, "重复路径应当被忽略");

        // Cleanup
        UIManifestEditor.manifests = null;
    }

    [Test]
    public void Create()
    {
        // 创建测试用的fui.bytes文件
        var fuiBytesPath = XFile.PathJoin(TEST_RAW_PATH, "TestManifest_fui.bytes");
        XFile.SaveText(fuiBytesPath, "test content");

        // Act
        var asset = UIManifestEditor.Create(TEST_MANIFEST_PATH, TEST_RAW_PATH);

        // Assert
        Assert.IsNotNull(asset, "应该成功创建资产");
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(TEST_MANIFEST_PATH);
        Assert.IsNotNull(prefab, "预制体应该存在");

        var manifest = prefab.GetComponent<UIManifest>();
        Assert.IsNotNull(manifest, "预制体应该包含UIManifest组件");
        Assert.AreEqual(TEST_RAW_PATH, manifest.RawPath, "RawPath应该被正确设置");
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
        PrefabUtility.SaveAsPrefabAsset(go, TEST_MANIFEST_PATH);    // 保存预制体触发Import
        Object.DestroyImmediate(go);
        Assert.IsFalse(result, "当RawPath不存在时应当返回False");

        // 创建一个manifest，并设置一个正确的RawPath
        // 创建测试用的fui.bytes文件
        var fuiBytesPath = XFile.PathJoin(TEST_RAW_PATH, "TestManifest_fui.bytes");
        XFile.SaveText(fuiBytesPath, "test content");
        go = new GameObject();
        go.AddComponent<UIManifest>().RawPath = TEST_RAW_PATH;
        PrefabUtility.SaveAsPrefabAsset(go, TEST_MANIFEST_PATH);
        Object.DestroyImmediate(go);

        result = UIManifestEditor.Import(TEST_MANIFEST_PATH);
        Assert.IsTrue(result, "Import应当成功");

        // 验证文件是否被复制
        var prefabDir = Path.GetDirectoryName(TEST_MANIFEST_PATH);
        var copiedFuiPath = XFile.PathJoin(prefabDir, "TestManifest_fui.bytes");
        Assert.IsTrue(XFile.HasFile(copiedFuiPath), "文件应该被复制到预制体目录");

        // 验证Manifest属性是否被更新
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(TEST_MANIFEST_PATH);
        var correctManifest = prefab.GetComponent<UIManifest>();
        Assert.AreEqual("TestManifest", correctManifest.PackageName, "PackageName应该被正确设置");
        Assert.AreEqual(TEST_MANIFEST_PATH.Replace(".prefab", ""), correctManifest.PackagePath, "PackagePath应该被正确设置");
    }

    [Test]
    public void ImportWithDependency()
    {
        if (!XFile.HasDirectory(PACKAGE1_RAW_PATH)) XFile.CreateDirectory(PACKAGE1_RAW_PATH);
        if (!XFile.HasDirectory(PACKAGE2_RAW_PATH)) XFile.CreateDirectory(PACKAGE2_RAW_PATH);
        var packagePath = EFramework.Editor.XEditor.Utility.FindPackage().assetPath;
        var package1Path = XFile.PathJoin(packagePath, "Tests/Runtime/Resources/Package1_fui.bytes");
        var package2Path = XFile.PathJoin(packagePath, "Tests/Runtime/Resources/Package2_fui.bytes");
        if (XFile.HasFile(package1Path)) XFile.CopyFile(package1Path, XFile.PathJoin(PACKAGE1_RAW_PATH, "Package1_fui.bytes"));
        if (XFile.HasFile(package2Path)) XFile.CopyFile(package2Path, XFile.PathJoin(PACKAGE2_RAW_PATH, "Package2_fui.bytes"));
        var prefab1Dir = Path.GetDirectoryName(PACKAGE1_PATH);
        var prefab2Dir = Path.GetDirectoryName(PACKAGE2_PATH);
        if (!XFile.HasDirectory(prefab1Dir)) XFile.CreateDirectory(prefab1Dir);
        if (!XFile.HasDirectory(prefab2Dir)) XFile.CreateDirectory(prefab2Dir);

        try
        {
            LogAssert.Expect(LogType.Error, new Regex(@".*UIManifestEditor.Import: manifest: .* dependency:.* was not found, please create it and import again.*"));
            LogAssert.Expect(LogType.Log, new Regex(@".*UIManifestEditor.Import: manifest: .* dependency: .* has cycle reference, please check it.*"));
            // 创建Package1
            var go1 = new GameObject("Package1");
            var manifest1 = go1.AddComponent<UIManifest>();
            manifest1.RawPath = PACKAGE1_RAW_PATH;
            manifest1.PackageName = "Package1";
            manifest1.PackagePath = PACKAGE1_PATH.Replace(".prefab", "");
            PrefabUtility.SaveAsPrefabAsset(go1, PACKAGE1_PATH);
            Object.DestroyImmediate(go1);

            // 创建Package2
            var go2 = new GameObject("Package2");
            var manifest2 = go2.AddComponent<UIManifest>();
            manifest2.RawPath = PACKAGE2_RAW_PATH;
            manifest2.PackageName = "Package2";
            manifest2.PackagePath = PACKAGE2_PATH.Replace(".prefab", "");
            PrefabUtility.SaveAsPrefabAsset(go2, PACKAGE2_PATH);
            Object.DestroyImmediate(go2);

            // 设置依赖关系，使Package1依赖Package2
            go1 = AssetDatabase.LoadAssetAtPath<GameObject>(PACKAGE1_PATH);
            manifest1 = go1.GetComponent<UIManifest>();
            manifest1.Dependency = new List<Object> { AssetDatabase.LoadAssetAtPath<GameObject>(PACKAGE2_PATH) };
            PrefabUtility.SavePrefabAsset(go1);

            // 设置依赖关系，使Package2依赖Package1
            go2 = AssetDatabase.LoadAssetAtPath<GameObject>(PACKAGE2_PATH);
            manifest2 = go2.GetComponent<UIManifest>();
            manifest2.Dependency = new List<Object> { AssetDatabase.LoadAssetAtPath<GameObject>(PACKAGE1_PATH) };
            PrefabUtility.SavePrefabAsset(go2);

            // 收集路径
            UIManifestEditor.Collect(PACKAGE1_PATH);
            UIManifestEditor.Collect(PACKAGE2_PATH);

            var result = UIManifestEditor.Import(PACKAGE1_PATH);
            Assert.IsTrue(result, "导入应该成功，尽管有循环依赖");
        }
        finally
        {
            if (XFile.HasFile(PACKAGE1_PATH)) XFile.DeleteFile(PACKAGE1_PATH);
            if (XFile.HasFile(PACKAGE2_PATH)) XFile.DeleteFile(PACKAGE2_PATH);
            if (XFile.HasDirectory(PACKAGE1_RAW_PATH)) XFile.DeleteDirectory(PACKAGE1_RAW_PATH);
            if (XFile.HasDirectory(PACKAGE2_RAW_PATH)) XFile.DeleteDirectory(PACKAGE2_RAW_PATH);
            if (XFile.HasDirectory(prefab1Dir)) XFile.DeleteDirectory(prefab1Dir);
            if (XFile.HasDirectory(prefab2Dir)) XFile.DeleteDirectory(prefab2Dir);
        }
    }
}
#endif
