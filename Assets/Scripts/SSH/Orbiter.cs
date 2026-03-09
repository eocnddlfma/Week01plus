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
        angle += orbitSpeed * Time.deltaTime;

        float rad = angle * Mathf.Deg2Rad;
        transform.position = center.position + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * currentRadius;

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
