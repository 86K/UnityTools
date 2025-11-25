#if UNITY_EDITOR
namespace Tools.Editor
{
    using UnityEditor;

    /// <summary>
    /// 显示GUID。
    /// </summary>
    [InitializeOnLoad]
    internal static class DisplayGUID
    {
        static DisplayGUID()
        {
            Editor.finishedDefaultHeaderGUI += DisplayGUIDIfPersistent;
        }

        static void DisplayGUIDIfPersistent(Editor editor)
        {
            if (!EditorUtility.IsPersistent(editor.target))
                return;

            var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(editor.target));
            var totalRect = EditorGUILayout.GetControlRect();
            var controlRect = EditorGUI.PrefixLabel(totalRect, EditorGUIUtility.TrTempContent("GUID"));

            if (editor.targets.Length > 1)
                EditorGUI.LabelField(controlRect, EditorGUIUtility.TrTempContent("[Multiple objects selected]"));
            else
                EditorGUI.SelectableLabel(controlRect, guid);
        }
    }
}
#endif