#if UNITY_EDITOR
namespace Tools.Editor
{
    using UnityEngine;
    using UnityEditor;
    using System.Collections.Generic;

    internal class ScenesInBuildWindow : EditorWindow
    {
        private readonly List<string> _scenePaths = new List<string>();
        private readonly List<bool> _sceneSelections = new List<bool>();
        private float _labelWidth = 150f;
        private bool _needCalcWidth = true;

        [MenuItem("Tools/itsxwz/Window/Scenes In Build", false, 2)]
        static void Init()
        {
            ScenesInBuildWindow window = GetWindow<ScenesInBuildWindow>();
            window.titleContent = new GUIContent("Scenes In Build");
            window.Show();
        }

        private void OnEnable()
        {
            SearchForScenes();
            _needCalcWidth = true;
        }

        void OnGUI()
        {
            EditorGUILayout.HelpBox("Select toggle to add or remove scenes in build settings.", MessageType.Warning);
            GUILayout.Space(12);

            if (_scenePaths.Count == 0)
            {
                GUILayout.Label("No scenes found.");
                return;
            }

            if (_needCalcWidth)
            {
                CalcMaxLabelWidth();
                _needCalcWidth = false;
            }

            for (int i = 0; i < _scenePaths.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                GUILayout.Label(_scenePaths[i], GUILayout.Width(_labelWidth));

                bool newValue = GUILayout.Toggle(
                    _sceneSelections[i],
                    GUIContent.none,
                    GUILayout.Width(20)
                );

                if (newValue != _sceneSelections[i])
                {
                    _sceneSelections[i] = newValue;
                    UpdateBuildSettings();
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        void SearchForScenes()
        {
            _scenePaths.Clear();
            _sceneSelections.Clear();

            string[] guids = AssetDatabase.FindAssets("t:Scene");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                _scenePaths.Add(path);
                _sceneSelections.Add(IsSceneInBuildSettings(path));
            }
        }

        void CalcMaxLabelWidth()
        {
            _labelWidth = 100f;

            foreach (string path in _scenePaths)
            {
                float w = GUI.skin.label.CalcSize(new GUIContent(path)).x;
                if (w > _labelWidth)
                    _labelWidth = w + 20f;
            }
        }

        bool IsSceneInBuildSettings(string scenePath)
        {
            foreach (var s in EditorBuildSettings.scenes)
            {
                if (s.path == scenePath)
                    return true;
            }
            return false;
        }

        void UpdateBuildSettings()
        {
            List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>();

            for (int i = 0; i < _scenePaths.Count; i++)
            {
                if (_sceneSelections[i])
                {
                    buildScenes.Add(new EditorBuildSettingsScene(_scenePaths[i], true));
                }
            }

            EditorBuildSettings.scenes = buildScenes.ToArray();
        }
    }
}
#endif
