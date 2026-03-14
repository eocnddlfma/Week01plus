using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.CullingGroup;

public class UI_HealthBar : MonoBehaviour
{
    [SerializeField] private GameObject _lifePrefab;
    [SerializeField] private Transform _livesContainer;

    [SerializeField] private PlayerBase _player;

    private List<Image> _lifeImages; // 연출을 위해 트래킹

    private int _currentHp = 5;

    // 연출
    private Sequence _critSeq;

    private void Start()
    {
        GameEvents.OnGameStateChanged += HandleStateChanged;  // Phase 6: GameEvents로 변경
            if (_player == null)
            {
                _player = FindAnyObjectByType<PlayerBase>();
                if (_player == null)
                    Debug.LogError("PlayerBase를 찾을 수 없습니다.");
            }
            if (_player != null)
                GameEvents.OnPlayerDamaged += TakeDamage;  // Phase 6: GameEvents로 변경
    }

    private void OnDestroy()
    {
        GameEvents.OnGameStateChanged -= HandleStateChanged;  // Phase 6: GameEvents로 변경
            if (_player != null)
                GameEvents.OnPlayerDamaged -= TakeDamage;  // Phase 6: GameEvents로 변경
    }

    void HandleStateChanged(GameManager.GameState state)
    {
        if (state == GameManager.GameState.Playing)
            SpawnLives(_player.MaxHp);
    }

    void SpawnLives(int count)
    {
        _lifeImages = new();

        for (int i = 0; i < count; i++)
        {
            var obj = Instantiate(_lifePrefab, _livesContainer);
            _lifeImages.Add(obj.GetComponent<Image>());
        }

        _currentHp = count;
    }

    // TODO:  public 빼기
    public void TakeDamage(int hp)
    {
        if (_currentHp == hp) return;

        _currentHp = hp;  

        var img = _lifeImages[_currentHp]; // 방금 깎인 슬롯  

        #region 연출

        // 쿵 튕기면서 흐려짐
        img.transform.DOPunchScale(Vector3.one * 0.5f, 0.3f, 6, 0.5f);
        img.DOFade(0.15f, 0.25f);

        if (_currentHp == 1) StartCritical();
        else StopCritical();

        void StartCritical()
        {
            StopCritical();
            var img = _lifeImages[0];

            _critSeq = DOTween.Sequence();
            _critSeq.Append(img.DOColor(new Color(1f, 0.3f, 0.3f), 0.3f).SetEase(Ease.InOutSine));
            _critSeq.Append(img.DOFade(0.2f, 0.4f).SetEase(Ease.InOutSine));
            _critSeq.Append(img.DOFade(1f, 0.4f).SetEase(Ease.InOutSine));
            _critSeq.SetLoops(-1, LoopType.Restart);
        }

        void StopCritical()
        {
            _critSeq?.Kill();
            if (_lifeImages.Count > 0)
                _lifeImages[0].DOColor(Color.white, 0.2f);
        }

        #endregion
    }
}
