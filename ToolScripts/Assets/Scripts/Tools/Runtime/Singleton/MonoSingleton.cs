using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Tools
{
    /// <summary>
    /// MonoBehaviour单例，默认DontDestroyOnLoad为false，即不可跨场景。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        public bool m_DontDestroyOnLoad;
        
        static T m_Instance;

        public static T Instance
        {
            get
            {
                if (!m_Instance)
                {
                    m_Instance = FindFirstObjectByType(typeof(T)) as T;

                    if (!m_Instance)
                    {
                        m_Instance = new GameObject(typeof(T).ToString(), typeof(T)).GetComponent<T>();

                        if (!m_Instance)
                        {
                            Debug.LogError("Create mono singleton failed, type of " + typeof(T));
                        }
                    }
                }

                return m_Instance;
            }
        }
        
        protected virtual void Awake()
        {
            if (!m_Instance)
            {
                m_Instance = this as T;
                if(m_DontDestroyOnLoad)
                    DontDestroyOnLoad(this);
                
                OnAwakeExecute();
            }
            else if (m_Instance != this)
            {
                DestroyImmediate(this);
                throw new Exception("same mono singleton assertion");
            }
        }

        protected virtual void OnAwakeExecute(){}

        protected async void OnDestroy()
        {
            try
            {
                OnDestroyExecute();
                await Task.Delay(1);
                m_Instance = null;
            }
            catch (Exception e)
            {
                Debug.LogError($"{typeof(T)} failed to destroy on {e}");
            }
        }
        
        protected virtual void OnDestroyExecute(){}
    }
}
