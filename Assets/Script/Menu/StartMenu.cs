using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class StartMenu : MonoBehaviour
{

    [SerializeField] GameObject goStageUI = null;


    //[SerializeField] Animator 아직 구현 안함;

    // 나중에 새 게임 만들면 여기서 아예 씬 변환 


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


