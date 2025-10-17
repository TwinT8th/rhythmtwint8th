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

    // ���� �� (Button�� onClick ���)
    public void OnPointerDown(PointerEventData eventData)
    {
        isHolding = true;
        if (parentNote != null)
            parentNote.OnHit();  // ���� ���� �״��

        // ��Ÿ�� ��Ʈ�� �ٷ� ������� ��
        gameObject.SetActive(false);
    }

    // ���� �� �� (�ճ�Ʈ��)
    public void OnPointerUp(PointerEventData eventData)
    {
        isHolding = false;
        // ���߿� �ճ�Ʈ�� ����Ǹ� ���⿡ �������� ������ �߰� ����
    }

    public void HideHitMarker()
    {
        if (markerImage != null)
            markerImage.enabled = false;
    }
}