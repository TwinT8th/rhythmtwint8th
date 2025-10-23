using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LongNote : MonoBehaviour
{
    // (1) 맨 위에 디버그 스위치
    [SerializeField] private bool DEBUG_LOG = true;

    [Header("풀 타입")]
    public int poolType = 1;

    [Header("Head / Tail")]
    [SerializeField] private RectTransform head;
    [SerializeField] private RectTransform headGlide; // 실제 이동하는 마커
    [SerializeField] private RectTransform tail;
    [SerializeField] private RectTransform tailJudge;
    [SerializeField] private Image line;
    [SerializeField] private Image body;

    [Header("TimingCircle")]
    [SerializeField] private SpriteAnimatorBPM headTimingCircleAnim;
    [SerializeField] private SpriteAnimatorBPM tailTimingCircleAnim;  //  Animator → SpriteAnimatorBPM
    [SerializeField] private float shrinkBeats = 2f; // 축소에 걸리는 비트 수 (기존처럼)
                                                     // bpm을 NoteManager나 AudioManager에서 받아온다고 가정 (구현해야 함)
    [Header("설정")]
    public float bpm = 90f;

    //리버스 노트 용
    private bool isReverse = false;


    // 내부 상태
    private bool isResolved = false;
    private bool isInitialized = false;
    private bool hasPlayedTailAnim = false;
    private bool wasHeld = false;  //유저가 한번이라도 롱노트르 잡았는지
    [SerializeField] private bool autoGlide = false; // 자동 이동 On/Off


    [HideInInspector] public double expectedHoldDuration; //롱 노트의 목표 지속시간. NoteManager가 채워줌
    private Vector2 headStart, headEnd;
    private double spawnDSP; // 스폰(DSP 기준 시작)
    private double headTargetDSP;     // 헤드를 눌러야 하는 ‘정확한 비트’의 DSP
    private double tailTargetDSP; // Tail 판정용 절대 시각 추가
    private float t = 0f;         // 진행률 캐싱
    private float glideDuration; // = (tailTargetDSP - spawnDSP)



    [Header("판정 이펙트")]
    [SerializeField] private Animator judgementAnimator;
    [SerializeField] private Image judgementImage; // 판정 스프라이트 교체용
    [SerializeField] private Sprite[] judgementSprites;
    // 0: Perfect, 1: Great, 2: Good, 3: Bad, 4: Miss


    void Awake()
    {
        // 최초 1회만 캐싱
        if (shrinkBeats < 0f && NoteManager.instance != null)
        {
            shrinkBeats = NoteManager.instance.approachBeats;
            if (DEBUG_LOG)
                Debug.Log($"[LongNote] Cached approachBeats = {shrinkBeats}");
        }
    }
    void OnEnable()
    {
        ResetState();
    }
       public void ResetState()
    {
        isResolved = false;
        isInitialized = false;
        hasPlayedTailAnim = false;
        wasHeld = false;
        autoGlide = false;
        isReverse = false;
        // 판정 이미지와 애니메이터 완전 초기화

        if (judgementImage)
        {
            judgementImage.enabled = false;
        }




        if (judgementAnimator)
        {
            judgementAnimator.enabled = false; // 기본은 꺼둠 (Hit 때만 켜짐)
            judgementAnimator.gameObject.SetActive(false); // 아예 숨김
        }


        if (headTimingCircleAnim)
        {
            headTimingCircleAnim.Stop();
            headTimingCircleAnim.gameObject.SetActive(true);
        }

        if (tailTimingCircleAnim)
        {
            tailTimingCircleAnim.Stop();
            tailTimingCircleAnim.gameObject.SetActive(true);
        }

        if (judgementAnimator)
        {
            judgementAnimator.enabled = true;
            judgementAnimator.gameObject.SetActive(true);
            judgementAnimator.ResetTrigger("Hit");
        }

        if (line)
        {
            //line.color = new Color(1, 1, 1, 1f);
            var lineRect = line.rectTransform;
            lineRect.sizeDelta = new Vector2(lineRect.sizeDelta.x, lineRect.sizeDelta.y);
        }

        if (body)
        {
            body.gameObject.SetActive(true);
        }

    }


    public void InitAuto(double scheduledStartDPSTime, double targetDSPTime, double expectedDuration)
    {

        ResetState();

        double secPerBeat = 60.0 / bpm;
        double approachSec = NoteManager.instance.approachBeats * secPerBeat;

        headTargetDSP = targetDSPTime;
        expectedHoldDuration = Mathf.Max(0f, (float)expectedDuration);
        spawnDSP = headTargetDSP - approachSec;
        tailTargetDSP = headTargetDSP + expectedHoldDuration;

        autoGlide = false; // 스폰 시점엔 정지
        head.anchoredPosition = headStart;
        headGlide.anchoredPosition = headStart;

        if (headTimingCircleAnim) { headTimingCircleAnim.bpm = bpm; headTimingCircleAnim.beatsToPlay = 2f; headTimingCircleAnim.Play(); }
        if (tailTimingCircleAnim) tailTimingCircleAnim.Stop();

        isResolved = false;

        //Debug.Log($"[LongNote] head={headTargetDSP:F3}, tail={tailTargetDSP:F3}, dur={expectedHoldDuration:F3}, spawn={spawnDSP:F3}");
    }

    public void StartAutoGlide()
    {
        autoGlide = true;

        if (DEBUG_LOG)
            Debug.Log("[LongNote] AutoGlide 시작됨 (사용자 입력)");
    }

    // === [1] Head / Tail 좌표 세팅 ===
    public void SetPositions(Vector2 startPos, Vector2 endPos)
    {
        if (head == null || headGlide == null || tailJudge == null || tail == null || line == null)
        {
            Debug.LogError("[LongNote] Head/Tail/Line 중 연결되지 않은 오브젝트가 있습니다!", this);
            return;
        }

        head.anchoredPosition = startPos;
        headGlide.anchoredPosition = startPos;
        tail.anchoredPosition = endPos;
        tailJudge.anchoredPosition = endPos;

        //두 점을 잇는 벡터 계산
        Vector2 dir = endPos - startPos;
        float distance = dir.magnitude;

        //라인 중앙 배치
        RectTransform lineRect = line.rectTransform;
        lineRect.anchoredPosition = startPos + dir / 2f;    // 정확히 중앙
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        lineRect.localRotation = Quaternion.Euler(0, 0, angle);
        lineRect.sizeDelta = new Vector2(distance, lineRect.sizeDelta.y);
        
        //  body 초기 세팅 ===
        if (body != null)
        {
            RectTransform bodyRect = body.rectTransform;
            bodyRect.anchoredPosition = lineRect.anchoredPosition;
            bodyRect.localRotation = lineRect.localRotation;
            bodyRect.sizeDelta = lineRect.sizeDelta;
        }
        
        // 내부 기록
        headStart = startPos;
        headEnd = endPos;
    }


    // Update() : 진행률 t를 'press 기준'으로 계산
    void Update()
    {
        double now = AudioSettings.dspTime;

        // 자동 시작 트리거: 판정 비트부터
        if (!autoGlide && now >= headTargetDSP)
        {
            autoGlide = true;
            wasHeld = true; // 자동 모드에서도 눌린 것으로 간주
        }

        if (!autoGlide) return;

        // 진행률: headTarget → tailTarget
        float t = Mathf.Clamp01((float)((now - headTargetDSP) / expectedHoldDuration));

        UpdateVisuals(t);

        // Tail 서클: 절대시간으로
        double tailShrinkStartDSP = tailTargetDSP - (shrinkBeats * (60.0 / bpm));
        if (!hasPlayedTailAnim && now >= tailShrinkStartDSP)
        {
            hasPlayedTailAnim = true;
            if (tailTimingCircleAnim) { tailTimingCircleAnim.bpm = bpm; tailTimingCircleAnim.beatsToPlay = shrinkBeats; tailTimingCircleAnim.Play(); }
        }



        //@ 추가. 여기 리버스로 전환되는 조건(isReverse = true;)이 있어야 함. csv에서 읽어와야됨 
        if (isReverse && now >= tailTargetDSP)
        {
            isReverse = true;
            //방향 반전
            (headStart, headEnd) = (headEnd, headStart);
            headTargetDSP = now;
            tailTargetDSP = now + expectedHoldDuration;
        }

        // 절대 tail 시점 약간 여유 후 종료
        if (now >= tailTargetDSP + 0.08f)
            FinishLongNote();
    }
    private void UpdateVisuals(float t)
    {
        // head 경로 따라 이동
        if (headGlide != null)
            headGlide.anchoredPosition = Vector2.Lerp(headStart, headEnd, t);


        // === [Body 길이 줄어드는 효과] ===
        if (line != null)
        {
            RectTransform lineRect = line.rectTransform;
            Vector2 dir = (headEnd - headStart).normalized;
            float fullLength = (headEnd - headStart).magnitude;

            // 진행률에 따라 남은 길이 계산
            float remainingLength = Mathf.Lerp(fullLength, 0f, t);


            // 길이 갱신
            lineRect.sizeDelta = new Vector2(remainingLength, lineRect.sizeDelta.y);

            // Tail 방향으로 남은 길이의 절반만큼)
            lineRect.anchoredPosition = headEnd - dir * (remainingLength / 2f);
        }


        //tail쪽 타이밍서클 축소(bpm 기준 2박자 전부터) 
        float secPerBeat = 60f / bpm;

        float tailAnimStartTime = (float)(expectedHoldDuration - (shrinkBeats * 60f / bpm));
        float tailAnimStartFrac = Mathf.Clamp01(tailAnimStartTime / (float)expectedHoldDuration);


        // Tail 애니메이션 시작
        if (!hasPlayedTailAnim && t >= tailAnimStartFrac)
        {
            hasPlayedTailAnim = true;
            tailTimingCircleAnim.bpm = bpm;
            tailTimingCircleAnim.beatsToPlay = shrinkBeats;
            tailTimingCircleAnim.Play();
        }

    }


    // === [2] 드래그 유지 시 시각 효과 ===
    public void ShowHoldEffect(bool isActive)
    {
        // 라인 색 변화, Glow 효과 등 (원하면 여기서 추가)
        //line.color = isActive ? Color.white : new Color(1, 1, 1, 0.5f);
    }
    /*
    public void UpdateHoldLine(Vector2 pointerPos)
    {
        // 손가락이 이동한 경로에 따라 라인 비율 업데이트
        // 예시: 드래그 진행률에 따라 라인 색/길이 변경 가능
    }

    */
    // === [3] Tail 기준으로 판정 이펙트 표시 ===
    public void ShowJudgementEffect(int index)
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
            Debug.LogError("[LongNote] judgementAnimator is NULL!", this);
        }
    }

    // === [4] 롱노트 완전히 끝났을 때 호출 ===
    public void FinishLongNote()
    {
        if (isResolved) return;

        isResolved = true;
        t = 0f; // 진행률 초기화 (풀 재사용 대비)
        Debug.Log("[LongNote] FinishLongNote() called - returning to pool");

        /*
        // 타이밍서클 정지
        if (tailTimingCircleAnim != null)
            tailTimingCircleAnim.Stop();
        */

        if (!wasHeld)
        {
            TimingManager.instance?.MissRecord();
            TimingManager.instance?.CharactorAct("Miss");
            ShowJudgementEffect(4); // Miss
        }
       
        // 약간의 지연 후 반납
        StartCoroutine(DelayedReturn(0.5f));
    }

    private IEnumerator DelayedReturn(float delay)
    {
        yield return new WaitForSeconds(delay);
         NoteManager.instance?.ReturnLongNote(this, tailTargetDSP);

        //TimingManager.instance?.EndHoldJudge(this, 0); // 강제 정리 (혹시 안끝난 홀드 제거)
    }

    public Vector2 HeadPosition //외부 접근용 프로퍼티.
    {
        get
        {
            if (headGlide != null)
                return headGlide.anchoredPosition;
            else
                return Vector2.zero;
        }
    }

    public bool IsFingerInRange { get; private set; } = false;
    public void ReportFingerInRange(bool inRange)
    {
        IsFingerInRange = inRange;
    }

    private float tailAnimOffsetSec = 0f;
    public void SetTailAnimOffset(double offset)
    {
        tailAnimOffsetSec = (float)offset;
    }
    // LongNoteHead에서 눌린 순간 알려줄 용도
    public void NotifyHoldStarted()
    {
        wasHeld = true; // 잡음 기록
    }

}
