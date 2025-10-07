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
    public AudioSource music; //BGM0 AudioSource 할당
    public float bpm = 90f; //1분당 노트 몇 개 생성 - 곡 교체시 받아오기.
    public double songStartOffsetSec = 0d; //음악 시작 시점(초), 필요 없으면 0
    //double currentTime = 0d; //노트 생성을 위한 시간을 체크할 변수
    public CSVLoader csvLoader;

    [Header("스폰")]
    [SerializeField] Transform noteParent = null; // 노트들을 담을 부모 오브젝트
    //[SerializeField] GameObject notePrefab = null; //노트 프리펩을 담을 변수

    private List<NoteEvent> pattern; //CSV에서 읽은 데이터
    private List<double> spawnTimesSec;     //각 노트의 절대 스폰시각(초)
    private List<double> targetTimesSec; //추가: 타겟 시각(초, 맞춰야 하는 순간)
    private int nextIndex = 0; //다음 스폰할 노트 인덱스

    [Header("DSP Schedule")]
    [SerializeField] double leadInSec = 1.0d; // 오디오 예약 여유 시간(초)
    private double scheduledStartDSPTime; // 오디오가 시작될 DSP 시각(절대)

    [Header("Approach Settings")]
    [SerializeField] public float approachBeats = 2f; // 추가: 타겟보다 몇 박자 먼저 나타날지 (타이밍서클/히트마커 길이와 동일)

    void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        //CSV에서 패턴 가져오기
        pattern = csvLoader.GetPattern();
        PrecomputeSpawnTimes();// @ 스폰/타겟 시각 모두 계산
        nextIndex = 0;

        // 2) 오디오 예약 시작: 현재 DSP 시각 + leadInSec 에 정확히 시작
        if (music != null && music.clip != null)
        {
            //PlayOnAwake는 꺼져있어야 함 (Inspector)
            scheduledStartDSPTime = AudioSettings.dspTime + leadInSec;
            music.PlayScheduled(scheduledStartDSPTime);
        }

    }


    // Update is called once per frame
    void Update()
    {

        if (music == null || music.clip == null) return;
        if (pattern == null || pattern.Count == 0) return;

        //“노래 시작 기준 0초”로 보정된 현재 시각(초). 이 값이 노래 시작 순간 0이 되도록 맞춰야
        double nowSec = AudioSettings.dspTime - scheduledStartDSPTime; 

        //offset을 빼서 "패턴 기준 0초"와 정렬
        nowSec -= songStartOffsetSec;

        //같은 프레임에 여러 노트가 대상이면 모두 처리. 스폰 기준으로 비교 (타겟이 아님)
        while (nextIndex<spawnTimesSec.Count&& nowSec >= spawnTimesSec[nextIndex])
            {
            SpawnNote(pattern[nextIndex], nextIndex);
            nextIndex++;
            }

    }

    private void PrecomputeSpawnTimes() //CSV에서 가져온 beat를 실제 절대시간(초)로 한번에 계산해두는 과정
    {
        spawnTimesSec = new List<double>(pattern.Count);
        targetTimesSec = new List<double>(pattern.Count); // @ 추가

        double secPerBeat = 60 / bpm;
        double approachSec = approachBeats * secPerBeat;  // @ 타겟보다 미리 나올 시간(초)

        for (int i = 0; i < pattern.Count; i++)
        {
            //  @ 타겟 시각(패턴 기준 절대시간, 초) (@원래 이거였음)각 노트의 절대 스폰시각(초) = 오프셋 + beat * (60/BPM)
            double target = (double)songStartOffsetSec + (double)pattern[i].beat * secPerBeat;
            targetTimesSec.Add(target);

            double spawn = target - approachSec; // @ 스폰 시각 = 타겟 시각 - 접근 시간
            spawnTimesSec.Add(spawn);
            Debug.Log($"[SpawnTable] beat={pattern[i].beat}, spawn={spawn:F3}s, target={target:F3}s");
        }
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
    }

    // 필요하다면 Note 컴포넌트에 목표 beat/시간 전달(판정/타이밍용)
    // var comp = note.GetComponent<Note>();
    // if (comp != null) comp.Init(targetBeat: e.beat, bpm: bpm);
}


