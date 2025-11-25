#if UNITY_EDITOR
namespace Tools.Editor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// 项目文件夹生成器。
    /// </summary>
    internal sealed class ProjectFoldersGenerator : EditorWindow
    {
        [Flags]
        enum Options
        {
            //Unity special folders
            Editor = 1,
            Plugins,
            Resources,
            StreamingAssets,

            //self defined folders
            Art = 101,
            Program,
        }

        static readonly Dictionary<int, List<string>> DefinedPaths = new Dictionary<int, List<string>>
        {
            { (int)Options.Editor, new List<string>() },
            { (int)Options.Plugins, new List<string>() },
            { (int)Options.Resources, new List<string>() },
            { (int)Options.StreamingAssets, new List<string>() },

            {
                (int)Options.Art, new List<string>
                {
                    "Shaders",
                    "Fonts",
                    "Models",
                    "Textures",
                    "Materials",
                    "Animations",
                    "Sounds",
                }
            },
            {
                (int)Options.Program, new List<string>
                {
                    "Prefabs",
                    "Scenes",
                    "Scripts",
                }
            }
        };

        const Options DefaultOptions = (Options)(-1);
        static Options EType_Options;

        [MenuItem("Tools/itsxwz/Gen/Project folders", false, 100)]
        static void Gen()
        {
            EType_Options = DefaultOptions;
            Generate(EType_Options);
        }

        static void GenerateItem(int item)
        {
            var fullPath = Path.Combine(Application.dataPath, ((Options)item).ToString());
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }

            if (DefinedPaths.TryGetValue(item, out var definedPath))
            {
                foreach (var subFolder in definedPath)
                {
                    var path = Path.Combine(fullPath, subFolder);
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                }
            }
        }

        static void Generate(Options options)
        {
            if (options == 0)
            {
                return;
            }

            try
            {
                foreach (Options item in Enum.GetValues(typeof(Options)))
                {
                    if ((options & item) != 0)
                    {
                        GenerateItem((int)item);
                    }
                }

                AssetDatabase.Refresh();
            }
            catch (Exception ex)
            {
                AssetDatabase.Refresh();
                Debug.LogError($"Generate project folder failed, cause :{ex}");
            }
        }
    }
}
#endif