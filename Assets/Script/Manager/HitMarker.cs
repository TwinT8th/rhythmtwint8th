using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;



public class HitMarker : MonoBehaviour
{

    private Button hitBtn;
    private Note parentNote;   // 부모 Note 스크립트 참조

    [SerializeField] private Image markerImage; //MarkerSprite 자식 Image

    void Awake()
    {
        hitBtn = GetComponent<Button>();
        parentNote = GetComponentInParent<Note>();

        if (hitBtn != null)
            hitBtn.onClick.AddListener(OnButtonClick);
        

        markerImage.enabled = true;

        if (markerImage == null)
        {
            markerImage = GetComponentInChildren<Image>(); // MarkerSprite 자동 검색
            //Debug.Log($"[HitMarker] markerImage 자동 할당: {(markerImage != null ? markerImage.name : "null")}", this);
        }
        if (markerImage != null)
        {
            markerImage.enabled = true;
        }

    }

    public void OnButtonClick()
    {

        //Debug.Log($"[HitMarker] 버튼 눌림 at DSP={AudioSettings.dspTime:F3}", this);

        if (parentNote !=null)
        {
            parentNote.OnHit(); //부모 Note에게 "눌림"전달
      
        }

        gameObject.SetActive(false); ; // 눌린 순간 버튼 자체도 꺼버림
    }

    public void HideHitMarker()
    {
        if (markerImage != null)
        {
            markerImage.enabled = false;
        }

    }


}

