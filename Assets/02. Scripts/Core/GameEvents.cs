using System;

/// <summary>
/// 글로벌 이벤트 버스 (Phase 6)
/// 게임 전역에서 사용하는 모든 이벤트를 정적 클래스로 관리하여 모듈 간 결합도 감소
/// </summary>
public static class GameEvents
{
    // ============= 게임 상태 =============
    /// <summary>
    /// 게임 상태가 변경될 때 발화 (Ready → Playing → GameOver/GameClear)
    /// </summary>
    public static event Action<GameManager.GameState> OnGameStateChanged;

    /// <summary>
    /// 점수가 획득될 때 발화
    /// </summary>
    public static event Action<int> OnScoreChanged;

    // ============= 전투 =============
    /// <summary>
    /// 적이 처치될 때 발화 (EnemyBase.OnDeath → GameManager.UnregisterEnemy)
    /// </summary>
    public static event Action OnEnemyKilled;

    /// <summary>
    /// 플레이어가 피격될 때 발화 (현재 남은 체력)
    /// </summary>
    public static event Action<int> OnPlayerDamaged;

    // ============= 웨이브 관리 =============
    /// <summary>
    /// 웨이브가 시작될 때 발화
    /// </summary>
    public static event Action<int> OnWaveStarted;

    /// <summary>
    /// 웨이브가 클리어될 때 발화 (bool: 다음 웨이브가 보스 웨이브인지 여부)
    /// </summary>
    public static event Action<bool> OnWaveCleared;

    // ============= 증강 시스템 =============
    /// <summary>
    /// 증강이 선택/적용될 때 발화
    /// </summary>
    public static event Action<AugmentData> OnAugmentApplied;

    // ============= Raise 메서드 =============
    public static void RaiseGameStateChanged(GameManager.GameState state) => OnGameStateChanged?.Invoke(state);
    public static void RaiseScoreChanged(int score) => OnScoreChanged?.Invoke(score);
    public static void RaiseEnemyKilled() => OnEnemyKilled?.Invoke();
    public static void RaisePlayerDamaged(int remainingHp) => OnPlayerDamaged?.Invoke(remainingHp);
    public static void RaiseWaveStarted(int waveIndex) => OnWaveStarted?.Invoke(waveIndex);
    public static void RaiseWaveCleared(bool isBoss) => OnWaveCleared?.Invoke(isBoss);
    public static void RaiseAugmentApplied(AugmentData data) => OnAugmentApplied?.Invoke(data);
}
