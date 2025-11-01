using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Note : MonoBehaviour
{
    [Header("풀 타입")]
    public int poolType = 0;


    public double targetTimeSec;  // 이 노트가 맞아야 할 절대 시각(DSP 기준)
    private bool isResolved = false; // true가 되면 이 노트는 더 이상 판정/연출을 하지 않음
                                 
    
    
    // ▼ 추가
    private bool isInitialized = false;


    // Animator 대신 SpriteAnimatorBPM
    [Header("TimingCircle")]
    [SerializeField] private SpriteAnimatorBPM timingCircleAnim;  //  Animator → SpriteAnimatorBPM

    [Header("HitMarker")]
    [SerializeField] GameObject gohitMarker;
    [SerializeField] private SpriteAnimatorBPM hitMarkerAnim;
    [SerializeField] private Image hitMarkerImg;

[Header("판정 이펙트")]
    [SerializeField] private Animator judgementAnimator;
    [SerializeField] private Image judgementImage; // 판정 스프라이트 교체용
    [SerializeField] private Sprite[] judgementSprites;
    // 0: Perfect, 1: Great, 2: Good, 3: Bad, 4: Miss

    void Awake()
    {
      
        // timingCircleAnim, hitMarkerAnim은 Inspector에서 직접 넣어주는 게 안정적임
        //  자동 검색 부분 수정. "JudgementEffect"라는 이름의 자식에서만 찾도록 제한
        if (judgementAnimator == null)
        {
            Transform judge = transform.Find("JudgementEffect");
            if (judge != null)
                judgementAnimator = judge.GetComponent<Animator>();
        }

        if (judgementImage == null)
        {
            Transform judge = transform.Find("JudgementEffect");
            if (judge != null)
                judgementImage = judge.GetComponent<Image>();
        }

    }
    public void ResetState()
    {
        targetTimeSec = 0;
        isResolved = false;
        isInitialized = false;

        if (gohitMarker != null)
        {
            if (hitMarkerImg != null)
                hitMarkerImg.enabled = true;
        }

        if (timingCircleAnim) { timingCircleAnim.Stop();  }
        if (hitMarkerAnim) { hitMarkerAnim.Stop();  }

        if (judgementImage) judgementImage.enabled = false;
        if (judgementAnimator)
        {
            judgementAnimator.ResetTrigger("Hit");
            judgementAnimator.gameObject.SetActive(true);
            judgementAnimator.enabled = true;
        }
    }

    void OnEnable()
    {

        isResolved = false;


        ResetState();

    }

    void Update()
    {

        if (GameManager.instance.isStartGame)
        {

            // ▼ Init() 호출되기 전 프레임 차단 (핵심)
            if (!isInitialized) return;
            if (isResolved) return;

            double now = AudioSettings.dspTime;
            double lastAllowed = targetTimeSec + (double)TimingManager.instance.missRange;

            // 노트 놓쳤을 떄 Miss 판정 - 여기서 바로 반납X (애니가 돌 기회가 사라짐)
            if (now > lastAllowed)
            {
                ShowJudgementEffect(4); // Miss = index 4
                TimingManager.instance.MissRecord();

                if (TimingManager.instance != null)
                    TimingManager.instance.CharactorAct("Miss");

                // Animator -> SpriteAnimatorBPM 방식으로 Stop 교체
                if (timingCircleAnim != null) timingCircleAnim.Stop();
                if (hitMarkerAnim != null) hitMarkerAnim.StopOnLastFrame();

                isResolved = true;

                //애니메이션이 안전하게 재생될 수 있게 시간 간격 둠
                //CancelInvoke(nameof(SafetyReturn));
                //Invoke(nameof(SafetyReturn), 1.0f);


                StartCoroutine(DelayedReturn(1.0f));

            }
        }
    }

    private void SafetyReturn() // 이벤트 못 받았을 때 대비용
    {
          if (isResolved) return;

    isResolved = true;  // 중복 방지 플래그


        if (gameObject.activeInHierarchy)
        {
            //targetTimeSec = 0;
            Debug.LogWarning("[Note] AnimationEvent 미수신으로 안전 반납 수행", this);

            NoteManager.instance?.ReturnNote(this);

            //ObjectPool.instance.ReturnNote(poolType, gameObject);
        }
    }


    public void Init(double targetTime)
    {

        targetTimeSec = targetTime;

        // @ Animator SetBool 대신 Play() 사용
        //Init()에서만 Play()를 호출하도록 보장
        if (timingCircleAnim != null)
            timingCircleAnim.Play();

        if (hitMarkerAnim != null)
            hitMarkerAnim.Play();

        if (judgementImage != null)
        {
            judgementImage.enabled = false;
        }

        isResolved = false;
        isInitialized = true; // ▼ 이제부터 Update 허용
    }
    public void OnHit()
    {
        if (isResolved) return;

        double now = (double)AudioSettings.dspTime;
        double diff = now - targetTimeSec;


        // TimingManager에서 판정/점수/연출 통합 처리
        if (TimingManager.instance != null)
            TimingManager.instance.ProcessJudgement(this, diff);
        else
            Debug.LogError("[Note] TimingManager.instance is null!");


        // Animator 대신 SpriteAnimatorBPM → Stop()
        if (timingCircleAnim != null) timingCircleAnim.Stop();
        if (hitMarkerAnim != null) hitMarkerAnim.StopOnLastFrame();


        isResolved = true;

        //안전장치: 이벤트가 안 들어오면 1초 뒤 강제 반납
        //CancelInvoke(nameof(SafetyReturn)); 
        //Invoke(nameof(SafetyReturn), 1.0f);

        StartCoroutine(DelayedReturn(1.0f));
    }

    public  void ShowJudgementEffect(int index)
    {
        if (judgementSprites != null && index >= 0 && index < judgementSprites.Length)
        {
            judgementImage.sprite = judgementSprites[index];
            judgementImage.enabled = true;
        }

        if (judgementAnimator != null)
        {
          
            judgementAnimator.gameObject.SetActive(true);
            judgementAnimator.enabled = true;
            judgementAnimator.ResetTrigger("Hit");
            judgementAnimator.SetTrigger("Hit");
          
        }
        else
        {
            Debug.LogError("[Note] judgementAnimator is NULL!", this);
        }

        if (index <= 2)
        {
            StartCoroutine(NotePulse(0.25f));
        }
        else
        {
            StartCoroutine(ShakeEffect(0.2f, 8f, 3f));
        }

    }


    // Animator의 AnimationEvent에서 호출
    public void NotifyNoteFinished()
    {
        // 판정 애니 끝났다고 Manager에 알림
        //Debug.Log("[Note] NotifyNoteFinished() 수신 → 매니저에 반납", this);
        CancelInvoke(nameof(SafetyReturn));

        // 마지막 노트 감지용으로 NoteManager에 보고
        if (NoteManager.instance != null)
            NoteManager.instance.ReturnNote(this);


    }

    //기존 AnimationEvent 대체: 일정 시간 후 반납
    private IEnumerator DelayedReturn(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (NoteManager.instance != null)
            NoteManager.instance.ReturnNote(this);
    }

    private IEnumerator NotePulse(float duration = 0.2f)
    {
        // hitMarker 기준으로 연출 — 필요에 따라 timingCircleAnim.gameObject 로 바꿔도 됨
        if (gohitMarker == null) yield break;

        Transform target = gohitMarker.transform;
        float timer = 0f;
        Vector3 startScale = target.localScale;
        Vector3 targetScale = startScale * 1.25f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            float eased = t * t * (3 - 2 * t); // ease-in-out

            // 처음 절반은 커지고, 후반 절반은 복귀
            float scale = (t < 0.5f)
                ? Mathf.Lerp(1f, 1.25f, eased * 2f)
                : Mathf.Lerp(1.25f, 1f, (eased - 0.5f) * 2f);

            target.localScale = startScale * scale;
            yield return null;
        }

        target.localScale = startScale;

        if (hitMarkerImg != null)
            hitMarkerImg.enabled = false;
    }

    private IEnumerator ShakeEffect(float duration, float shakeMagnitude, float shakeFrequency)
    {
        if (gohitMarker == null) yield break;

        Transform target = gohitMarker.transform;
        Vector3 originalPos = target.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            // 감쇠 (처음엔 세게, 점점 줄어듦)
            float intensity = (1f - progress);

            // 프레임마다 랜덤 진동
            float offsetX = Mathf.Sin(Time.time * shakeFrequency * 20f) * shakeMagnitude * intensity;
            float offsetY = Mathf.Cos(Time.time * shakeFrequency * 15f) * shakeMagnitude * intensity;

            target.localPosition = originalPos + new Vector3(offsetX, offsetY, 0f);

            yield return null;
        }

        target.localPosition = originalPos;

        if (hitMarkerImg != null)
            hitMarkerImg.enabled = false;
    }



}
