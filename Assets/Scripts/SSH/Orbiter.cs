using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class Orbiter : MonoBehaviour
{
    /// <summary>
    /// 공 회전하는 스크립트임
    /// 일단 테스트를 위해 뉴인풋의 스페이스로 발사하게 만들어놨는데,
    /// 나중에 이거 게임에 쓰실거면 
    /// 함수로 바꾼다음에 
    /// 공 여러개 순서대로 할거면 스택같은걸로 쓰거나
    /// 충돌시 발사하는 식으로 바꾸는걸 추천함
    /// 260310 신상현 작성 
    /// </summary>

 
    [Header("Orbit")]
    [Tooltip("공전의 중심 Transform")]
    public Transform center;
    [Tooltip("기본 공전 반지름")]
    public float orbitRadius = 4f;
    [Tooltip("초당 회전 각도 (도/초)")]
    public float orbitSpeed = 180f;

    [Header("Boomerang")]
    [Tooltip("부메랑 거리 배율(높을수록 멀리 나가요!)")]
    public float boomerangMultiplier = 5f;
    [Tooltip("부메랑 중 회전할 목표 각도 (도) — 예: 180이면 반바퀴 회전")]
    public float boomerangDurationFactor = 90f;
    [Tooltip("부메랑이 나갈 때 적용되는 이징 커브(수정ㄴ)")]
    public AnimationCurve easeOut = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [Tooltip("부메랑이 돌아올 때 적용되는 이징 커브(수정ㄴ)")]
    public AnimationCurve easeIn  = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Speed")]
    [Tooltip("기본 공전시 현재 반지름과 목표 반지름이 다르면 이동하는 속도(수정할일 없음) currentRadius를 orbitRadius로 복구하는 속도 (단위/초)")]
    public float radiusRecoverySpeed = 30f;
    [Tooltip("부메랑때 최대 높이 도달 시 감속할 속도 (0~1, 1이면 감속 없음)")]
    public float boomerangMinSpeedMultiplier = 0.5f;
    [Tooltip("부메랑 중 증가할 속도 배율 (1이면 orbitSpeed 그대로)")]
    public float boomerangSpeedMultiplier = 3f;

    [Header("Trajectory")]
    [Tooltip("궤적을 표시할 LineRenderer (없으면 자동 생성)")]
    public LineRenderer trajectoryRenderer;
    [Tooltip("궤적 점 개수 (높을수록 부드럽지만 비용 증가)")]
    public int trajectoryResolution = 60;

    private float BoomerangMaxRadius => orbitSpeed/150 * boomerangMultiplier;//회전 속도에 따라 최대 반지름이 달라지도록 (속도가 빠르면 멀리 나감)
    private float EffectiveBoomerangSpeedMultiplier => Mathf.Max(0.2f, boomerangSpeedMultiplier - BoomerangMaxRadius/7);

    private float angle = 0f;
    private float currentRadius;
    private bool isBoomeranging = false;

    void Start()
    {
        currentRadius = orbitRadius;

        if (trajectoryRenderer == null)
        {
            trajectoryRenderer = gameObject.AddComponent<LineRenderer>();
            trajectoryRenderer.startWidth = 0.05f;
            trajectoryRenderer.endWidth = 0.05f;
            trajectoryRenderer.useWorldSpace = true;
            trajectoryRenderer.material = new Material(Shader.Find("Sprites/Default"));
            trajectoryRenderer.startColor = new Color(1f, 1f, 0f, 0.8f);
            trajectoryRenderer.endColor   = new Color(1f, 0.5f, 0f, 0.1f);
        }
    }

    void Update()
    {
        print(" ");
        print(boomerangSpeedMultiplier);
        print(BoomerangMaxRadius);
        print(EffectiveBoomerangSpeedMultiplier);
        float t = Mathf.Clamp01(Mathf.InverseLerp(orbitRadius, BoomerangMaxRadius, currentRadius));
        float baseSpeed = orbitSpeed * (isBoomeranging ? EffectiveBoomerangSpeedMultiplier : 1f);
        float effectiveAngularSpeed = baseSpeed * Mathf.Lerp(1f, boomerangMinSpeedMultiplier, t * t);
        angle += effectiveAngularSpeed * Time.deltaTime;

        float rad = angle * Mathf.Deg2Rad;
        transform.position = center.position + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * currentRadius;

        if (!isBoomeranging)
            currentRadius = Mathf.MoveTowards(currentRadius, orbitRadius, radiusRecoverySpeed * Time.deltaTime);

        if (Keyboard.current.spaceKey.wasPressedThisFrame && !isBoomeranging)
        {
            isBoomeranging = true;
            float avgFactor = (2f + boomerangMinSpeedMultiplier) / 3f;
            float duration = boomerangDurationFactor / (orbitSpeed * EffectiveBoomerangSpeedMultiplier * avgFactor * 1.6f);
            DOTween.Sequence()
                .Append(DOTween.To(() => currentRadius, v => currentRadius = v, BoomerangMaxRadius, duration * 0.6f).SetEase(easeOut))
                .Append(DOTween.To(() => currentRadius, v => currentRadius = v, orbitRadius, duration).SetEase(easeIn))
                .OnComplete(() => isBoomeranging = false);
        }

        UpdateTrajectory();
    }

    void UpdateTrajectory()
    {
        if (trajectoryRenderer == null) return;

        if (isBoomeranging)
        {
            trajectoryRenderer.positionCount = 0;
            return;
        }

        float avgFactor = (2f + boomerangMinSpeedMultiplier) / 3f;
        float duration    = boomerangDurationFactor / (orbitSpeed * EffectiveBoomerangSpeedMultiplier * avgFactor * 1.6f);
        float durationOut = duration * 0.6f;
        float totalDuration = durationOut + duration;

        trajectoryRenderer.positionCount = trajectoryResolution;

        float simAngle = angle;
        float stepDt   = totalDuration / (trajectoryResolution - 1);

        for (int i = 0; i < trajectoryResolution; i++)
        {
            float elapsed = i * stepDt;

            float simRadius = elapsed <= durationOut
                ? Mathf.Lerp(orbitRadius, BoomerangMaxRadius, easeOut.Evaluate(elapsed / durationOut))
                : Mathf.Lerp(BoomerangMaxRadius, orbitRadius, easeIn.Evaluate((elapsed - durationOut) / duration));

            float rad = simAngle * Mathf.Deg2Rad;
            trajectoryRenderer.SetPosition(i, center.position + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * simRadius);

            float tSpeed     = Mathf.Clamp01(Mathf.InverseLerp(orbitRadius, BoomerangMaxRadius, simRadius));
            float baseSpeed  = orbitSpeed * EffectiveBoomerangSpeedMultiplier;
            simAngle += baseSpeed * Mathf.Lerp(1f, boomerangMinSpeedMultiplier, tSpeed * tSpeed) * stepDt;
        }
    }
}
