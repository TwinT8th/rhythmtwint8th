using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Note : MonoBehaviour
{
    public double targetTimeSec;  // 이 노트가 맞아야 할 절대 시각(DSP 기준)

    // Animator 대신 SpriteAnimatorBPM
    [Header("TimingCircle")]
    [SerializeField] private SpriteAnimatorBPM timingCircleAnim;  //  Animator → SpriteAnimatorBPM

    [Header("HitMarker")]
    [SerializeField] private SpriteAnimatorBPM hitMarkerAnim;     

    [Header("판정 이펙트")]
    [SerializeField] private Animator judgementAnimator;
    [SerializeField] private Image judgementImage; // 판정 스프라이트 교체용
    [SerializeField] private Sprite[] judgementSprites;
    // 0: Perfect, 1: Great, 2: Good, 3: Bad, 4: Miss

    private bool isResolved = false; // true가 되면 이 노트는 더 이상 판정/연출을 하지 않음

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

    void Update()
    {
        if (isResolved) return;

        double now = AudioSettings.dspTime;
        double lastAllowed = targetTimeSec + (double)TimingManager.instance.missRange;

        // Miss 판정
        if (now > lastAllowed)
        {
            ShowJudgementEffect(4); // Miss = index 4

            if (TimingManager.instance != null)
                TimingManager.instance.CharactorAct("Miss");

            // Animator → SpriteAnimatorBPM 방식으로 Stop 교체
            if (timingCircleAnim != null) timingCircleAnim.Stop();
            if (hitMarkerAnim != null) hitMarkerAnim.Stop();

            isResolved = true;
        }
    }

    private void HideNote()
    {
        gameObject.SetActive(false); // @ Destroy → SetActive(false)로 풀링 대응
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
            Debug.Log($"[Note] Init() judgementImage = {judgementImage.name}", this);
            judgementImage.enabled = false;
        }

        isResolved = false;
    }
    public void OnHit()
    {
        if (isResolved) return;

        double now = (double)AudioSettings.dspTime;
        double diff = now - targetTimeSec;

        // 판정 계산
        string result = TimingManager.instance.GetJudgement(diff);

        // 디버그 찍기
        Debug.Log($"[Note] target={targetTimeSec:F3}, now={now:F3}, diff={diff:F3}, result={result}", this);


        // 문자열 → 인덱스 변환
        int index = 4; // 기본 Miss
        switch (result)
        {
            case "Perfect": index = 0; break;
            case "Great": index = 1; break;
            case "Good": index = 2; break;
            case "Bad": index = 3; break;
            case "Miss": index = 4; break;
        }

        ShowJudgementEffect(index);

        // 캐릭터 연출
        TimingManager.instance.CharactorAct(result);

        // @ Animator 대신 SpriteAnimatorBPM → Stop()
        if (timingCircleAnim != null) timingCircleAnim.Stop();
        if (hitMarkerAnim != null) hitMarkerAnim.Stop();

        // 여기 추가: 눌리면 아예 애니메이션 꺼버림
        if (hitMarkerAnim != null) hitMarkerAnim.gameObject.SetActive(false);
        if (timingCircleAnim != null) timingCircleAnim.gameObject.SetActive(false);

        isResolved = true;
    }

    private void ShowJudgementEffect(int index)
    {
        if (judgementSprites != null && index >= 0 && index < judgementSprites.Length)
        {
            judgementImage.sprite = judgementSprites[index];
            judgementImage.enabled = true;
        }

        if (judgementAnimator != null)
            judgementAnimator.SetTrigger("Hit");
    }


}