using DG.Tweening;
using UnityEngine;

/// <summary>
/// 튜토리얼 전용 배트.
/// 이 오브젝트를 플레이어 body의 자식으로 두면 자동으로 바라보는 방향을 따라갑니다.
/// 로컬 position을 (거리, 0, 0)으로 설정하면 항상 바라보는 방향 앞쪽에 위치합니다.
/// 클릭 시 로컬 Z축으로 스윙합니다.
/// </summary>
public class TutorialBat : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _pivot;   // Player → Body → Pivot  ← 이걸 회전시킴

    [Header("Swing")]
    [SerializeField] private float _swingRadius   = 1.5f;
    [SerializeField] private float _swingAngle    = 160f;
    [SerializeField] private float _swingDuration = 0.25f;
    [SerializeField] private float _cooldown      = 0.4f;
    [SerializeField] private AnimationCurve _swingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Ball Launch")]
    [SerializeField] private float _launchChargePercent = 1f;   // 공에 전달할 차지량
    [SerializeField] private float _launchAttackPower   = 1f;   // 공에 전달할 어택파워

    private float     _nextSwingTime;
    private bool      _isSwinging;

    private void Awake()
    {
        if (_pivot == null)
            _pivot = transform.parent;   // 직접 지정 안 했으면 부모(Pivot)를 기본값으로
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !_isSwinging && Time.time >= _nextSwingTime)
        {
            Swing();
            _nextSwingTime = Time.time + _cooldown;
        }
    }

    private System.Collections.IEnumerator SwingRoutine()
    {
        _isSwinging = true;

        // BatOrbitalWeapon과 동일하게 월드 Z 기준으로 스윙
        float facingZ = _pivot.eulerAngles.z;
        float startZ  = facingZ ;
        float endZ    = facingZ + _swingAngle * 1f;

        float elapsed = 0f;
        while (elapsed < _swingDuration)
        {
            float t = _swingCurve.Evaluate(Mathf.Clamp01(elapsed / _swingDuration));
            _pivot.eulerAngles = new Vector3(0f, 0f, Mathf.Lerp(startZ, endZ, t));
            elapsed += Time.deltaTime;
            yield return null;
        }

        _pivot.eulerAngles = new Vector3(0f, 0f, facingZ);
        _isSwinging = false;
    }

    private void Swing()
    {
        StartCoroutine(SwingRoutine());

        // 히트 판정
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _swingRadius);
        foreach (var hit in hits)
        {
            // 튜토리얼 적
            var enemy = hit.GetComponentInParent<TutorialEnemy>();
            if (enemy != null)
            {
                enemy.Hit();
                continue;
            }

            // 튜토리얼 궤도 공 → 날리기
            if (hit.TryGetComponent<TutorialOrbitalWeapon>(out var tutBall))
            {
                tutBall.TriggerLaunchFromWeapon(_launchChargePercent, _launchAttackPower);
                continue;
            }

            // 일반 궤도 공 → 날리기 (혹시 씬에 있을 경우)
            if (hit.TryGetComponent<OrbitalWeapon>(out var ball))
            {
                ball.TriggerLaunchFromWeapon(_launchChargePercent, _launchAttackPower);
                ball.PlayWeaponEffect(transform, _launchChargePercent);
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _swingRadius);
    }
#endif
}
