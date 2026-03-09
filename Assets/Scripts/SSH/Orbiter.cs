using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class Orbiter : MonoBehaviour
{
    public Transform center;
    public float orbitRadius = 4f;               // 공전 반지름
    public float orbitSpeed = 180f;              // 초당 회전 각도 (도)
    public float boomerangMultiplier = 2f;       // 부메랑 최대 반지름 = orbitRadius × multiplier
    public float boomerangDurationFactor = 90f;  // duration = factor / orbitSpeed
    public AnimationCurve easeOut = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);   // 나가는 커브
    public AnimationCurve easeIn  = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);   // 돌아오는 커브
    public float radiusRecoverySpeed = 2f;                                       // 비부메랑 시 복구 속도 (단위/초)
    public float boomerangMinSpeedMultiplier = 0.5f;                             // MaxRadius에서의 최소 속도 비율
    public float boomerangSpeedMultiplier = 1.5f;                                // 부메랑 중 기본 속도 배율

    private float BoomerangMaxRadius => orbitRadius * boomerangMultiplier;

    private float angle = 0f;
    private float currentRadius;
    private bool isBoomeranging = false;

    void Start()
    {
        currentRadius = orbitRadius;
    }

    void Update()
    {
        float t = Mathf.Clamp01(Mathf.InverseLerp(orbitRadius, BoomerangMaxRadius, currentRadius));
        float baseSpeed = orbitSpeed * (isBoomeranging ? boomerangSpeedMultiplier * 2 / boomerangMultiplier : 1f);
        float effectiveAngularSpeed = baseSpeed * Mathf.Lerp(1f, boomerangMinSpeedMultiplier, t * t);
        angle += effectiveAngularSpeed * Time.deltaTime;

        float rad = angle * Mathf.Deg2Rad;
        transform.position = center.position + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * currentRadius;

        if (!isBoomeranging)
            currentRadius = Mathf.MoveTowards(currentRadius, orbitRadius, radiusRecoverySpeed * Time.deltaTime);

        if (Keyboard.current.spaceKey.wasPressedThisFrame && !isBoomeranging)
        {
            isBoomeranging = true;
            float duration = boomerangDurationFactor / orbitSpeed;
            DOTween.Sequence()
                .Append(DOTween.To(() => currentRadius, v => currentRadius = v, BoomerangMaxRadius, duration).SetEase(easeOut))
                .Append(DOTween.To(() => currentRadius, v => currentRadius = v, orbitRadius, duration).SetEase(easeIn))
                .OnComplete(() => isBoomeranging = false);
        }
    }
}
