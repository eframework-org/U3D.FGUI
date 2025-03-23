// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using EFramework.FairyGUI;
using UnityEngine;
using FairyGUI;
using EFramework.Utility;

public class TestUIUtility
{
    [Test]
    public void Index()
    {
        var canvasObj = new GameObject("TestCanvas");
        var canvas = canvasObj.AddComponent<UICanvas>();
        var rootComp = new GComponent();

        // 测试UICanvas.Index
        var testCanvas1 = new GameObject("testChild1");
        testCanvas1.SetParent(canvasObj);
        var testCanvas2 = new GameObject("testChild2");
        testCanvas2.SetParent(testCanvas1);
        var boxCollider = testCanvas2.AddComponent<BoxCollider>();

        // 测试正常情况
        var canvasResult = canvas.Index<BoxCollider>("testChild2");
        Assert.AreSame(boxCollider, canvasResult, "应该返回正确的子对象");
        // 测试组件不存在
        canvasResult = canvas.Index<BoxCollider>("testChild1");
        Assert.IsNull(canvasResult, "当组件不存在时应该返回null");
        // 测试路径不存在
        canvasResult = canvas.Index<BoxCollider>("nonExistentPath");
        Assert.IsNull(canvasResult, "当路径不存在时应该返回null");
        // 测试UICanvas为null
        canvas = null;
        canvasResult = canvas.Index<BoxCollider>("testChild2");
        Assert.IsNull(canvasResult, "当UICanvas为null时应该返回null");

        // 测试GComponent.Index
        var child1 = new GComponent();
        var child2 = new GGraph();
        child1.name = "testChild1";
        child2.name = "testChild2";
        rootComp.AddChild(child1);
        child1.AddChild(child2);

        // 测试正常情况
        var gCompResult = rootComp.Index<GGraph>("testChild1.testChild2");
        Assert.IsNotNull(gCompResult, "应该返回正确的子对象");
        // 测试组件不存在
        gCompResult = rootComp.Index<GGraph>("testChild1");
        Assert.IsNull(gCompResult, "当组件不存在时应该返回null");
        // 测试路径不存在
        gCompResult = rootComp.Index<GGraph>("nonExistentPath");
        Assert.IsNull(gCompResult, "当路径不存在时应该返回null");
        // 测试GComponent为null
        rootComp = null;
        gCompResult = rootComp.Index<GGraph>("testChild1.testChild2");
        Assert.IsNull(gCompResult, "当GComponent为null时应该返回null");

        // 清理测试对象
        GameObject.DestroyImmediate(canvasObj);
    }

    [Test]
    public void SetActiveState()
    {
        // 准备测试对象
        var rootComp = new GComponent();
        var objChild = new GObject();
        var compChild1 = new GComponent();
        var compChild2 = new GComponent();
        objChild.name = "objChild";
        compChild1.name = "compChild1";
        compChild2.name = "compChild2";
        rootComp.AddChild(objChild);
        rootComp.AddChild(compChild1);
        compChild1.AddChild(compChild2);

        // 测试GObject
        objChild.SetActiveState(true);
        Assert.IsTrue(objChild.visible, "应该设置GObject为可见");
        objChild.SetActiveState(false);
        Assert.IsFalse(objChild.visible, "应该设置GObject为不可见");

        // 测试GComponent
        rootComp.SetActiveState(true);
        Assert.IsTrue(rootComp.visible, "应该设置GComponent为可见");
        rootComp.SetActiveState(false);
        Assert.IsFalse(rootComp.visible, "应该设置GComponent为不可见");

        rootComp.SetActiveState("compChild1.compChild2", true);
        Assert.IsTrue(compChild2.visible, "应该设置GComponent为可见");
        rootComp.SetActiveState("compChild1.compChild2", false);
        Assert.IsFalse(compChild2.visible, "应该设置GComponent为不可见");
    }
}
#endif
