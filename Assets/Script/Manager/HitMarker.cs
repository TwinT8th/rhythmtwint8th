using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HitMarker : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private Note parentNote;
    [SerializeField] private Image markerImage;

    private bool isHolding = false;

    void Awake()
    {
        parentNote = GetComponentInParent<Note>();

        if (markerImage == null)
            markerImage = GetComponentInChildren<Image>();

        if (markerImage != null)
            markerImage.enabled = true;
    }

    // 누를 때 (Button의 onClick 대신)
    public void OnPointerDown(PointerEventData eventData)
    {
        isHolding = true;
        if (parentNote != null)
            parentNote.OnHit();  // 기존 동작 그대로

        // 단타형 노트면 바로 사라져도 됨
        gameObject.SetActive(false);
    }

    // 손을 뗄 때 (롱노트용)
    public void OnPointerUp(PointerEventData eventData)
    {
        isHolding = false;
        // 나중에 롱노트가 적용되면 여기에 “릴리즈 판정” 추가 가능
    }

    public void HideHitMarker()
    {
        if (markerImage != null)
            markerImage.enabled = false;
    }
}