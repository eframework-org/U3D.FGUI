// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using EFramework.FairyGUI.Editor;
using UnityEditor;
using UnityEngine;
using EFramework.FairyGUI;

public class TestUICanvasEditor
{
    const string TEST_PREFAB_PATH = "Assets/Temp/TestCanvas.prefab";

    [Test]
    public void OnInit()
    {
        // Arrange
        UICanvasEditor.icon = null;
        var originCount = EditorApplication.projectWindowItemOnGUI.GetInvocationList()?.Length ?? 0;

        // Act
        UICanvasEditor.OnInit();

        // Assert
        Assert.IsNotNull(UICanvasEditor.icon, "icon应当被加载到");
        var addedCount = EditorApplication.projectWindowItemOnGUI.GetInvocationList().Length;
        Assert.AreEqual(originCount + 1, addedCount, "回调函数应当被注册");
    }

    [Test]
    public void PostProcessor()
    {
        // Arrange
        UICanvasEditor.OnInit();
        var canvasObj = new GameObject("TestCanvas");
        canvasObj.AddComponent(typeof(UICanvas));

        try
        {
            // Assert
            PrefabUtility.SaveAsPrefabAsset(canvasObj, TEST_PREFAB_PATH);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(TEST_PREFAB_PATH);
            var actualIcon = EditorGUIUtility.GetIconForObject(prefab);
            Assert.AreEqual(UICanvasEditor.icon, actualIcon, "图标设置应当正确");
        }
        finally
        {
            // Cleanup
            AssetDatabase.DeleteAsset(TEST_PREFAB_PATH);
            Object.DestroyImmediate(canvasObj);
        }
    }
}
#endif