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
    [SerializeField] TMP_Text txtSongScore = null;
    [SerializeField] TMP_Text txtSongRank = null; 
    [SerializeField] Image imgDisk = null;


    int currentSong = 0;

    DatabaseManager theDatabase;


   

    void OnEnable()
    {
        if(theDatabase == null)
          theDatabase = FindObjectOfType<DatabaseManager>();
        SettingSong();

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
        txtSongScore.text = string.Format("{0:#,##0}", theDatabase.score[currentSong]);
        txtSongRank.text = theDatabase.rank[currentSong];

        AudioManager.instance.PlayBGM("BGM" + currentSong);
    }



    public void BtnPlay ()
    {
        if (spinningDisk != null)
        {
            // Animator ����
            spinningDisk.Rebind();
            spinningDisk.Update(0);

            // Transform ȸ������ �ʱ�ȭ
            spinningDisk.transform.rotation = Quaternion.identity;

            Debug.Log("[StageMenu] Disk ȸ�� �ʱ�ȭ �Ϸ�");
        }

        StartCoroutine(PlayBtn());
        GameManager.instance.currentSongIndex = currentSong;
        GameManager.instance.StartGame(currentSong);

        this.gameObject.SetActive(false);
    }

    public void BtnTitle()
    {


        AudioManager.instance.StopBGM(); // ������ �ߴ�

        if (spinningDisk != null)
        {
            // Animator ����
            spinningDisk.Rebind();
            spinningDisk.Update(0);

            // Transform ȸ������ �ʱ�ȭ
            spinningDisk.transform.rotation = Quaternion.identity;

            Debug.Log("[StageMenu] Disk ȸ�� �ʱ�ȭ �Ϸ�");
        }


        TitleMenu.SetActive(true);
        this.gameObject.SetActive(false);
    }


    private IEnumerator PlayBtn()
    {
        playBtnAnim.SetTrigger("Click");
        yield return new WaitForSeconds(1f);

        AudioManager.instance.StopBGM();//������ �ߴ�

        GameManager.instance.StartGame(currentSong);
    }



}



