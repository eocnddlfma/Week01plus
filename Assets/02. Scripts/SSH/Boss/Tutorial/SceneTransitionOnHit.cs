using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 충돌(Trigger 또는 TutorialSwitch.OnBallHit)하면 지정된 씬으로 이동합니다.
/// </summary>
public class SceneTransitionOnHit : MonoBehaviour
{
    [SerializeField] private string _sceneName;

    private bool _triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        TriggerTransition();
    }

    public void TriggerTransition()
    {
        if (_triggered) return;
        _triggered = true;
        SceneManager.LoadScene(_sceneName);
    }
}
