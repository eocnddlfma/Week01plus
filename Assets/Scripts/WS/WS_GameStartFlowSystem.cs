using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class WS_MainGameFlowController : MonoBehaviour
{

    [SerializeField] private CanvasGroup _titleCg;
    [SerializeField] private RingAnimation _ringAnimation;
    private bool _isClick = false;
    

    // 타이틀 화면 없앰 이 다음 3-2-1 연출 뒤에 게임 시작
    public void TitleClick()
    {
        //중복 방지
        if (_isClick)
            return;
        _isClick = true;

        _titleCg.GetComponent<Animation>().Play();

        DOVirtual.DelayedCall(1.8f, () =>
        {
            _ringAnimation.Play();
        });
    }
}