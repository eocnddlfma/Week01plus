using TMPro;
using UnityEngine;

public class WaveComingText : MonoBehaviour
{
    [SerializeField] private Ryeol_EnemySpawner _spanwer;
    private Animation _animation;
    private TMP_Text _text;

    public void OnEnable()
    {
        _animation = GetComponent<Animation>();
        _text = GetComponent<TMP_Text>();
        if (_spanwer != null)
            _spanwer.OnWaveClear += Play;
    }
    private void OnDisable()
    {
        if (_spanwer != null)
            _spanwer.OnWaveClear -= Play;
    }

    private void Play(bool isBoss)
    {
        if(_animation != null)
            _animation.Play();

        _text.text = isBoss == true ? "B O S S C O M I N G" : "Next Wave Coming!";
    }
}