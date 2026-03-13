using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class Ryeol_UI_Button : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private RectTransform _rect;
    [SerializeField] private TMP_Text _text;
    [SerializeField] private string _label = "";

    private void Start()
    {
        _text.text = $"[ {_label} ]";
    }

    public void OnPointerEnter(PointerEventData e)
    {
        _rect.DOScale(1.05f, 0.12f).SetEase(Ease.OutCubic);
        _text.text = $"[  {_label}  ]";  // °ýÈ£ ¹ú¾îÁü
    }

    public void OnPointerExit(PointerEventData e)
    {
        _rect.DOScale(1f, 0.12f).SetEase(Ease.OutCubic);
        _text.text = $"[ {_label} ]";  // ¿ø·¡´ë·Î
    }

    public void OnPointerClick(PointerEventData e)
    {
        _rect.DOKill();
        _rect.DOPunchScale(Vector3.one * -0.08f, 0.2f, 5, 0.5f);
    }

}
