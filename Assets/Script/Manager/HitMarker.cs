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

    // Ŭ�� �� - ��Ÿ�� ��Ʈ �Է� ó��
    public void OnPointerDown(PointerEventData eventData)
    {
        if (parentNote != null)
        {
            parentNote.OnHit(); // Note�� ���� ����
        }

        // Ŭ���� ���� HitMarker ��Ȱ��ȭ (��Ÿ���� ��� �����)
        gameObject.SetActive(false);
    }

    // �ܺο��� ������ ���� �� (Miss ��)
    public void HideHitMarker()
    {
        if (markerImage != null)
            markerImage.enabled = false;
    }
}