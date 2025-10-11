using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class StartMenu : MonoBehaviour
{

    [SerializeField] GameObject goStageUI = null;


    //[SerializeField] Animator ���� ���� ����;

    // ���߿� �� ���� ����� ���⼭ �ƿ� �� ��ȯ 


    void Start()
    {
        VideoManager.instance.RestartEarth();
    }


    void OnEnable()
    {

       VideoManager.instance.RestartEarth();

    }


    public void BtnSelectEP()
    {
        VideoManager.instance.PauseEarth();
        goStageUI.SetActive(true);
        this.gameObject.SetActive(false);
    }
}


