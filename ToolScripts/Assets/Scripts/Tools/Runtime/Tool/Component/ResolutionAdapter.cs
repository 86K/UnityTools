using System.IO;

namespace Tools
{
    using UnityEngine;
    using UnityEngine.UI;
    using System.Xml;

    /// <summary>
    /// TESTED
    /// editor : 1920*1080  real：redmi k30 2400*1080  GOOD FIT.
    /// 如果锚点设置妥当，应该是能够通过代码匹配不同分辨率的。
    ///
    /// 2025.11.25 好像有个插件能够根据编辑器下的分辨率直接设置锚点。
    /// </summary>
    [AddComponentMenu("itsxwz/Component/Resolution Adapter")]
    public class ResolutionAdapter : MonoBehaviour
    {
        [Tooltip("You can change this resolution as same as your project's resolution.")] [SerializeField]
        Vector2 m_PresetResolution = new Vector2(1920, 1080);

        Vector2 m_RealResolution;

        [SerializeField] private Canvas[] m_Canvas;
        string m_XmlPath;

        void Awake()
        {
            m_Canvas ??= FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            
            GetRealResolution();
            SetAdapterResolution();
        }

        void GetRealResolution()
        {
            m_XmlPath = Application.streamingAssetsPath + "/Xml/ResolutionAdapter.xml";
            if (!File.Exists(m_XmlPath))
            {
                Debug.LogError($"没有配置文件：{m_XmlPath}");
                return;
            }

            XmlDocument doc = new XmlDocument();
            doc.Load(m_XmlPath);

            var innerTextX = doc.SelectSingleNode("root/width")?.InnerText;
            if (innerTextX != null)
            {
                m_RealResolution.x = float.Parse(innerTextX);
            }
            var innerTextY = doc.SelectSingleNode("root/height")?.InnerText;
            if (innerTextY != null)
            {
                m_RealResolution.y = float.Parse(innerTextY);
            }
        }

        void SetAdapterResolution()
        {
            foreach (var canvas in m_Canvas)
            {
                CanvasScaler canvasScaler = canvas.GetComponent<CanvasScaler>();
                if (canvasScaler)
                {
                    canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    canvasScaler.referenceResolution = m_RealResolution;
                    canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
                    canvasScaler.matchWidthOrHeight = 0.5f;
                }
                else
                {
                    Debug.LogError("The Canvas don't have CanvasScaler.");
                }
                
                RectTransform[] rectTransforms = canvas.GetComponentsInChildren<RectTransform>();
                foreach (var rectTransform in rectTransforms)
                {
                    SetUITransform(rectTransform);
                }
            }
        }

        void SetUITransform(RectTransform rectTransform)
        {
            if (rectTransform != null)
            {
                Vector2 size = rectTransform.sizeDelta;
                Vector3 pos = rectTransform.localPosition;

                if (m_RealResolution != m_PresetResolution)
                {
                    //得到宽高比
                    float widthPrecent = m_RealResolution.x / m_PresetResolution.x;
                    float heightPrecent = m_RealResolution.y / m_PresetResolution.y;
                    //调整宽高
                    rectTransform.sizeDelta = new Vector2(size.x * widthPrecent, size.y * heightPrecent);
                    //调整位置
                    rectTransform.localPosition = new Vector3(pos.x * widthPrecent, pos.y * heightPrecent, pos.z);
                }
            }
        }
    }
}