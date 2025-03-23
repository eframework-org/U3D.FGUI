// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using EFramework.FairyGUI;
using UnityEngine;
using FairyGUI;
using System.Collections.Generic;

public class TestUICanvas
{
    [TestCase(true, "通过UIPanel")]
    [TestCase(false, "不通过UIPanel")]
    public void Index(bool isUIPanel, string _)
    {
        object result = null;
        UICanvas canvas = null;
        var rootObj = new GameObject("TestCanvas");
        if (isUIPanel)
        {
            canvas = rootObj.AddComponent<UICanvas>();

            UIPackage.AddPackage("Package1");
            canvas.packageName = "Package1";
            canvas.componentName = "Component1_2";
            canvas.CreateUI();

            // 测试正常情况
            result = canvas.Index("Child1.Child2", typeof(GComponent));
            Assert.IsNotNull(result, "应该返回正确的子对象");
        }
        else
        {

            canvas = rootObj.AddComponent<UICanvas>();
            // 创建测试子对象
            var testChild1 = new GameObject("testChild1");
            testChild1.transform.SetParent(rootObj.transform);
            var testChild2 = new GameObject("testChild2");
            testChild2.transform.SetParent(testChild1.transform);
            var boxCollider = testChild2.AddComponent<BoxCollider>();

            // 测试正常情况
            result = canvas.Index("testChild2", typeof(BoxCollider));
            Assert.AreEqual(boxCollider, result, "应该返回正确的子对象");
        }

        // 测试路径不存在
        result = canvas.Index("nonExistentPath", typeof(BoxCollider));
        Assert.IsNull(result, "当路径不存在时应该返回null");

        // 测试类型不存在
        result = canvas.Index("testChild2", typeof(MeshRenderer));
        Assert.IsNull(result, "当类型不存在时应该返回null");

        // 清理测试对象
        GameObject.DestroyImmediate(canvas.gameObject);
    }

    [TestCase(true, "有依赖关系")]
    [TestCase(false, "没有依赖关系")]
    public void Awake(bool hasDependency, string _)
    {
        // 创建测试对象
        var canvasObj = new GameObject("TestCanvas");
        canvasObj.SetActive(false);
        var canvas = canvasObj.AddComponent<UICanvas>();
        canvas.packagePath = "Package1";
        canvas.packageName = "Component1_2";
        if (hasDependency)
        {
            // 创建测试UIManifest
            var manifest = canvasObj.AddComponent<UIManifest>();
            manifest.PackagePath = "Package1";
            // 创建测试依赖
            var dependencyObj = new GameObject("TestDependency");
            var dependencyManifest = dependencyObj.AddComponent<UIManifest>();
            dependencyManifest.PackagePath = "Package2";
            manifest.Dependency = new List<Object> { dependencyObj };
            dependencyManifest.Dependency = new List<Object>();
            canvas.packageMani = manifest;
        }
        else
        {
            bool loaderCalled = false;
            UICanvas capturedCanvas = null;
            // 设置Loader回调
            UICanvas.Loader = (uiCanvas) =>
            {
                loaderCalled = true;
                capturedCanvas = uiCanvas;
            };

            canvasObj.SetActive(true);

            Assert.IsTrue(loaderCalled, "Loader回调应该被调用");
            Assert.AreEqual(canvas, capturedCanvas, "Loader回调应该接收正确的UICanvas实例");
        }

        if (hasDependency)
        {
            canvasObj.SetActive(true);
            Assert.IsTrue(UIPackage.GetByName("Package2") != null, "Package2应该被加载");
        }

        // 清理测试对象
        GameObject.DestroyImmediate(canvasObj);
    }
}
#endif
