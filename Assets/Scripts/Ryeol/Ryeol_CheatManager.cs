using UnityEngine;

public class Ryeol_CheatManager : MonoBehaviour
{
    [SerializeField] private int _cheatScore = 500;

    [SerializeField] private Jaein_PlayerBase _player;
    [SerializeField] private Ryeol_UI_Lives _ui_lives;

    void Update()
    {
        // 1: 점수 추가
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Ryeol_GameManager.Instance.AddScore(_cheatScore);
            Debug.Log($"[Cheat] 점수 +{_cheatScore} / 현재 점수: {Ryeol_GameManager.Instance.GetScore()}");
        }

        // 2: 점수 초기화
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            // TODO
            Debug.Log("[Cheat] 점수 초기화");
        }

        // 3: 적 수 1 추가
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Ryeol_GameManager.Instance.RegisterEnemy();
            Debug.Log($"[Cheat] 적 수 +1 / 현재 적 수: {Ryeol_GameManager.Instance.EnemyCount}");
        }

        // 4: 적 수 1 내림
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            //int count = Ryeol_GameManager.Instance.EnemyCount;
            //for (int i = 0; i < count; i++)
            Ryeol_GameManager.Instance.UnregisterEnemy();
            Debug.Log($"[Cheat] 적 수 -1 / 현재 적 수: {Ryeol_GameManager.Instance.EnemyCount}");
        }

        // 플레이어 1 체력 깎기
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            _player.TakeDamage(1);

            int currentHp = _player.Hp;
            _ui_lives.TakeDamage(currentHp);

            Debug.Log($"[Cheat] HP -1 / 현재 HP: {_player.Hp}");
        }
    }
}
