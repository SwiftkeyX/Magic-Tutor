using UnityEngine;

namespace MagicSchool.Battle
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Vector2 _minBounds = new Vector2(-1f, 1f);
        [SerializeField] private Vector2 _maxBounds = new Vector2( 9f, 9f);

        private Camera  _cam;
        private bool    _panning;
        private Vector3 _panOriginWorld;

        private void Awake() => _cam = GetComponent<Camera>();

        private void Update()
        {
            if (Input.GetMouseButtonDown(2))
            {
                _panning        = true;
                _panOriginWorld = ScreenToWorld(Input.mousePosition);
            }

            if (_panning && Input.GetMouseButton(2))
            {
                Vector3 current = ScreenToWorld(Input.mousePosition);
                Vector3 delta   = _panOriginWorld - current;
                Vector3 newPos  = transform.position + delta;
                newPos.x = Mathf.Clamp(newPos.x, _minBounds.x, _maxBounds.x);
                newPos.y = Mathf.Clamp(newPos.y, _minBounds.y, _maxBounds.y);
                newPos.z = transform.position.z;
                transform.position = newPos;
            }

            if (Input.GetMouseButtonUp(2))
                _panning = false;
        }

        private Vector3 ScreenToWorld(Vector3 screenPos)
        {
            screenPos.z = -transform.position.z;
            return _cam.ScreenToWorldPoint(screenPos);
        }
    }
}
