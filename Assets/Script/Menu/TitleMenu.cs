using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleMenu : MonoBehaviour
{

    [SerializeField] GameObject goStartUI = null;

    [SerializeField] Animator starAnimator;


    void OnEnable()
    {
        if (starAnimator != null)
            starAnimator.Play("Idle", -1, 0f);
    }


    public void BtnPlay()
    {
        // Debug.Log("[TitleMenu] BtnPlay 클릭됨");

        if (LoadingManager.instance != null)
            LoadingManager.instance.LoadScene("PrologueScene");
        else
            Debug.LogError("LoadingManager.instance가 null입니다!");
    }


    // 프롤로그 스킵 후 돌아왔을 때 호출됨
    public void AfterPrologue ()
    {
        if (goStartUI != null)
            goStartUI.SetActive(true);

        this.gameObject.SetActive(false);
    }

}
    /*
    public void BtnPlay()
    {
        //LoadingManager.instance.FadeQuick();

        LoadingManager.instance.LoadScene("PrologueScene");
        goStartUI.SetActive(true);
        this.gameObject.SetActive(false);

    }

    */



