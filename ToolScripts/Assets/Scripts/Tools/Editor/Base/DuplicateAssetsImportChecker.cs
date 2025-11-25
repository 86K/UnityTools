#if UNITY_EDITOR
namespace Tools.Editor
{
    using System.IO;
    using System.Text.RegularExpressions;
    using UnityEditor;
    using UnityEngine;
    
    /// <summary>
    /// 重复资产导入检查器。
    /// </summary>
    internal class DuplicateAssetsImportChecker
    {
        internal string OriginalAssetPath => Path.Combine(_directoryName, _fileName + "." + _extension);

        internal readonly bool isExist;
        internal readonly string _fileName;
        internal readonly string _extension;

        readonly string _directoryName;
        readonly string _assetPath;
        const string Pattern = "^(?<name>.*)\\s\\d+\\.(?<m_Extension>.*)$";

        internal DuplicateAssetsImportChecker(string assetPath)
        {
            _assetPath = assetPath;
            _directoryName = Path.GetDirectoryName(assetPath);
            var match = Regex.Match(Path.GetFileName(assetPath), Pattern);

            isExist = match.Success;

            if (isExist)
            {
                _fileName = match.Groups["name"].Value;
                _extension = match.Groups["m_Extension"].Value;
            }
        }

        internal void Overwrite()
        {
            FileUtil.ReplaceFile(_assetPath, OriginalAssetPath);
            Delete();
            AssetDatabase.ImportAsset(OriginalAssetPath);
        }

        internal void Delete()
        {
            AssetDatabase.DeleteAsset(_assetPath);
        }
    }

    internal class DuplicateImport : AssetPostprocessor
    {
        const string Message = "\"{0}.{1}\"is already exist, would you want to update?";

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromPath)
        {
            if (Event.current == null || Event.current.type != EventType.DragPerform)
                return;

            foreach (var assetPath in importedAssets)
            {
                var checker = new DuplicateAssetsImportChecker(assetPath);
                if (checker.isExist)
                {
                    var overwriteMessage =
                        string.Format(Message, checker._fileName, checker._extension);
                    
                    var result = EditorUtility.DisplayDialogComplex(checker.OriginalAssetPath, overwriteMessage,
                        "Replace",
                        "Keep",
                        "Cancel");

                    if (result == 0)
                    {
                        checker.Overwrite();
                    }
                    else if (result == 2)
                    {
                        checker.Delete();
                    }
                }
            }
        }
    }
}
#endif