using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimingManager : MonoBehaviour
{

    public static TimingManager instance;

    [Header("���� ����(�� ����)")]
    public float perfectRange = 0.05f;
    public float greatRange = 0.1f;
    public float goodRange = 0.2f;
    public float badRange = 0.3f;
    public float missRange = 0.4f;


    [Header("�߾� ĳ����")]
    public CharactorController charactor; // Animator ��� CharacterController ����

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
    }

    public string GetJudgement(double diff)
    {
        diff = Mathf.Abs((float)diff);

        // (����, ���) ���� �迭�� ����
        (float range, string result)[] judgements = new (float, string)[]
        {
        (perfectRange, "Perfect"),
        (greatRange,   "Great"),
        (goodRange,    "Good"),
        (badRange,     "Bad"),
        (missRange,     "Miss"),
        };

        // for������ ������� �˻�
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
