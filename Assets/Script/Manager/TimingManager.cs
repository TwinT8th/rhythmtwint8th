using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimingManager : MonoBehaviour
{

    public static TimingManager instance;

    [Header("판정 범위(초 단위)")]
    public float perfectRange = 0.05f;
    public float greatRange = 0.1f;
    public float goodRange = 0.2f;
    public float badRange = 0.3f;
    public float missRange = 0.4f;


    [Header("중앙 캐릭터")]
    public CharactorController charactor; // Animator 대신 CharacterController 참조

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
    }

    public string GetJudgement(double diff)
    {
        diff = Mathf.Abs((float)diff);

        // (범위, 결과) 쌍을 배열로 정의
        (float range, string result)[] judgements = new (float, string)[]
        {
        (perfectRange, "Perfect"),
        (greatRange,   "Great"),
        (goodRange,    "Good"),
        (badRange,     "Bad"),
        (missRange,     "Miss"),
        };

        // for문으로 순서대로 검사
        for (int i = 0; i < judgements.Length; i++)
        {
            if (diff <= judgements[i].range)
                return judgements[i].result;
        }

        return "Miss";
    }

   public void CharactorAct(string result)
    {
        if (charactor != null)
        {
            charactor.JudgementAct(result);
        }
          
    }

}
