using System.Collections;
using UnityEngine;

public class fbdfbd_BossSkillDash : fbdfbd_BossSkillBase
{
    [Header("Skill Data")]
    [SerializeField] private fbdfbd_SOBossSkillDash _data;

    private fbdfbd_EnemyBase _owner;
    private bool _isDashing;
    private static readonly WaitForFixedUpdate _waitFixed = new WaitForFixedUpdate();

    private void Awake()
    {
        _owner = GetComponent<fbdfbd_EnemyBase>();
    }

    public override void Enter()
    {
    }

    public override void Execute()
    {
        if (_isDashing) return;
        if (_owner == null || _data == null) return;

        StartCoroutine(DashRoutine());
    }

    public override void Exit()
    {
    }

    private IEnumerator DashRoutine()
    {
        _isDashing = true;
        SpawnDashStartFx();

        Vector2 dir = GetDashDirection();
        float elapsed = 0f;

        while (elapsed < _data.DashDuration)
        {
            _owner.AddExternalVelocity(dir * _data.DashSpeed);

            elapsed += Time.fixedDeltaTime;
            yield return _waitFixed;
        }

        _isDashing = false;
    }

    private Vector2 GetDashDirection()
    {
        if (_owner.Target != null)
        {
            Vector2 toTarget = (Vector2)_owner.Target.position - (Vector2)transform.position;
            if (toTarget.sqrMagnitude > 0.0001f)
                return toTarget.normalized;
        }

        return Vector2.down;
    }

    private void SpawnDashStartFx()
    {
        if (_data.DashStartFxPrefab == null)
            return;

        Instantiate(_data.DashStartFxPrefab, transform.position, Quaternion.identity);
    }
}
