namespace Tools
{
    public abstract class Singleton<T> where T : new()
    {
        static T m_Instance;
        static readonly T Locker = new();

        public static T Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    lock (Locker)
                    {
                        m_Instance = new T();
                    }
                }

                return m_Instance;
            }
        }
    }
}
