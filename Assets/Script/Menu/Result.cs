using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


//아직 Rank 구현 안함. 계산 공식 만들어야 함

public class Result : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] GameObject goUI = null;
    [SerializeField] Animator boardAnimator = null; //Board에 붙은 Animator

    [SerializeField] GameObject ResultImg = null;
    [SerializeField] TMP_Text[] txtJudgement = null; // Board의 자식 TMP 오브젝트들 (Perfect, Great, 등)
    [SerializeField] GameObject scoreGauge = null;
    [SerializeField] GameObject[] scoreGaugeBox = null; //박스 하나씩 채워지게..
    [SerializeField] TMP_Text txtScoreGaugePer = null; //퍼센트

    [SerializeField] TMP_Text[] txtRestItems = null;
    [SerializeField] GameObject BackBtn = null;

    [SerializeField] TMP_Text[] txtCount = null;
    [SerializeField] TMP_Text txtScore = null;
    [SerializeField] TMP_Text txtMaxCombo = null;
    [SerializeField] TMP_Text txtRank = null;

    [SerializeField] GameObject StageMenu = null;


    ScoreManager theScore;
    TimingManager theTiming;


    [Header("애니메이션 타이밍")]
    [SerializeField] private float delayBeforeResults = 0.8f; //Board 애니 끝난 뒤 대기 시간
    [SerializeField] private float delayAfterResults = 0.8f;
    [SerializeField] private float intervalBtwItems = 0.3f;


    // Start is called before the first frame update
    void Start()
    {
        theScore = FindObjectOfType<ScoreManager>();
        theTiming = FindObjectOfType<TimingManager>();

        //시작 시 모든 항목 숨기기
        if (txtJudgement != null)
        {
            foreach (var t in txtJudgement)
                t.gameObject.SetActive(false);
        }
        if (txtRestItems != null)
        {
            foreach (var t in txtRestItems)
                t.gameObject.SetActive(false);
        }

        if (goUI != null)
            goUI.SetActive(false);

        if (ResultImg != null)
            ResultImg.SetActive(false);

        if (scoreGaugeBox != null)
        {
            foreach (var t in scoreGaugeBox)
                t.gameObject.SetActive(false);
        }

        if (scoreGauge != null)
            scoreGauge.SetActive(false);

        if (BackBtn != null)
            BackBtn.SetActive(false);



    }

    public void ShowResult()
    {

        {
            StartCoroutine(ShowResultSequence());
        }
    }
    private IEnumerator ShowResultSequence()
    {
        //1.배경 활성화
        if (goUI != null)
            goUI.SetActive(true);

        //Result 내용 채우기
        int[] t_judgement = theTiming.GetJudgementRecord();
        int t_currentScore = theScore.GetCurrentScore();
        int t_maxCombo = theScore.GetMaxCombo();

        for (int i = 0; i < txtCount.Length; i++)
        {
            txtCount[i].text = string.Format("{0:#,##0}", t_judgement[i]);
        }

        txtScore.text = string.Format("{0:#,##0}", t_currentScore);
        txtMaxCombo.text = string.Format("{0:#,##0}", t_maxCombo);

        // Score와 Rank 계산 (PerfectRate는 이제 사용 안 함)
        int currentScore = theScore.GetCurrentScore();
        int maxScore = theScore.GetMaxPossibleScore();

        // 현재 점수 / 만점 비율
        float scoreRate = (maxScore > 0) ? ((float)currentScore / maxScore) * 100f : 0f;
        scoreRate = Mathf.Floor(scoreRate); // 정수로 변환

        // 💯 스코어 기반 랭크 계산
        string rank;

        if (scoreRate >= 99f)
            rank = "SSS";
        else if (scoreRate >= 90f)
            rank = "S";
        else if (scoreRate >= 80f)
            rank = "A";
        else if (scoreRate >= 70f)
            rank = "B";
        else if (scoreRate >= 60f)
            rank = "C";
        else
            rank = "D";

        txtRank.text = rank;
        txtScoreGaugePer.text = string.Format("{0:0}", scoreRate);



        //2.Board 자식 TMP들 모두 숨기기
        if (ResultImg != null)
            ResultImg.SetActive(false);

        if (scoreGauge != null)
            scoreGauge.SetActive(false);

        if (BackBtn != null)
            BackBtn.SetActive(false);

        if (txtJudgement != null)
        {
            foreach (var t in txtJudgement)
                t.gameObject.SetActive(false);
        }

        if (txtRestItems != null)
        {
            foreach (var t in txtRestItems)
                t.gameObject.SetActive(false);
        }

        // 모든 박스 비활성화
        for (int i = 0; i < scoreGaugeBox.Length; i++)
            scoreGaugeBox[i].SetActive(false);


        //3.Board 애니메이션 재생
        if (boardAnimator != null)
        {
            boardAnimator.SetTrigger("Result"); // 첫 프레임부터 1회 재생
            Debug.Log("[Result] Board 애니메이션 재생 시작");
        }

        //4.애니메이션 재생 후 잠시 대기
        yield return new WaitForSeconds(delayBeforeResults);

        //5️. TMP 오브젝트들 순차적으로 등장 (ScoreGauge는 구현 더 필요)
        if (ResultImg != null)
        {
            ResultImg.SetActive(true);
            yield return new WaitForSeconds(delayAfterResults);

        }

        if (txtJudgement != null)
        {
            for (int i = 0; i < txtJudgement.Length; i++)
            {
                txtJudgement[i].gameObject.SetActive(true);
                yield return new WaitForSeconds(intervalBtwItems);
            }
        }

        if (scoreGauge != null)
        {
            scoreGauge.SetActive(true);

            yield return new WaitForSeconds(delayAfterResults);

        }

        //점수 게이지 순차 애니메이션 시작

        yield return StartCoroutine(AnimateScoreGauge(scoreRate));

        if (txtRestItems != null)
        {
            for (int i = 0; i < txtRestItems.Length; i++)
            {
                txtRestItems[i].gameObject.SetActive(true);
                yield return new WaitForSeconds(intervalBtwItems);
            }
        }
        yield return new WaitForSeconds(intervalBtwItems);

        if (BackBtn != null)
        {
            BackBtn.SetActive(true);
        }

        Debug.Log("[Result] 모든 결과 텍스트 활성화 완료");
    }

    // 점수 퍼센트에 따라 게이지 박스를 순차적으로 켜는 코루틴
    private IEnumerator AnimateScoreGauge(float scoreRate)
    {
        if (scoreGaugeBox == null || scoreGaugeBox.Length == 0)
            yield break;

        // 0~100% → 0~10단계
        int activeBoxes = Mathf.FloorToInt(scoreRate / 10f);
        if (activeBoxes > 10) activeBoxes = 10;
        if (activeBoxes < 0) activeBoxes = 0;

        // 순차적으로 켜기
        for (int i = 0; i < activeBoxes; i++)
        {
            scoreGaugeBox[i].SetActive(true);
            yield return new WaitForSeconds(0.1f); // 한 칸당 간격
        }

        Debug.Log($"[Result] ScoreGauge 애니메이션 완료: {activeBoxes}/10 (scoreRate={scoreRate:F1}%)");
    }


    public void BtnBack()
    {
        GameManager.instance.ExitGame();
        StageMenu.SetActive(true);
        this.gameObject.SetActive(false);
    }


}



