using UnityEngine;

public class Ryeol_CheatManager : MonoBehaviour
{
    [SerializeField] private int _cheatScore = 500;

    void Update()
    {
        // Q: 점수 추가
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Ryeol_GameManager.Instance.AddScore(_cheatScore);
            Debug.Log($"[Cheat] 점수 +{_cheatScore} / 현재 점수: {Ryeol_GameManager.Instance.GetScore()}");
        }

        // W: 점수 초기화
        if (Input.GetKeyDown(KeyCode.W))
        {
            // TODO
            Debug.Log("[Cheat] 점수 초기화");
        }

        // E: 적 수 1 추가
        if (Input.GetKeyDown(KeyCode.E))
        {
            Ryeol_GameManager.Instance.RegisterEnemy();
            Debug.Log($"[Cheat] 적 수 +1 / 현재 적 수: {Ryeol_GameManager.Instance.EnemyCount}");
        }

        // R: 적 수 초기화
        if (Input.GetKeyDown(KeyCode.R))
        {
            int count = Ryeol_GameManager.Instance.EnemyCount;
            for (int i = 0; i < count; i++)
                Ryeol_GameManager.Instance.UnregisterEnemy();
            Debug.Log("[Cheat] 적 수 초기화");
        }
    }
}
