using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimingManager : MonoBehaviour
{

    ScoreManager theScore;

    public static TimingManager instance;

    [Header("판정 범위(초 단위)")]
    public float perfectRange = 0.05f;
    public float greatRange = 0.1f;
    public float goodRange = 0.2f;
    public float badRange = 0.3f;
    public float missRange = 0.4f;


    [Header("중앙 캐릭터")]
    public CharactorController charactor; // Animator 대신 CharacterController 참조

    int[] judgementRecord = new int[5];
    private class HoldTrack
    {
        public LongNote note;
        public double nextTickDSP;
        public double tickInterval; // 초
    }
    private readonly Dictionary<LongNote, HoldTrack> holds = new();



    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
    }


    void Start()
    {
        theScore = FindObjectOfType<ScoreManager>();
    }

    public void Update()
    {
        double now = AudioSettings.dspTime;
        // 모든 진행 중 롱노트에 대해 틱 처리
        foreach (var kv in holds)
        {
            var h = kv.Value;
            while (now >= h.nextTickDSP)
            {
                bool ok = noteInRange(kv.Key); // ← inRange 여부 질의; 구현은 LongNoteHead에서 상태를 노출 or 콜백
                if (ok)
                {
                    theScore.IncreaseHoldTick(); // ★ ScoreManager에 틱 점수 함수 하나 추가
                                                 // (선택) 콤보 +1, 이펙트 스파크 등
                }
                h.nextTickDSP += h.tickInterval;
            }
        }
    }
    public int GetJudgement(double diff)
    {
        diff = Mathf.Abs((float)diff);

        // (범위, 결과) 쌍을 배열로 정의
        float[] ranges = { perfectRange, greatRange, goodRange, badRange, missRange };

        for (int i = 0; i < ranges.Length; i++)
        {
            if (diff <= ranges[i])
                return i;
        }

        return 4; // Miss
    }
    public void ProcessJudgement(Note note, double diff)
    {

        int index = GetJudgement(diff);

        // 캐릭터 연출
        string[] resultNames = { "Perfect", "Great", "Good", "Bad", "Miss" };
        string result = resultNames[index];

        //Debug.Log($"[TimingManager] 판정: {result}, charactor={(charactor ? charactor.name : "NULL")}");


        theScore.IncreaseScore(index);        // 점수 증가
        judgementRecord[index]++;        //판정 기록


        CharactorAct(result);

        // 판정 이펙트 표시
        if (note != null)
            note.ShowJudgementEffect(index);

        return;

    }

    public void CharactorAct(string result)
    {
        if (charactor != null)
        {
            charactor.JudgementAct(result);
        }
          
    }

    public int[] GetJudgementRecord()
    {
        return judgementRecord;
    }

    public void MissRecord()
    {
        judgementRecord[4]++;
    }
    public void ResetJudgementRecord()
    {
        for (int i = 0; i < judgementRecord.Length; i++)
            judgementRecord[i] = 0;

        //Debug.Log("[TimingManager] 판정 기록 초기화 완료");
    }

    private Dictionary<LongNote, double> holdStartTimes = new Dictionary<LongNote, double>();

    /// <summary>
    /// 롱노트 홀드 시작 시점 기록
    /// 홀드 시작 시각을 딕셔너리에 저장 - 나중에 EndHoldJudge에서 지속시간을 비교할 때 사용
    /// </summary>
    public void StartHoldJudge(LongNote note)
    {
        Debug.Log($"[TimingManager] StartHoldJudge() registered for {note.name}");
        if (note == null) return;
        holdStartTimes[note] = AudioSettings.dspTime;

        // BPM 기반 틱 스케줄
        double secPerBeat = 60.0 / NoteManager.instance.bpm;

        // 1박자에 2틱 → 박자당 두 번 틱이 울림 (1박자 1틱 원하면 /1.0으로)
        double interval = secPerBeat / 2.0;

        holds[note] = new HoldTrack
        {
            note = note,
            nextTickDSP = AudioSettings.dspTime + interval,
            tickInterval = interval
        }; 
    }

    /// <summary>
    /// 롱노트 홀드 종료 시점 비교 후 판정
    /// LongNoteHead의 OnPointerUp()에서 호출됨
    /// 결과를 ScoreManager / CharactorAct() / judgementRecord에 반영
    /// </summary>
    public void EndHoldJudge(LongNote note, double holdDuration)
    {
        if (note == null) return;

        double now = AudioSettings.dspTime;
        double startTime;

        if (!holdStartTimes.TryGetValue(note, out startTime))
        {
            Debug.LogWarning("[TimingManager] StartHoldJudge() 기록 없음 → Miss 처리");
            theScore.IncreaseScore(4); // Miss
            judgementRecord[4]++;
            return;
        }

        // 지속시간 기준 판정
        // 예시: 1초 이상이면 Perfect, 0.7~1.0은 Great, 0.4~0.7은 Good, 0.2~0.4은 Bad, 그 이하는 Miss
        float duration = (float)holdDuration;
        int index = 4; // Miss 기본

        if (duration >= 1.0f) index = 0;
        else if (duration >= 0.7f) index = 1;
        else if (duration >= 0.4f) index = 2;
        else if (duration >= 0.2f) index = 3;
        else index = 4;

        string[] resultNames = { "Perfect", "Great", "Good", "Bad", "Miss" };
        string result = resultNames[index];

        Debug.Log($"[TimingManager] EndHoldJudge() {note.name} result={result}, duration={duration:F2}s");

        theScore.IncreaseScore(index);
        judgementRecord[index]++;
        CharactorAct(result);

        note.ShowJudgementEffect(index);//Tail에서 판정 표시
        holds.Remove(note);
        holdStartTimes.Remove(note);
    }

    public void BreakHold(LongNote note)
    {
        holds.Remove(note);
        holdStartTimes.Remove(note);
        // (선택) 콤보 끊김/Bad 처리 등
    }

    // 예시: inRange 질의 (설계에 따라 LongNoteHead가 상태 제공) dkwlr
    private bool noteInRange(LongNote note)
    {
        return note != null && note.IsFingerInRange; // LongNote에 bool 노출 or Head로부터 받은 상태 캐시
    }

}
