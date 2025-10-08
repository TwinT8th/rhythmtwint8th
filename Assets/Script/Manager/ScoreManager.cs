using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] TMP_Text txtScore = null;
    [SerializeField] TMP_Text txtCombo = null;
    [SerializeField] GameObject goComboImage = null; // Inspector에 Combo 배경 오브젝트 직접 연결

    private Image comboImage; // ★ goComboImage의 Image 컴포넌트를 캐싱해 둘 변수

    [Header("Score Settings")]
    [SerializeField] int increaseScore = 10;
    [SerializeField] float[] weight = null;

    private int currentScore = 0;
    private int currentCombo = 0;

    private Animator myAnim;
    private string animScoreUp = "ScoreUp";

    void Start()
    {
        myAnim = GetComponent<Animator>();

        // 초기화
        currentScore = 0;
        txtScore.text = "0";
        txtCombo.gameObject.SetActive(false);
        goComboImage.SetActive(false);

        // ★ 여기서 한 번만 캐싱
        if (goComboImage != null)
            comboImage = goComboImage.GetComponent<Image>();
    }

    public void IncreaseScore(int p_JudgementState)
    {
        int t_increaseScore = increaseScore;

        // 콤보 처리
        if (p_JudgementState < 2) // Perfect, Great
            IncreaseCombo();
        else if (p_JudgementState > 3) // Miss
            ResetCombo();

        // 점수 가중치
        t_increaseScore = (int)(t_increaseScore * weight[p_JudgementState]);
        currentScore += t_increaseScore;
        txtScore.text = string.Format("{0:#,##0}", currentScore);

        myAnim.SetTrigger(animScoreUp);
    }

    public void IncreaseCombo(int p_num = 1)
    {
        currentCombo += p_num;
        txtCombo.text = string.Format("{0:#,##0}", currentCombo);

        if (currentCombo >= 1)
        {
            txtCombo.gameObject.SetActive(true);
            goComboImage.SetActive(true);

            // 콤보 색상 변화
            if (currentCombo > 2)
            {
                Color baseColor = Color.white;
                Color redColor = new Color(1f, 0.4f, 0.4f);
                Color goldColor = new Color(1f, 0.85f, 0.3f);

                Color targetColor;
                if (currentCombo < 16)
                    targetColor = baseColor;
                else if (currentCombo < 50)
                {
                    float t = Mathf.InverseLerp(16, 50, currentCombo);
                    targetColor = Color.Lerp(redColor, goldColor, t);
                }
                else
                    targetColor = goldColor;

                txtCombo.color = Color.Lerp(txtCombo.color, targetColor, Time.deltaTime * 8f);

                // ★ comboImage 캐싱 덕분에 바로 접근 가능
                if (comboImage != null)
                    comboImage.color = Color.Lerp(comboImage.color, targetColor, Time.deltaTime * 8f);
            }
        }
    }

    public void ResetCombo()
    {
        currentCombo = 0;
        txtCombo.text = "0";
        txtCombo.gameObject.SetActive(false);
        goComboImage.SetActive(false);
    }
}