using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 업그레이드 선택 카드 UI 컴포넌트.
///
/// 카드 프리팹 구조:
///   Card (Image + Button + UpgradeCard)
///   ├── RarityBorder (Image) - 등급 색상 테두리
///   ├── Icon (Image) - 업그레이드 아이콘 (선택사항)
///   ├── NameText (TextMeshProUGUI) - 업그레이드 이름
///   └── DescriptionText (TextMeshProUGUI) - 업그레이드 설명
/// </summary>
public class UpgradeCard : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image _background;
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private Button _button;
    [SerializeField] private Outline _outline;

    [Header("등급 색상")]
    [SerializeField] private Color _commonColor = new Color(0.6f, 0.6f, 0.6f);
    [SerializeField] private Color _rareColor = new Color(0.2f, 0.5f, 1f);
    [SerializeField] private Color _epicColor = new Color(0.75f, 0.2f, 1f);

    private UpgradeData _data;
    private Action<UpgradeData> _onClick;

    private void Awake()
    {
        if (_button == null)
            _button = GetComponent<Button>();
    }

    public void Setup(UpgradeData data, Action<UpgradeData> onClick)
    {
        if (_button == null)
            _button = GetComponent<Button>();

        _data = data;
        _onClick = onClick;

        if (_nameText != null)
            _nameText.text = data.upgradeName;

        if (_descriptionText != null)
            _descriptionText.text = data.description;

        // 아이콘
        if (_icon != null)
        {
            if (data.icon != null)
            {
                _icon.sprite = data.icon;
                _icon.gameObject.SetActive(true);
            }
            else
            {
                _icon.gameObject.SetActive(false);
            }
        }

        // 등급 색상
        if (_outline != null)
        {
            _outline.effectColor = data.rarity switch
            {
                UpgradeRarity.Common => _commonColor,
                UpgradeRarity.Rare => _rareColor,
                UpgradeRarity.Epic => _epicColor,
                _ => _commonColor
            };
        }

        // 버튼 이벤트
        if (_button != null)
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => _onClick?.Invoke(_data));
        }

        SetInteractable(true);
    }

    public void SetInteractable(bool interactable)
    {
        if (_button != null)
            _button.interactable = interactable;
    }
}
