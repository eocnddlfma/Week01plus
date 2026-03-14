using TMPro;
using UnityEngine;

public class WaveComingText : MonoBehaviour
{
    private Animation _animation;
    private TMP_Text _text;

    public void OnEnable()
    {
        _animation = GetComponent<Animation>();
        _text = GetComponent<TMP_Text>();
        // Phase 6: WaveManager 참조 제거 및 GameEvents로 변경
        GameEvents.OnWaveCleared += Play;
    }
    private void OnDisable()
    {
        // Phase 6: GameEvents로 변경
        GameEvents.OnWaveCleared -= Play;
    }

    private void Play(bool isBoss)
    {
        if(_animation != null)
            _animation.Play();

        _text.text = isBoss == true ? "B O S S C O M I N G" : "Next Wave Coming!";
    }
}