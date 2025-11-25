namespace Tools
{
    using UnityEngine;
    
    [AddComponentMenu("itsxwz/Component/Billboard")]
    public class Billboard : MonoBehaviour
    {
        [SerializeField] Camera _camera;
        [SerializeField] bool _isRevert;

        void Start()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }
        }

        void Update()
        {
            if (_camera)
            {
                Vector3 headPosition = _camera.transform.position;
                Vector3 vector = headPosition - transform.position;
                vector.y = 0;
                if (vector.magnitude >= 0.5f)
                {
                    transform.rotation = Quaternion.LookRotation((_isRevert ? -1 : 1) * vector);
                }
            }
        }
    }
}