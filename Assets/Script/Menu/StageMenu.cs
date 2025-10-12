using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class Song
{
    public string name;
    public Sprite sprite; 
}



public class StageMenu : MonoBehaviour
{
    [SerializeField] GameObject TitleMenu = null;
    [SerializeField] Animator playBtnAnim = null;
   // [SerializeField] Animator spinningDisk = null;

    // Start is called before the first frame update


    [SerializeField] Song[] songList = null;

    [SerializeField] TMP_Text txtSongName = null;
    [SerializeField] Image imgDisk = null;


    int currentSong = 0;

    void Start()
    {
        SettingSong();
    }


    public void BtnNext()
    {
        if (++currentSong > songList.Length - 1)
            currentSong = 0;
        SettingSong();
 
    }

    public void BtnPrior()
    {
        if(--currentSong < 0)
            currentSong = songList.Length - 1;
        SettingSong();

    }

    void SettingSong()
    { 
        txtSongName.text = songList[currentSong].name;
        imgDisk.sprite = songList[currentSong].sprite;

        //StartCoroutine(SettingSongAnim());
    }



    public void BtnPlay ()
    {
        StartCoroutine(PlayBtn());
        this.gameObject.SetActive(false);
    }

    public void BtnTitle()
    {
        TitleMenu.SetActive(true);
        this.gameObject.SetActive(false);
    }


    private IEnumerator PlayBtn()
    {
        playBtnAnim.SetTrigger("Click");
        yield return new WaitForSeconds(1f);
        GameManager.instance.StartGame();
    }



}



