using UnityEngine;

/// <summary>
/// 튜토리얼 전용 궤도 공.
/// PlayerController 없이 TutorialPlayerController를 중심으로 궤도를 돕니다.
/// TutorialBat이 TriggerLaunchFromWeapon을 호출하면 발사됩니다.
/// </summary>
public class TutorialOrbitalWeapon : MonoBehaviour
{
    public enum BallState { Orbit, Launched, Returning }

    [Header("Orbit")]
    [SerializeField] private float _orbitRadius      = 2f;
    [SerializeField] private float _orbitSpeed       = 180f;  // 도/초
    [SerializeField] private float _startAngle       = 0f;

    [Header("Launch")]
    [SerializeField] private float _launchSpeed      = 12f;
    [SerializeField] private float _launchDuration   = 0.4f;

    [Header("Return")]
    [SerializeField] private float _returnStrength   = 16f;
    [SerializeField] private float _returnDamping    = 1f;
    [SerializeField] private float _maxReturnSpeed   = 18f;

    [Header("Damage")]
    [SerializeField] private int   _damage           = 5;

    private TutorialPlayerController _player;
    private Transform                _center;

    private BallState _state = BallState.Orbit;
    private float     _angleDeg;
    private Vector2   _velocity;
    private float     _stateTimer;
    private float     _returnTimeElapsed;

    private void Start()
    {
        _player = FindAnyObjectByType<TutorialPlayerController>();
        if (_player == null)
        {
            Debug.LogError("[TutorialOrbitalWeapon] TutorialPlayerController를 찾지 못했습니다.");
            return;
        }
        _center   = _player.transform;
        _angleDeg = _startAngle;
        transform.position = OrbitalPos(_angleDeg);
    }

    private void Update()
    {
        if (_center == null) return;

        switch (_state)
        {
            case BallState.Orbit:     UpdateOrbit();     break;
            case BallState.Launched:  UpdateLaunched();  break;
            case BallState.Returning: UpdateReturning(); break;
        }
    }

    // ── 상태별 업데이트 ────────────────────────────────────

    private void UpdateOrbit()
    {
        _angleDeg = (_angleDeg + _orbitSpeed * Time.deltaTime) % 360f;
        transform.position = OrbitalPos(_angleDeg);
    }

    private void UpdateLaunched()
    {
        _stateTimer -= Time.deltaTime;
        transform.position += (Vector3)(_velocity * Time.deltaTime);
        CheckOverlapHits();

        if (_stateTimer <= 0f)
        {
            _returnTimeElapsed = 0f;
            _state = BallState.Returning;
        }
    }

    private void UpdateReturning()
    {
        Vector2 pos = transform.position;

        // 중심 방향으로 당기기
        Vector2 toCenter   = (Vector2)_center.position - pos;
        float   dist       = toCenter.magnitude;
        Vector2 pullDir    = dist > 0.001f ? toCenter / dist : Vector2.up;

        _returnTimeElapsed += Time.deltaTime;
        _velocity += pullDir * (_returnStrength + _returnTimeElapsed * 8f) * Time.deltaTime;
        _velocity -= _velocity * (_returnDamping * Time.deltaTime);

        if (_velocity.magnitude > _maxReturnSpeed)
            _velocity = _velocity.normalized * _maxReturnSpeed;

        pos += _velocity * Time.deltaTime;
        transform.position = pos;

        // 궤도 반경 내 복귀 판정
        float distToOrbit = Mathf.Abs(dist - _orbitRadius);
        if (distToOrbit <= 0.3f && _velocity.magnitude <= 8f)
            RejoinOrbit(pos);
    }

    private void RejoinOrbit(Vector2 pos)
    {
        Vector2 fromCenter = pos - (Vector2)_center.position;
        _angleDeg = Mathf.Atan2(fromCenter.y, fromCenter.x) * Mathf.Rad2Deg;
        _velocity = Vector2.zero;
        _state    = BallState.Orbit;
    }

    // ── 외부 호출 (TutorialBat에서 호출) ──────────────────

    public void TriggerLaunchFromWeapon(float chargePercent, float attackPower = 1f)
    {
        Launch();
    }

    public void PlayWeaponEffect(Transform weaponTransform, float chargePercent) { }

    private void Launch()
    {
        // 플레이어 body 방향으로 발사
        Vector2 dir = Vector2.right;
        if (_player != null)
        {
            // Body는 TutorialPlayerController의 첫 번째 자식
            Transform body = _player.transform.childCount > 0
                ? _player.transform.GetChild(0)
                : _player.transform;
            dir = body.right;
        }

        _velocity   = dir * _launchSpeed;
        _stateTimer = _launchDuration;
        _returnTimeElapsed = 0f;
        _state      = BallState.Launched;
    }

    // ── 충돌 ──────────────────────────────────────────────

    [Header("Hit Detection")]
    [SerializeField] private float _hitRadius = 0.3f;

    private void CheckOverlapHits()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _hitRadius);
        foreach (var hit in hits)
        {
            var sw = hit.GetComponentInParent<TutorialSwitch>();
            if (sw != null) { sw.OnBallHit(); continue; }

            var tutEnemy = hit.GetComponentInParent<TutorialEnemy>();
            if (tutEnemy != null) { tutEnemy.Hit(); continue; }

            var enemy = hit.GetComponentInParent<EnemyBase>();
            if (enemy != null) enemy.TakeDamage(_damage);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_state == BallState.Orbit) return;

        var sw = other.GetComponentInParent<TutorialSwitch>();
        if (sw != null) { sw.OnBallHit(); return; }

        var tutEnemy = other.GetComponentInParent<TutorialEnemy>();
        if (tutEnemy != null) { tutEnemy.Hit(); return; }

        var enemy = other.GetComponentInParent<EnemyBase>();
        if (enemy != null) enemy.TakeDamage(_damage);
    }

    // ── 헬퍼 ──────────────────────────────────────────────

    private Vector2 OrbitalPos(float angleDeg)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        return (Vector2)_center.position + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * _orbitRadius;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (_center == null) return;
        Gizmos.color = Color.cyan;
        const int seg = 64;
        for (int i = 0; i < seg; i++)
        {
            float a0 = i       / (float)seg * 360f * Mathf.Deg2Rad;
            float a1 = (i + 1) / (float)seg * 360f * Mathf.Deg2Rad;
            Gizmos.DrawLine(
                (Vector2)_center.position + new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * _orbitRadius,
                (Vector2)_center.position + new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * _orbitRadius);
        }
    }
#endif
}
