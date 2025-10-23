using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LongNote : MonoBehaviour
{
    // (1) �� ���� ����� ����ġ
    [SerializeField] private bool DEBUG_LOG = true;

    [Header("Ǯ Ÿ��")]
    public int poolType = 1;

    [Header("Head / Tail")]
    [SerializeField] private RectTransform head;
    [SerializeField] private RectTransform headGlide; // ���� �̵��ϴ� ��Ŀ
    [SerializeField] private RectTransform tail;
    [SerializeField] private RectTransform tailJudge;
    [SerializeField] private Image line;
    [SerializeField] private Image body;

    [Header("TimingCircle")]
    [SerializeField] private SpriteAnimatorBPM headTimingCircleAnim;
    [SerializeField] private SpriteAnimatorBPM tailTimingCircleAnim;  //  Animator �� SpriteAnimatorBPM
    [SerializeField] private float shrinkBeats = 2f; // ��ҿ� �ɸ��� ��Ʈ �� (����ó��)
                                                     // bpm�� NoteManager�� AudioManager���� �޾ƿ´ٰ� ���� (�����ؾ� ��)
    [Header("����")]
    public float bpm = 90f;

    //������ ��Ʈ ��
    private bool isReverse = false;


    // ���� ����
    private bool isResolved = false;
    private bool isInitialized = false;
    private bool hasPlayedTailAnim = false;
    private bool wasHeld = false;  //������ �ѹ��̶� �ճ�Ʈ�� ��Ҵ���
    [SerializeField] private bool autoGlide = false; // �ڵ� �̵� On/Off


    [HideInInspector] public double expectedHoldDuration; //�� ��Ʈ�� ��ǥ ���ӽð�. NoteManager�� ä����
    private Vector2 headStart, headEnd;
    private double spawnDSP; // ����(DSP ���� ����)
    private double headTargetDSP;     // ��带 ������ �ϴ� ����Ȯ�� ��Ʈ���� DSP
    private double tailTargetDSP; // Tail ������ ���� �ð� �߰�
    private float t = 0f;         // ����� ĳ��
    private float glideDuration; // = (tailTargetDSP - spawnDSP)



    [Header("���� ����Ʈ")]
    [SerializeField] private Animator judgementAnimator;
    [SerializeField] private Image judgementImage; // ���� ��������Ʈ ��ü��
    [SerializeField] private Sprite[] judgementSprites;
    // 0: Perfect, 1: Great, 2: Good, 3: Bad, 4: Miss


    void Awake()
    {
        // ���� 1ȸ�� ĳ��
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
        // ���� �̹����� �ִϸ����� ���� �ʱ�ȭ

        if (judgementImage)
        {
            judgementImage.enabled = false;
        }




        if (judgementAnimator)
        {
            judgementAnimator.enabled = false; // �⺻�� ���� (Hit ���� ����)
            judgementAnimator.gameObject.SetActive(false); // �ƿ� ����
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

        autoGlide = false; // ���� ������ ����
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
            Debug.Log("[LongNote] AutoGlide ���۵� (����� �Է�)");
    }

    // === [1] Head / Tail ��ǥ ���� ===
    public void SetPositions(Vector2 startPos, Vector2 endPos)
    {
        if (head == null || headGlide == null || tailJudge == null || tail == null || line == null)
        {
            Debug.LogError("[LongNote] Head/Tail/Line �� ������� ���� ������Ʈ�� �ֽ��ϴ�!", this);
            return;
        }

        head.anchoredPosition = startPos;
        headGlide.anchoredPosition = startPos;
        tail.anchoredPosition = endPos;
        tailJudge.anchoredPosition = endPos;

        //�� ���� �մ� ���� ���
        Vector2 dir = endPos - startPos;
        float distance = dir.magnitude;

        //���� �߾� ��ġ
        RectTransform lineRect = line.rectTransform;
        lineRect.anchoredPosition = startPos + dir / 2f;    // ��Ȯ�� �߾�
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        lineRect.localRotation = Quaternion.Euler(0, 0, angle);
        lineRect.sizeDelta = new Vector2(distance, lineRect.sizeDelta.y);
        
        //  body �ʱ� ���� ===
        if (body != null)
        {
            RectTransform bodyRect = body.rectTransform;
            bodyRect.anchoredPosition = lineRect.anchoredPosition;
            bodyRect.localRotation = lineRect.localRotation;
            bodyRect.sizeDelta = lineRect.sizeDelta;
        }
        
        // ���� ���
        headStart = startPos;
        headEnd = endPos;
    }


    // Update() : ����� t�� 'press ����'���� ���
    void Update()
    {
        double now = AudioSettings.dspTime;

        // �ڵ� ���� Ʈ����: ���� ��Ʈ����
        if (!autoGlide && now >= headTargetDSP)
        {
            autoGlide = true;
            wasHeld = true; // �ڵ� ��忡���� ���� ������ ����
        }

        if (!autoGlide) return;

        // �����: headTarget �� tailTarget
        float t = Mathf.Clamp01((float)((now - headTargetDSP) / expectedHoldDuration));

        UpdateVisuals(t);

        // Tail ��Ŭ: ����ð�����
        double tailShrinkStartDSP = tailTargetDSP - (shrinkBeats * (60.0 / bpm));
        if (!hasPlayedTailAnim && now >= tailShrinkStartDSP)
        {
            hasPlayedTailAnim = true;
            if (tailTimingCircleAnim) { tailTimingCircleAnim.bpm = bpm; tailTimingCircleAnim.beatsToPlay = shrinkBeats; tailTimingCircleAnim.Play(); }
        }



        //@ �߰�. ���� �������� ��ȯ�Ǵ� ����(isReverse = true;)�� �־�� ��. csv���� �о�;ߵ� 
        if (isReverse && now >= tailTargetDSP)
        {
            isReverse = true;
            //���� ����
            (headStart, headEnd) = (headEnd, headStart);
            headTargetDSP = now;
            tailTargetDSP = now + expectedHoldDuration;
        }

        // ���� tail ���� �ణ ���� �� ����
        if (now >= tailTargetDSP + 0.08f)
            FinishLongNote();
    }
    private void UpdateVisuals(float t)
    {
        // head ��� ���� �̵�
        if (headGlide != null)
            headGlide.anchoredPosition = Vector2.Lerp(headStart, headEnd, t);


        // === [Body ���� �پ��� ȿ��] ===
        if (line != null)
        {
            RectTransform lineRect = line.rectTransform;
            Vector2 dir = (headEnd - headStart).normalized;
            float fullLength = (headEnd - headStart).magnitude;

            // ������� ���� ���� ���� ���
            float remainingLength = Mathf.Lerp(fullLength, 0f, t);


            // ���� ����
            lineRect.sizeDelta = new Vector2(remainingLength, lineRect.sizeDelta.y);

            // Tail �������� ���� ������ ���ݸ�ŭ)
            lineRect.anchoredPosition = headEnd - dir * (remainingLength / 2f);
        }


        //tail�� Ÿ�ּ̹�Ŭ ���(bpm ���� 2���� ������) 
        float secPerBeat = 60f / bpm;

        float tailAnimStartTime = (float)(expectedHoldDuration - (shrinkBeats * 60f / bpm));
        float tailAnimStartFrac = Mathf.Clamp01(tailAnimStartTime / (float)expectedHoldDuration);


        // Tail �ִϸ��̼� ����
        if (!hasPlayedTailAnim && t >= tailAnimStartFrac)
        {
            hasPlayedTailAnim = true;
            tailTimingCircleAnim.bpm = bpm;
            tailTimingCircleAnim.beatsToPlay = shrinkBeats;
            tailTimingCircleAnim.Play();
        }

    }


    // === [2] �巡�� ���� �� �ð� ȿ�� ===
    public void ShowHoldEffect(bool isActive)
    {
        // ���� �� ��ȭ, Glow ȿ�� �� (���ϸ� ���⼭ �߰�)
        //line.color = isActive ? Color.white : new Color(1, 1, 1, 0.5f);
    }
    /*
    public void UpdateHoldLine(Vector2 pointerPos)
    {
        // �հ����� �̵��� ��ο� ���� ���� ���� ������Ʈ
        // ����: �巡�� ������� ���� ���� ��/���� ���� ����
    }

    */
    // === [3] Tail �������� ���� ����Ʈ ǥ�� ===
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

    // === [4] �ճ�Ʈ ������ ������ �� ȣ�� ===
    public void FinishLongNote()
    {
        if (isResolved) return;

        isResolved = true;
        t = 0f; // ����� �ʱ�ȭ (Ǯ ���� ���)
        Debug.Log("[LongNote] FinishLongNote() called - returning to pool");

        /*
        // Ÿ�ּ̹�Ŭ ����
        if (tailTimingCircleAnim != null)
            tailTimingCircleAnim.Stop();
        */

        if (!wasHeld)
        {
            TimingManager.instance?.MissRecord();
            TimingManager.instance?.CharactorAct("Miss");
            ShowJudgementEffect(4); // Miss
        }
       
        // �ణ�� ���� �� �ݳ�
        StartCoroutine(DelayedReturn(0.5f));
    }

    private IEnumerator DelayedReturn(float delay)
    {
        yield return new WaitForSeconds(delay);
         NoteManager.instance?.ReturnLongNote(this, tailTargetDSP);

        //TimingManager.instance?.EndHoldJudge(this, 0); // ���� ���� (Ȥ�� �ȳ��� Ȧ�� ����)
    }

    public Vector2 HeadPosition //�ܺ� ���ٿ� ������Ƽ.
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
    // LongNoteHead���� ���� ���� �˷��� �뵵
    public void NotifyHoldStarted()
    {
        wasHeld = true; // ���� ���
    }

}
