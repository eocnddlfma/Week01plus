using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Orbiter : MonoBehaviour
{
    public Transform center;
    public float orbitRadius = 4f;               // 공전 반지름
    public float orbitSpeed = 180f;              // 초당 회전 각도 (도)
    public float boomerangMultiplier = 2f;       // 부메랑 최대 반지름 = orbitRadius × multiplier
    public float boomerangDurationFactor = 90f;  // duration = factor / orbitSpeed

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
            float duration = boomerangDurationFactor / orbitSpeed;
            StartCoroutine(BoomerangCoroutine(duration));
        }
    }

    private IEnumerator BoomerangCoroutine(float duration)
    {
        isBoomeranging = true;

        // 나가는 구간: EaseInQuad
        float elapsed = 0f;
        float from = orbitRadius;
        float to = BoomerangMaxRadius;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            currentRadius = Mathf.Lerp(from, to, t * t);
            yield return null;
        }
        currentRadius = to;

        // 돌아오는 구간: EaseOutQuad
        elapsed = 0f;
        from = BoomerangMaxRadius;
        to = orbitRadius;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            currentRadius = Mathf.Lerp(from, to, t * (2f - t));
            yield return null;
        }
        currentRadius = to;

        isBoomeranging = false;
    }
}
