using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Result : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] GameObject goUI = null;
 [SerializeField] Animator boardAnimator = null; //Board에 붙은 Animator
    [SerializeField] GameObject ResultImg = null;
    [SerializeField] TMP_Text[] txtJudgement = null; // Board의 자식 TMP 오브젝트들 (Perfect, Great, 등)
    [SerializeField] GameObject scoreGauge = null;
    [SerializeField] TMP_Text[] txtRestItems = null;
    [SerializeField] GameObject BackBtn = null;

    [SerializeField] TMP_Text[] txtCount = null;
    [SerializeField] TMP_Text txtScore = null;
    [SerializeField] TMP_Text txtMaxCombo = null;
    [SerializeField] TMP_Text txtRank = null;

    ScoreManager theScore;
    TimingManager theTiming;


    [Header("애니메이션 타이밍")]
    [SerializeField] private float delayBeforeResults = 1f; //Board 애니 끝난 뒤 대기 시간
    [SerializeField] private float delayAfterResults = 1f;
   [SerializeField] private float intervalBtwItems = 0.5f;

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
}
    /*
        //UI 활성화
        goUI.SetActive(true);

        //기존에 있던 모든 텍스트 초기화
        for (int i = 0; i < txtCount.Length; i++)
        {
            txtCount[i].text = "0";
        }
        txtMaxCombo.text = "0";
        txtRank.text = "A";
    }
    */


