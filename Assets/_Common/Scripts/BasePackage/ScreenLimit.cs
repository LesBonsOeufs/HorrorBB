///-----------------------------------------------------------------
///   Author : Théo Sabattié                    
///   Date   : 04/02/2020 14:06
///-----------------------------------------------------------------

using UnityEngine;

namespace Com.GabrielBernabeu.Common {
    [RequireComponent(typeof(Camera))]
    public class ScreenLimit : MonoBehaviour {
        private Rect _rect;
        private Camera _camera;

        public Rect Rect => _rect;
        public Camera Camera => _camera;

        public static ScreenLimit Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            _camera = GetComponent<Camera>();
            Update();
        }

        private void Update()
        {
            Vector3 position = transform.position;
            float height = _camera.orthographicSize * 2;
            Vector2 size = new Vector2(_camera.aspect * height, height);

            _rect.xMin = position.x - size.x / 2;
            _rect.yMin = position.y - size.y / 2;
            _rect.xMax = position.x + size.x / 2;
            _rect.yMax = position.y + size.y / 2;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}