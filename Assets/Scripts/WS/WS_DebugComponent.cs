using UnityEngine;
using UnityEngine.UI;

public class DebugComponent : MonoBehaviour
{
    [SerializeField] private int _textScore = 0;
    [SerializeField] private WS_ScoreTextAnimation _animationText;
    [SerializeField] private Button _testBnt;

    private void Start()
    {
        _testBnt.onClick.AddListener(() =>
        {
            _textScore += 500;
            _animationText.Play(_textScore);
        });
    }
}