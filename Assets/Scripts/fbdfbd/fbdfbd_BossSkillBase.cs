using UnityEngine;

public abstract class fbdfbd_BossSkillBase : MonoBehaviour
{
    /// <summary>
    /// 스킬 시전 전 동작
    /// 없다면 생략 가능
    /// </summary>
    public abstract void Enter();

    /// <summary>
    /// 스킬 동작
    /// </summary>
    public abstract void Execute();

    /// <summary>
    /// 스킬 종료 후 동작
    /// </summary>
    public abstract void Exit();
}
