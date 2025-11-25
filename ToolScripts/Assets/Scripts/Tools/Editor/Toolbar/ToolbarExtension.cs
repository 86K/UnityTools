#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Unity.CodeEditor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tools.Editor
{
    /// <summary>
    /// 工具栏扩展。
    /// </summary>
    public static class ToolbarExtension
    {
        private static GUIContent _switchSceneContent;
        private static GUIContent _openIDEContent;

        private static List<Type> _toolTypes;
        private static List<string> _scenePaths;

        [InitializeOnLoadMethod]
        static void Init()
        {
            var currentOpenSceneName = SceneManager.GetActiveScene().name;

            _switchSceneContent =
                EditorGUIUtility.TrTextContentWithIcon(string.IsNullOrEmpty(currentOpenSceneName) ? "Switch Scene" : currentOpenSceneName, "切换场景",
                    "UnityLogo");
            _openIDEContent = EditorGUIUtility.TrTextContentWithIcon("Open C# Project", "打开C#工程", "dll Script Icon");

            Toolbar.LeftToolbarGUI.Add(DrawLeftToolbars);
            Toolbar.RightToolbarGUI.Add(DrawRightToolbars);

            EditorSceneManager.sceneOpened += OnSceneOpened;
        }

        static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            _switchSceneContent.text = scene.name;
        }

        static void DrawLeftToolbars()
        {
            // 绘制场景切换dropdown
            GUILayout.Space(5);
            float switchSceneContentWidth = EditorStyles.toolbarPopup.CalcSize(_switchSceneContent).x;
            // NOTE：计算出来的width - 50 是刚刚好覆盖住，给2像素的余量显得不是非常逼仄
            if (EditorGUILayout.DropdownButton(_switchSceneContent, FocusType.Passive, EditorStyles.toolbarPopup,
                    GUILayout.Width(switchSceneContentWidth - 48)))
            {
                DrawSwitchSceneContent();
            }

            // 绘制打开项目工程
            GUILayout.Space(5);
            if (GUILayout.Button(_openIDEContent, EditorStyles.toolbarButton, GUILayout.MaxWidth(120)))
            {
                DrawOpenIDEContent();
            }
        }

        static void DrawRightToolbars()
        {
            
        }

        static void DrawSwitchSceneContent()
        {
            GenericMenu menu = new GenericMenu
            {
                allowDuplicateNames = true
            };
            var sceneGuids = AssetDatabase.FindAssets("t:scene", new string[] { "Assets" });
            _scenePaths = new List<string>();

            for (int i = 0; i < sceneGuids.Length; i++)
            {
                var scenePath = AssetDatabase.GUIDToAssetPath(sceneGuids[i]);
                _scenePaths.Add(scenePath);

                string fileDirectory = System.IO.Path.GetDirectoryName(scenePath);
                var sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                string displayName = $"{System.IO.Path.GetRelativePath("Assets", fileDirectory)}/{sceneName}";

                menu.AddItem(new GUIContent(displayName), false, x => { SwitchScene((int)x); }, i);
            }

            menu.ShowAsContext();
        }

        static void SwitchScene(int idx)
        {
            if (idx >= 0 && idx < _scenePaths.Count)
            {
                var scenePath = _scenePaths[idx];
                var currentScene = SceneManager.GetActiveScene();
                if (currentScene.isDirty)
                {
                    int optionIdx = EditorUtility.DisplayDialogComplex("警告", $"当前场景{currentScene.name}未保存, 是否保存?", "保存", "取消", "不保存");
                    switch (optionIdx)
                    {
                        case 0:
                            if (!EditorSceneManager.SaveOpenScenes())
                            {
                                return;
                            }

                            break;
                        case 1:
                            return;
                    }
                }

                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            }
        }

        static void DrawOpenIDEContent()
        {
            AssetDatabase.Refresh();

            CodeEditor.Editor.CurrentCodeEditor.SyncAll();
            CodeEditor.Editor.CurrentCodeEditor.OpenProject();
        }
    }
}
#endif