using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class OrbitalTrajectory : MonoBehaviour
{
    [SerializeField] private OrbitalWeapon _weapon;

    [Header("Simulation")]
    [SerializeField] private int _simulationSteps = 500;
    [SerializeField] private float _simulationDt = 0.02f;

    [Header("Line Style")]
    [SerializeField] private Gradient _launchColor;
    [SerializeField] private Gradient _returnColor;
    [SerializeField] private float _lineWidth = 0.05f;

    private LineRenderer _lineRenderer;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.useWorldSpace = true;
        _lineRenderer.startWidth = _lineWidth;
        _lineRenderer.endWidth = _lineWidth;
        _lineRenderer.enabled = false;
    }

    private void LateUpdate()
    {
        if (_weapon == null) return;

        ShowTrajectory();
    }

    private void ShowTrajectory()
    {
        Vector2[] positions = _weapon.SimulateTrajectory(_simulationSteps, _simulationDt);

        if (positions.Length < 2)
        {
            _lineRenderer.enabled = false;
            return;
        }

        _lineRenderer.positionCount = positions.Length;
        for (int i = 0; i < positions.Length; i++)
            _lineRenderer.SetPosition(i, (Vector3)positions[i]);

        Gradient color = _weapon.IsReturning() ? _returnColor : _launchColor;
        if (color != null)
            _lineRenderer.colorGradient = color;

        _lineRenderer.enabled = true;
    }
}
