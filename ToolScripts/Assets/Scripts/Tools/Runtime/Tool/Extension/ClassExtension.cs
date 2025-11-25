using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Tools
{
    public class ClassExtension
    {
        /// <summary>
        /// 得到类中所有的字段名称（仅 Public Instance）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>以数组方式返回</returns>
        public static string[] GetClassFieldsNameArray<T>() where T : class
        {
            FieldInfo[] fieldInfos = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance);
            string[] fieldNames = new string[fieldInfos.Length];
            for (int i = 0; i < fieldInfos.Length; i++)
            {
                fieldNames[i] = fieldInfos[i].Name;
            }

            return fieldNames;
        }

        /// <summary>
        /// 得到类中所有的字段值（仅 Public Instance）
        /// </summary>
        /// <param name="t"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns>以数组方式返回</returns>
        public static string[] GetClassFieldsValueArray<T>(T t) where T : class
        {
            if (t == null)
                return Array.Empty<string>();
            
            FieldInfo[] fieldInfos = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance);
            string[] fieldValues = new string[fieldInfos.Length];
            for (int i = 0; i < fieldInfos.Length; i++)
            {
                object obj = fieldInfos[i].GetValue(t);
                fieldValues[i] = obj?.ToString() ?? string.Empty;
            }

            return fieldValues;
        }
        
         /// <summary>
        /// 对任意对象进行深拷贝（支持引用类型、数组、List、Dictionary、类、结构体）
        /// </summary>
        public static T DeepCopy<T>(T obj)
        {
            return (T)DeepCopyInternal(obj, new Dictionary<object, object>(64));
        }

         /// <summary>
        /// 递归深拷贝（带引用循环检测）
        /// </summary>
        private static object DeepCopyInternal(object obj, Dictionary<object, object> visited)
        {
            if (obj == null)
                return null;

            Type type = obj.GetType();

            // 原始类型与字符串直接返回
            if (type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal))
                return obj;

            // 避免循环引用
            if (visited.TryGetValue(obj, out var resultObj))
                return resultObj;

            // 数组处理
            if (type.IsArray)
            {
                Type elemType = type.GetElementType();
                Array array = (Array)obj;
                Array copy = Array.CreateInstance(elemType, array.Length);
                visited[obj] = copy;

                for (int i = 0; i < array.Length; i++)
                    copy.SetValue(DeepCopyInternal(array.GetValue(i), visited), i);

                return copy;
            }

            // List<T>
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                IList list = (IList)Activator.CreateInstance(type);
                visited[obj] = list;

                IList origin = (IList)obj;
                foreach (var item in origin)
                    list.Add(DeepCopyInternal(item, visited));

                return list;
            }

            // Dictionary<K, V>
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                IDictionary dict = (IDictionary)Activator.CreateInstance(type);
                visited[obj] = dict;

                IDictionary origin = (IDictionary)obj;
                foreach (var key in origin.Keys)
                {
                    var newKey = DeepCopyInternal(key, visited);
                    var newValue = DeepCopyInternal(origin[key], visited);
                    dict[newKey] = newValue;
                }

                return dict;
            }

            // 其他 class：通过反射执行深拷贝
            object clone = Activator.CreateInstance(type);
            visited[obj] = clone;

            // 复制字段
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                object value = field.GetValue(obj);
                field.SetValue(clone, DeepCopyInternal(value, visited));
            }

            // 复制可写属性
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var property in properties)
            {
                if (!property.CanWrite) continue;
                if (property.GetIndexParameters().Length > 0) continue; // 跳过索引器

                object value = property.GetValue(obj);
                property.SetValue(clone, DeepCopyInternal(value, visited));
            }

            return clone;
        }
    }
}
