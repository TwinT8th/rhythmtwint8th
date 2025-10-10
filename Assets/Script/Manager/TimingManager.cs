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


    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
    }


    void Start()
    {
        theScore = FindObjectOfType<ScoreManager>();
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

        theScore.IncreaseScore(index);        // 점수 증가
        judgementRecord[index]++;        //판정 기록

        // 캐릭터 연출
        string[] resultNames = { "Perfect", "Great", "Good", "Bad", "Miss" };
        string result = resultNames[index];
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



}
