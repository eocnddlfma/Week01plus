using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class WS_CircleLineRenderer : MonoBehaviour
{
    [SerializeField] private float _radius = 3f;
    [SerializeField] private int _segments = 96;

    private LineRenderer _lineRenderer;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        DrawCircle();
    }

    private void OnValidate()
    {
        if (_segments < 3)
            _segments = 3;

        if (_lineRenderer == null)
            _lineRenderer = GetComponent<LineRenderer>();

        DrawCircle();
    }

    private void DrawCircle()
    {
        if (_lineRenderer == null)
            return;

        _lineRenderer.loop = true;
        _lineRenderer.positionCount = _segments;

        float angleStep = 360f / _segments;

        for (int i = 0; i < _segments; i++)
        {
            float angle = Mathf.Deg2Rad * angleStep * i;
            float x = Mathf.Cos(angle) * _radius;
            float y = Mathf.Sin(angle) * _radius;

            _lineRenderer.SetPosition(i, new Vector3(x, y, 0f));
        }
    }
}