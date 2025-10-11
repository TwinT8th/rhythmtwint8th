using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageMenu : MonoBehaviour
{


    [SerializeField] GameObject TitleMenu = null;
    [SerializeField] Animator playAnim = null;

    // Start is called before the first frame update


    public void BtnPlay ()
    {
        StartCoroutine(PlayBtn());
        GameManager.instance.StartGame();
        this.gameObject.SetActive(false);
    }

    public void BtnTitle()
    {
        TitleMenu.SetActive(true);
        this.gameObject.SetActive(false);
    }

    private IEnumerator PlayBtn()
    {
        playAnim.SetTrigger("Click");
        yield return new WaitForSeconds(1f);
    }

}



