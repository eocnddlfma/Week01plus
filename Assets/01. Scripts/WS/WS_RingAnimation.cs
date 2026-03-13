using UnityEngine;
using DG.Tweening;

public class RingAnimation : MonoBehaviour
{
    [System.Serializable]
    private struct RingStep
    {
        public float radius;
        public int segments;
    }

    [Header("Reference")]
    [SerializeField] private WS_CircleLineRenderer _mainRing;
    [SerializeField] private WS_CircleLineRenderer _innterRing;
    [SerializeField] private WS_CircleLineRenderer _outerRing;
    [SerializeField] private WS_CircleLineRenderer _outerRing2;
    [SerializeField] private WS_ChargeCameraEffect _cameraEffect;

    [Header("Curve")]
    [SerializeField] private AnimationCurve _curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    [Header("Step")]
    [SerializeField] private RingStep _startStep = new RingStep { radius = 3f, segments = 3 };
    [SerializeField] private RingStep _step1 = new RingStep { radius = 7f, segments = 4 };
    [SerializeField] private RingStep _step2 = new RingStep { radius = 14f, segments = 12 };
    [SerializeField] private RingStep _endStep = new RingStep { radius = 24f, segments = 32 };

    [Header("Duration")]
    [SerializeField] private float _firstDuration = 1f;
    [SerializeField] private float _secondDuration = 1f;
    [SerializeField] private float _thirdDuration = 1f;

    //TODO 최종 스텝에 도달하면 아래 필드를 SetActive(true)해줘.

    private Sequence _sequence;
    private float _cameraStartSize;

    public void Play()
    {
        _sequence?.Kill();

        Apply(_startStep.radius, _startStep.segments, _cameraStartSize);

        _sequence = DOTween.Sequence();

        _sequence.Append(
            DOVirtual.Float(0f, 1f, _firstDuration, t =>
            {
                float eased = _curve.Evaluate(t);

                float radius = Mathf.Lerp(_startStep.radius, _step1.radius, eased);
                int segments = Mathf.RoundToInt(Mathf.Lerp(_startStep.segments, _step1.segments, eased));
                float camSize = Mathf.Lerp(_cameraStartSize, _step1.radius, eased);

                Apply(radius, segments, camSize);
            })
        );

        _sequence.Append(
            DOVirtual.Float(0f, 1f, _secondDuration, t =>
            {
                float eased = _curve.Evaluate(t);

                float radius = Mathf.Lerp(_step1.radius, _step2.radius, eased);
                int segments = Mathf.RoundToInt(Mathf.Lerp(_step1.segments, _step2.segments, eased));
                float camSize = Mathf.Lerp(_step1.radius, _step2.radius, eased);

                Apply(radius, segments, camSize);
            })
        );

        _sequence.Append(
            DOVirtual.Float(0f, 1f, _thirdDuration, t =>
            {
                float eased = _curve.Evaluate(t);

                float radius = Mathf.Lerp(_step2.radius, _endStep.radius, eased);
                int segments = Mathf.RoundToInt(Mathf.Lerp(_step2.segments, _endStep.segments, eased));
                float camSize = Mathf.Lerp(_step2.radius, _endStep.radius, eased);

                Apply(radius, segments, camSize);
            }).OnComplete(() =>
            {
                _innterRing.gameObject.SetActive(true);
                _outerRing.gameObject.SetActive(true);
                _outerRing2.gameObject.SetActive(true);
            })
        );
    }

    private void Apply(float radius, int segments, float cameraSize)
    {
        _mainRing.SetCircle(radius, segments);

        if (_cameraEffect != null)
            _cameraEffect.SetBaseSize(cameraSize);
    }

    private void OnDestroy()
    {
        _sequence?.Kill();
    }
}