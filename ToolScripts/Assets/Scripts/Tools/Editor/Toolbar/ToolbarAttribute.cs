#if UNITY_EDITOR
using System;

namespace Tools.Editor
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ToolbarAttribute : Attribute
    {
        public string Path { get; private set; }
        public int Priority { get; private set; }
        public bool IsUtility { get; private set; }
        public Type Type { get; private set; }

        public ToolbarAttribute(Type type, string path, int priority, bool isUtility = false)
        {
            Path = path;
            Priority = priority;
            IsUtility = isUtility;
            Type = type;
        }
    }
}
#endif