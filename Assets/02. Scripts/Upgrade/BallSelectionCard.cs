using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BallSelectionCard : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private Image _iconImage;
    [SerializeField] private Outline _outline;

    private BallData _data;
    private Action<BallData> _onClicked;

    private void Awake()
    {
        if (_button == null)
            _button = GetComponent<Button>();
    }

    private static readonly Color[] RarityColors = {
        new Color(0.8f, 0.8f, 0.8f, 1f),  // 일반
        new Color(0.2f, 0.8f, 0.2f, 1f),  // 희귀
        new Color(0.2f, 0.5f, 1f, 1f),    // 영웅
        new Color(1f, 0.8f, 0.2f, 1f)     // 전설
    };

    private void OnEnable()
    {
        if (_button != null)
            _button.onClick.AddListener(OnClick);
    }

    private void OnDisable()
    {
        if (_button != null)
            _button.onClick.RemoveListener(OnClick);
    }

    public void Setup(BallData data, Action<BallData> onClicked)
    {
        if (_button == null)
            _button = GetComponent<Button>();

        _data = data;
        _onClicked = onClicked;
        SetInteractable(true);

        if (_nameText != null)
            _nameText.text = data.BallName;
        if (_descriptionText != null)
            _descriptionText.text = data.Description;
        if (_iconImage != null && data.Icon != null)
            _iconImage.sprite = data.Icon;
        if (_outline != null && (int)data.Rarity < RarityColors.Length)
            _outline.effectColor = RarityColors[(int)data.Rarity];
    }

    public void SetInteractable(bool interactable)
    {
        if (_button != null)
            _button.interactable = interactable;
    }

    private void OnClick()
    {
        _onClicked?.Invoke(_data);
    }
}
