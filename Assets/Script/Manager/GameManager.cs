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
    NoteManager theNote;    // 추가
    AudioManager theAudio;  // 추가

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

        // 현재 BGM 재생시간
        double now = AudioSettings.dspTime - theNote.scheduledStartDSPTime;
        double endTime = theNote.lastNoteTimeSec + 1.5f; // 마지막 노트보다 1.5초 여유

        if (now >= endTime)
        {
            Debug.Log("[GameManager] 마지막 노트 시간 도달 → 게임 자동 종료");
            EndGameWithResult();
        }
    }

    public void StartGame(int p_songNum)
    {

        //점수 및 콤보 초기화

        theScore.ResetScore();
        theScore.ResetCombo();
        theTiming.ResetJudgementRecord();

        currentSongIndex = p_songNum;

        theResult.SetCurrentSong(p_songNum);

        // StageManager에 스테이지 로드 명령
        if (theStage != null)
            theStage.LoadSong(p_songNum);
        else
            Debug.LogError("[GameManager] StageManager.instance가 null입니다!");

        for (int i = 0; i< goGameUI.Length; i++)
        {
            goGameUI[i].gameObject.SetActive(true);
        }

        stageVideo.RestartVideo();

        isStartGame = true;
        Debug.Log("[GameManager] 게임 시작됨!");

        // 여기서 NoteManager에게 음악 시작 요청
        if (NoteManager.instance != null)
            NoteManager.instance.StartMusic();
    }

    public void ExitGame()
    {

        for (int i = 0; i < goGameUI.Length; i++)
        {
            goGameUI[i].gameObject.SetActive(false);
        }

        // 재생 중인 노트매니저 초기화
        if (NoteManager.instance != null)
        {
            NoteManager.instance.StopAllCoroutines(); // 혹시 잔여 코루틴 있으면 중단
            NoteManager.instance.ResetForReplay();    // 상태 초기화
        }

        // BGM도 멈추기
        if (AudioManager.instance != null)
            AudioManager.instance.StopBGM();

        // 게임 종료 플래그
        isStartGame = false;

        // 현재 곡 index도 초기화
        currentSongIndex = 0;
        
        //캐릭터 끄기
        if (StageManager.instance != null)
            StageManager.instance.DeactivateAllCharacters();
        Debug.Log("[GameManager] 게임 종료 및 인덱스 초기화 완료");
    }


    //게임 종료를 감지해서 Result 창 호출 
    public void EndGameWithResult()
    {
        if (!isStartGame) return;

        isStartGame = false;

        Debug.Log("[GameManager] EndGameWithResult() 호출됨");

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
