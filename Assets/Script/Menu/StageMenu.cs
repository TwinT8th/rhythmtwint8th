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
    [SerializeField] Animator spinningDisk = null;

    // Start is called before the first frame update


    [SerializeField] Song[] songList = null;

    [SerializeField] TMP_Text txtSongName = null;
    [SerializeField] Image imgDisk = null;


    int currentSong = 0;

    void Start()
    {
        SettingSong();
    }

    void OnEnable()
    {
        if (spinningDisk != null)
        {
            // Animator 리셋
            spinningDisk.Rebind();
            spinningDisk.Update(0);

            // Transform 회전값도 초기화
            spinningDisk.transform.rotation = Quaternion.identity;

            Debug.Log("[StageMenu] Disk 회전 초기화 완료");
        }
    }


    public void BtnNext()
    {

        //AudioManager.instance.PlaySFX("Touch");

        if (++currentSong > songList.Length - 1)
            currentSong = 0;
        SettingSong();
 
    }

    public void BtnPrior()
    {
        //AudioManager.instance.PlaySFX("Touch");

        if (--currentSong < 0)
            currentSong = songList.Length - 1;
        SettingSong();

    }

    void SettingSong()
    { 
        txtSongName.text = songList[currentSong].name;
        imgDisk.sprite = songList[currentSong].sprite;

        AudioManager.instance.PlayBGM("BGM" + currentSong);
    }



    public void BtnPlay ()
    {
        StartCoroutine(PlayBtn());
        GameManager.instance.currentSongIndex = currentSong;
        GameManager.instance.StartGame(currentSong);

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

        AudioManager.instance.StopBGM();//프리뷰 중단

        GameManager.instance.StartGame(currentSong);
    }



}



