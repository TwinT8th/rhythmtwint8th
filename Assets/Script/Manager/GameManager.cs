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
    NoteManager theNote;    // �߰�
    AudioManager theAudio;  // �߰�

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
        theNote = FindObjectOfType<NoteManager>();
        theAudio = FindObjectOfType<AudioManager>();
      //  theStageMenu =  FindObjectOfType<StageMenu>();
    }


    void Update()
    {
        if (!isStartGame) return;
        if (theNote == null || theAudio == null) return;

        // ���� BGM ����ð�
        double now = AudioSettings.dspTime - theNote.scheduledStartDSPTime;
        double endTime = theNote.lastNoteTimeSec + 1.5f; // ������ ��Ʈ���� 1.5�� ����

        if (now >= endTime)
        {
            Debug.Log("[GameManager] ������ ��Ʈ �ð� ���� �� ���� �ڵ� ����");
            EndGameWithResult();
        }
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


    //���� ���Ḧ �����ؼ� Result â ȣ�� 
    public void EndGameWithResult()
    {
        if (!isStartGame) return;

        isStartGame = false;

        Debug.Log("[GameManager] EndGameWithResult() ȣ���");

        StartCoroutine(ShowResultAfterFade());
    }

    private IEnumerator ShowResultAfterFade()
    {
        float fadeDuration = 3f;

        if (theAudio != null)
            yield return StartCoroutine(theAudio.FadeOutBGM(fadeDuration));

        if (stageVideo != null)
            stageVideo.PauseVideo();

        yield return new WaitForSeconds(fadeDuration);

        if (theResult != null)
            theResult.ShowResult();
    }
}
