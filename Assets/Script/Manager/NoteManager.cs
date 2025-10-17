using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;


// 곡 교체시 BPM 변화 구현해야 함.

public class NoteManager : MonoBehaviour
{
    public static NoteManager instance;

    [Header("음악, 패턴")]
    //public AudioSource music; //BGM0 AudioSource 할당
    public float bpm = 90f; //1분당 노트 몇 개 생성 - 곡 교체시 받아오기.
    public double songStartOffsetSec = 0d; //음악 시작 시점(초), 필요 없으면 0
    public CSVLoader csvLoader;

    [Header("스폰")]
    [SerializeField] Transform noteParent = null; // 노트들을 담을 부모 오브젝트


    private List<NoteEvent> pattern; //CSV에서 읽은 데이터
    private List<double> spawnTimesSec;     //각 노트의 절대 스폰시각(초)
    private List<double> targetTimesSec; //추가: 타겟 시각(초, 맞춰야 하는 순간)
    private int nextIndex = 0; //다음 스폰할 노트 인덱스

    [Header("DSP Schedule")]
    [SerializeField] double leadInSec = 1.0d; // 오디오 예약 여유 시간(초)
    private double scheduledStartDSPTime; // 오디오가 시작될 DSP 시각(절대)

    [Header("Approach Settings")]
    [SerializeField] public float approachBeats = 2f; // 추가: 타겟보다 몇 박자 먼저 나타날지 (타이밍서클/히트마커 길이와 동일)

    public double lastNoteTimeSec = 0; // 마지막 노트의 시간
    private bool isLastNoteProcessed = false;
    int songIndex = 0;

//    StageVideoController stageVideo;


    void Awake()
    {
        instance = this;

    }


    void Start()
    {
   //     stageVideo = FindObjectOfType<StageVideoController>();
    }

    // Update is called once per frame
    void Update()
    {



        if (GameManager.instance.isStartGame)
        {

            if (AudioManager.instance == null) return;
            if (pattern == null || pattern.Count == 0) return;

            //“노래 시작 기준 0초”로 보정된 현재 시각(초). 이 값이 노래 시작 순간 0이 되도록 맞춰야
            double nowSec = AudioSettings.dspTime - scheduledStartDSPTime;

            //offset을 빼서 "패턴 기준 0초"와 정렬
            nowSec -= songStartOffsetSec;


            //같은 프레임에 여러 노트가 대상이면 모두 처리. 스폰 기준으로 비교 (타겟이 아님)
            while (nextIndex < spawnTimesSec.Count && nowSec >= spawnTimesSec[nextIndex])
            {
                SpawnNote(pattern[nextIndex], nextIndex);
                nextIndex++;
            }
        }


    }

    public void StartMusic()
    {

        // GameManager에서 현재 선택된 곡 인덱스 받아오기
        songIndex = GameManager.instance.currentSongIndex;


        //CSV패턴 로드

        pattern = csvLoader.GetPattern(songIndex);
        if(pattern == null || pattern.Count == 0)
        {
            Debug.LogError($"[NoteManager] pattern{songIndex}.csv 로드 실패!");
            return;
        }

        // 2) 스폰타임 계산
        PrecomputeSpawnTimes();
        nextIndex = 0;

        // 오디오 예약 시작: 현재 DSP 시각 + leadInSec 에 정확히 시작
        if (AudioManager.instance != null)
        {
            scheduledStartDSPTime = AudioSettings.dspTime + leadInSec;

            // Inspector에서 bgm 리스트 안에 곡 이름 맞춰둬야 함. 스테이지 매니저 구현하면서 수정
            AudioManager.instance.PlayBGM("BGM" + songIndex, leadInSec);
            Debug.Log($"[NoteManager] BGM{songIndex} + pattern{songIndex}.csv 시작 예정");

        }

        else
        {
            Debug.LogWarning("[NoteManager] AudioManager instance가 없습니다. 음악 재생 불가");
        }

    }

    private void LoadPattern(string fileName)
    {
        TextAsset patternFile = Resources.Load<TextAsset>(fileName);
        if (patternFile == null)
        {
            Debug.LogError($"[NoteManager] 패턴 파일 '{fileName}.csv'을(를) 찾을 수 없습니다!");
            return;
        }
    }


