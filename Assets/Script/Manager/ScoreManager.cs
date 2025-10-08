using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{

    [SerializeField] TMP_Text txtScore = null;
    [SerializeField] int increaseScore = 10; //������ �ö���
    int currentScore = 0; //���� ���� 

    [SerializeField] float[] weight = null; //������ ���� ����ġ

    Animator myAnim;
    string animScoreUp = "ScoreUp";

    // Start is called before the first frame update
    void Start()
    {
        myAnim = GetComponent<Animator>();
        currentScore = 0;
        txtScore.text = "0"; //�ʱ�ȭ
    }

    public void IncreaseScore(int p_JudgementState) //� ��Ʈ ������ �޾ƿԴ���
    {

        Debug.Log($"[ScoreManager] ȣ���! ����={p_JudgementState}");
        int t_increaseScore = increaseScore;

        //����ġ ���
        t_increaseScore = (int)(t_increaseScore * weight[p_JudgementState]);    

        currentScore += t_increaseScore;
        txtScore.text = string.Format("{0:#,##0}", currentScore);

        myAnim.SetTrigger(animScoreUp);

    }

}
