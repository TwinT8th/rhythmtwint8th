using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleMenu : MonoBehaviour
{

    [SerializeField] GameObject goStartUI=null;

    [SerializeField] Animator starAnimator;


    void OnEnable()
    {
        starAnimator.Play("Idle", -1, 0f);
    }
    public void BtnPlay()
    {
        //LoadingManager.instance.FadeQuick();
        goStartUI.SetActive(true);
        this.gameObject.SetActive(false);
    }



}
