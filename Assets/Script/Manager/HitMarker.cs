using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HitMarker : MonoBehaviour, IPointerDownHandler
{
    private Note parentNote;
    [SerializeField] private Image markerImage;

    void Awake()
    {
        parentNote = GetComponentInParent<Note>();

        if (markerImage == null)
            markerImage = GetComponentInChildren<Image>();

        if (markerImage != null)
            markerImage.enabled = true;
    }

    // 클릭 시 - 단타형 노트 입력 처리
    public void OnPointerDown(PointerEventData eventData)
    {
        if (parentNote != null)
        {
            parentNote.OnHit(); // Note에 판정 전달
        }

        // 클릭한 순간 HitMarker 비활성화 (단타형은 즉시 사라짐)
        gameObject.SetActive(false);
    }

    // 외부에서 강제로 숨길 때 (Miss 등)
    public void HideHitMarker()
    {
        if (markerImage != null)
            markerImage.enabled = false;
    }
}