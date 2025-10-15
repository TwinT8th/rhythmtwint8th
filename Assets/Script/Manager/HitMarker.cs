using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;



public class HitMarker : MonoBehaviour
{

    private Button hitBtn;
    private Note parentNote;   // �θ� Note ��ũ��Ʈ ����

    [SerializeField] private Image markerImage; //MarkerSprite �ڽ� Image

    void Awake()
    {
        hitBtn = GetComponent<Button>();
        parentNote = GetComponentInParent<Note>();

        if (hitBtn != null)
            hitBtn.onClick.AddListener(OnButtonClick);
        

        markerImage.enabled = true;

        if (markerImage == null)
        {
            markerImage = GetComponentInChildren<Image>(); // MarkerSprite �ڵ� �˻�
            //Debug.Log($"[HitMarker] markerImage �ڵ� �Ҵ�: {(markerImage != null ? markerImage.name : "null")}", this);
        }
        if (markerImage != null)
        {
            markerImage.enabled = true;
        }

    }

    public void OnButtonClick()
    {

        //Debug.Log($"[HitMarker] ��ư ���� at DSP={AudioSettings.dspTime:F3}", this);

        if (parentNote !=null)
        {
            parentNote.OnHit(); //�θ� Note���� "����"����
      
        }

        gameObject.SetActive(false); ; // ���� ���� ��ư ��ü�� ������
    }

    public void HideHitMarker()
    {
        if (markerImage != null)
        {
            markerImage.enabled = false;
        }

    }


}

