using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    [SerializeField] GameObject[] goGameUI = null;

    public static GameManager instance;

    public bool isStartGame = false;
    public int currentSongIndex = 0;

    ScoreManager theScore;
    TimingManager theTiming;
    StageManager theStage;
    StageVideoController stageVideo;
    Result theResult;
  //  StageMenu theStageMenu;

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        theScore= FindObjectOfType<ScoreManager>();
        theTiming = FindObjectOfType<TimingManager>();
        theStage = FindObjectOfType<StageManager>();
        stageVideo = FindObjectOfType<StageVideoController>();
        theResult = FindObjectOfType<Result>();
      //  theStageMenu =  FindObjectOfType<StageMenu>();
    }

    public void StartGame(int p_songNum)
    {

        //���� �� �޺� �ʱ�ȭ

        theScore.ResetScore();
        theScore.ResetCombo();
        theTiming.ResetJudgementRecord();

        currentSongIndex = p_songNum;

        theResult.SetCurrentSong(p_songNum);

        // StageManager�� �������� �ε� ���
        if (theStage != null)
            theStage.LoadSong(p_songNum);
        else
            Debug.LogError("[GameManager] StageManager.instance�� null�Դϴ�!");

        for (int i = 0; i< goGameUI.Length; i++)
        {
            goGameUI[i].gameObject.SetActive(true);
        }

        stageVideo.RestartVideo();

        isStartGame = true;
        Debug.Log("[GameManager] ���� ���۵�!");

        if (NoteManager.instance != null)
            NoteManager.instance.ResetForReplay();

        // ���⼭ NoteManager���� ���� ���� ��û
        if (NoteManager.instance != null)
            NoteManager.instance.StartMusic();
    }

    public void ExitGame()
    {

        for (int i = 0; i < goGameUI.Length; i++)
        {
            goGameUI[i].gameObject.SetActive(false);
        }

        // ��� ���� ��Ʈ�Ŵ��� �ʱ�ȭ
        if (NoteManager.instance != null)
        {
            NoteManager.instance.StopAllCoroutines(); // Ȥ�� �ܿ� �ڷ�ƾ ������ �ߴ�
            NoteManager.instance.ResetForReplay();    // ���� �ʱ�ȭ
        }

        // BGM�� ���߱�
        if (AudioManager.instance != null)
            AudioManager.instance.StopBGM();

        // ���� ���� �÷���
        isStartGame = false;

        // ���� �� index�� �ʱ�ȭ
        currentSongIndex = 0;
        
        //ĳ���� ����
        if (StageManager.instance != null)
            StageManager.instance.DeactivateAllCharacters();
        Debug.Log("[GameManager] ���� ���� �� �ε��� �ʱ�ȭ �Ϸ�");
    }

}
