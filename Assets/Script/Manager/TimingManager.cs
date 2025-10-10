using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimingManager : MonoBehaviour
{

    ScoreManager theScore;

    public static TimingManager instance;

    [Header("���� ����(�� ����)")]
    public float perfectRange = 0.05f;
    public float greatRange = 0.1f;
    public float goodRange = 0.2f;
    public float badRange = 0.3f;
    public float missRange = 0.4f;


    [Header("�߾� ĳ����")]
    public CharactorController charactor; // Animator ��� CharacterController ����

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

        // (����, ���) ���� �迭�� ����
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

        theScore.IncreaseScore(index);        // ���� ����
        judgementRecord[index]++;        //���� ���

        // ĳ���� ����
        string[] resultNames = { "Perfect", "Great", "Good", "Bad", "Miss" };
        string result = resultNames[index];
        CharactorAct(result);

        // ���� ����Ʈ ǥ��
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
