using UnityEngine;
using UnityEngine.UI;

namespace Tools
{
    [AddComponentMenu("itsxwz/Component/Fps Counter")]
    public class FpsCounter : MonoBehaviour
    {
        [SerializeField] private Text _fpsText;
        
        float _updateInterval;
        int _currentFps;
        int _frames;
        float _accumulator;
        float _timeLeft;

        void Awake()
        {
            _updateInterval = 0.5f;
            _currentFps = 0;
            _frames = 0;
            _accumulator = 0f;
            _timeLeft = 0f;

            DontDestroyOnLoad(this);
        }

        void Update()
        {
            _frames++;
            _accumulator += Time.deltaTime;
            _timeLeft -= Time.unscaledDeltaTime;

            if (_timeLeft <= 0f)
            {
                _currentFps = _accumulator > 0f ? (int) (_frames / _accumulator) : 0;
                _frames = 0;
                _accumulator = 0f;
                _timeLeft += _updateInterval;

                if (_fpsText)
                    _fpsText.text = "Fps: " + _currentFps;
            }
        }

        /// <summary>
        /// [NOTE]
        /// XR equipments cannot use this function, you need to show fps data by a text.
        /// </summary>
        void OnGUI()
        {
            if (_fpsText)
                return;
            
            GUIStyle style = new GUIStyle("Box")
            {
                fontSize = 40,
                richText = true,
                alignment = TextAnchor.MiddleCenter
            };

            GUI.color = Color.red;
            GUILayout.Box("Fps: " + _currentFps, style);
        }
    }
}