    private void PrecomputeSpawnTimes() //CSV에서 가져온 beat를 실제 절대시간(초)로 한번에 계산해두는 과정
    {
        spawnTimesSec = new List<double>(pattern.Count);
        targetTimesSec = new List<double>(pattern.Count); 

        double secPerBeat = 60 / bpm;
        double approachSec = approachBeats * secPerBeat;  //타겟보다 미리 나올 시간(초)
        
        lastNoteTimeSec = 0; // 초기화

        for (int i = 0; i < pattern.Count; i++)
        {
            //  @ 타겟 시각(패턴 기준 절대시간, 초) (@원래 이거였음)각 노트의 절대 스폰시각(초) = 오프셋 + beat * (60/BPM)
            double target = (double)songStartOffsetSec + (double)pattern[i].beat * secPerBeat;
            targetTimesSec.Add(target);

            double spawn = target - approachSec; // @ 스폰 시각 = 타겟 시각 - 접근 시간
            spawnTimesSec.Add(spawn);

            // 마지막 노트 시간 갱신
            if (target > lastNoteTimeSec)
                lastNoteTimeSec = target;

            // Debug.Log($"[SpawnTable] beat={pattern[i].beat}, spawn={spawn:F3}s, target={target:F3}s");
        }

        Debug.Log($"[NoteManager] Last Note Time = {lastNoteTimeSec:F3}s");
    }



    /*
  SpawnNote:
  - 스폰은 “미리” 하고,
  - Note.Init에는 “타겟의 절대 DSP 시각”을 넣어줌.
*/
    private void SpawnNote(NoteEvent e, int index)
    {
       //풀에서 오브젝트 꺼내오기
        GameObject note = ObjectPool.instance.noteQueue.Dequeue();

        // 좌표계 결정: 여기서는 부모 기준 localPosition으로 배치
        note.transform.SetParent(noteParent); //부모 지정
        note.transform.localPosition = new Vector3(e.x, e.y, 0f);
        note.transform.localScale = Vector3.one;
        note.SetActive(true);

        // Note 컴포넌트 초기화
        Note comp = note.GetComponent<Note>();
        if (comp != null)
        {
            //  @ 타겟의 절대 DSP 시각 = 오디오 예약 시작 DSP + 타겟(초) 
            double targetDSPTime = scheduledStartDSPTime + targetTimesSec[index];
            comp.Init(targetDSPTime);   // 타겟 “절대” 시각 전달 (Note는 이걸 기준으로 판정)
        }

    }
    public void ReturnNote(Note note)
    {

        note.gameObject.SetActive(false);
        ObjectPool.instance.noteQueue.Enqueue(note.gameObject);

        // 마지막 노트가 처리되었는지 감지
        if (!isLastNoteProcessed && Mathf.Abs((float)(note.targetTimeSec - (scheduledStartDSPTime + lastNoteTimeSec))) < 0.02f)
        {
            isLastNoteProcessed = true;
            Debug.Log("[NoteManager] 마지막 노트 판정 완료!");

            StartCoroutine(ShowResultAfterFade());

            //StageManager.instance.StartEndSequence(); - 아직 스테이지 매니저 구현 안함
        }

    }
    private IEnumerator ShowResultAfterFade()
    {
        float fadeDuration = 3f;

        // 오디오 페이드 아웃
        if (AudioManager.instance != null)
        {
            yield return StartCoroutine(AudioManager.instance.FadeOutBGM(fadeDuration));
        }
        // 비디오 일시정지
        StageVideoController stageVideo = FindObjectOfType<StageVideoController>();
        if (stageVideo != null)
        {
            stageVideo.PauseVideo();   // 완전히 정지
                                      // 또는 stageVideo.PauseVideo(); (일시정지만 하고 싶다면)
        }
        // 페이드 시간 대기
        yield return new WaitForSeconds(fadeDuration);

        // 결과창 표시
        Result resultUI = FindObjectOfType<Result>();
        if (resultUI != null)
        {
            resultUI.ShowResult();
        }
    }

    //패턴에 기록된 총 노트 수
    public int GetTotalNoteCount()
    {
        return (pattern != null) ? pattern.Count : 0;
    }

    public void ResetForReplay()
    {
        //Debug.Log("[NoteManager] ResetForReplay() 호출됨 - 상태 초기화 시작");

        nextIndex = 0;
        isLastNoteProcessed = false;

        // 패턴 다시 로드 (CSVLoader에서 다시 읽기)
        pattern = csvLoader.GetPattern(songIndex);
        PrecomputeSpawnTimes();

        // 모든 노트 오브젝트 비활성화 (혹시 남아있는 게 있으면)
        foreach (Transform child in noteParent)
            child.gameObject.SetActive(false);

        // 오디오도 멈춤
        AudioManager.instance.StopBGM();

    }

}


