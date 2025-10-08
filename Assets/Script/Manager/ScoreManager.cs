using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{

    [SerializeField] TMP_Text txtScore = null;
    [SerializeField] int increaseScore = 10; //몇점씩 올라갈지
    int currentScore = 0; //현재 점수 

    [SerializeField] float[] weight = null; //판정에 따른 가중치

    Animator myAnim;
    string animScoreUp = "ScoreUp";

    // Start is called before the first frame update
    void Start()
    {
        myAnim = GetComponent<Animator>();
        currentScore = 0;
        txtScore.text = "0"; //초기화
    }

    public void IncreaseScore(int p_JudgementState) //어떤 노트 판정을 받아왔는지
    {

        Debug.Log($"[ScoreManager] 호출됨! 판정={p_JudgementState}");
        int t_increaseScore = increaseScore;

        //가중치 계산
        t_increaseScore = (int)(t_increaseScore * weight[p_JudgementState]);    

        currentScore += t_increaseScore;
        txtScore.text = string.Format("{0:#,##0}", currentScore);

        myAnim.SetTrigger(animScoreUp);

    }

}
