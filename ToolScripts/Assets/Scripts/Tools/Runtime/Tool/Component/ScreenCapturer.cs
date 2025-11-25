using System;
using System.IO;
using UnityEngine;

namespace Tools
{
    /// <summary>
    /// 屏幕截图器
    /// eg：提供给客户
    /// </summary>
    [AddComponentMenu("itsxwz/Component/Screen Capturer")]
    public class ScreenCapturer : MonoBehaviour
    {
        [SerializeField] KeyCode m_KeyCode = KeyCode.C;
        string m_FolderPath;
        string m_FilePath;

        void Awake()
        {
            m_FolderPath = Application.dataPath + "../ScreenCapture";
            if (!Directory.Exists(m_FolderPath))
            {
                Directory.CreateDirectory(m_FolderPath);
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(m_KeyCode))
            {
                m_FilePath = Path.Combine(m_FolderPath, DateTime.Now.ToString("HH_mm_ss") + ".jpg").Replace("\\", "/");
                if (File.Exists(m_FilePath))
                {
                    File.Delete(m_FilePath);
                }

                ScreenCapture.CaptureScreenshot(m_FilePath, ScreenCapture.StereoScreenCaptureMode.BothEyes);
            }
        }
    }
}