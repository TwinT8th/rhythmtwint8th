using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LongNote : MonoBehaviour
{
    // (1) �� ���� ����� ����ġ
    [SerializeField] private bool DEBUG_LOG = true;

    public int poolType = 1; // 1 = �ճ�Ʈ Ǯ

    [SerializeField] private RectTransform head;
   // [SerializeField] private RectTransform glideIcon; // Head ���ο��� ������ �̹���
    [SerializeField] private RectTransform tail;
    [SerializeField] private Image line;

    [Header("TimingCircle")]
    [SerializeField] private SpriteAnimatorBPM headTimingCircleAnim;
    [SerializeField] private SpriteAnimatorBPM tailTimingCircleAnim;  //  Animator �� SpriteAnimatorBPM
    [SerializeField] private float shrinkBeats = 2f; // ��ҿ� �ɸ��� ��Ʈ �� (����ó��)
                                                     // bpm�� NoteManager�� AudioManager���� �޾ƿ´ٰ� ���� (�����ؾ� ��)
    public float bpm = 90f;

    private double headTargetDSP;     // ��带 ������ �ϴ� ����Ȯ�� ��Ʈ���� DSP
    private double tailTargetDSP; // Tail ������ ���� �ð� �߰�
    private float t = 0f;         // ����� ĳ��
    private float glideDuration; // = (tailTargetDSP - spawnDSP)

    // ���� �ð� ����
    private bool hasPlayedTailAnim = false;

    private bool wasHeld = false; //������ �ѹ��̶� �ճ�Ʈ�� ��Ҵ���

    [HideInInspector] public double expectedHoldDuration; //�� ��Ʈ�� ��ǥ ���ӽð�. NoteManager�� ä����
    private Vector2 headStart, headEnd;
    private double spawnDSP; // ����(DSP ���� ����)
    [SerializeField] private bool autoGlide = false; // �ڵ� �̵� On/Off


    //[HideInInspector] public double expectedHoldDuration;
    /*
    [Header("HitMarker-Head")]
    [SerializeField] private SpriteAnimatorBPM hitMarkerAnim;
    */

    [Header("���� ����Ʈ")]
    [SerializeField] private Animator judgementAnimator;
    [SerializeField] private Image judgementImage; // ���� ��������Ʈ ��ü��
    [SerializeField] private Sprite[] judgementSprites;
    // 0: Perfect, 1: Great, 2: Good, 3: Bad, 4: Miss


    private bool isResolved = false;

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
        isResolved = false;
        //wasHeld = false; ->init�� ����
        if (judgementImage) judgementImage.enabled = false;

        // ��Ŭ�� ����, autoGlide�� �ǵ��� ���� (InitAuto�� ����)
        if (headTimingCircleAnim) { headTimingCircleAnim.bpm = bpm; headTimingCircleAnim.beatsToPlay = 2f; headTimingCircleAnim.Play(); }
        if (tailTimingCircleAnim) tailTimingCircleAnim.Stop();

        // head ��ġ�� SetPositions/InitAuto���� ����
    }


    public void InitAuto(double scheduledStartDPSTime, double targetDSPTime, double expectedDuration)
    {

        wasHeld = false;
        hasPlayedTailAnim = false;

        headTargetDSP = targetDSPTime;
        expectedHoldDuration = Mathf.Max(0f, (float)expectedDuration);

        double secPerBeat = 60.0 / bpm;
        double approachSec = NoteManager.instance.approachBeats * secPerBeat;

        spawnDSP = headTargetDSP - approachSec;
        tailTargetDSP = headTargetDSP + expectedHoldDuration;

        autoGlide = false; // �� ���� ������ ����
        head.anchoredPosition = headStart;

        if (headTimingCircleAnim) { headTimingCircleAnim.bpm = bpm; headTimingCircleAnim.beatsToPlay = 2f; headTimingCircleAnim.Play(); }
        if (tailTimingCircleAnim) tailTimingCircleAnim.Stop();

        isResolved = false;

        Debug.Log($"[LongNote] head={headTargetDSP:F3}, tail={tailTargetDSP:F3}, dur={expectedHoldDuration:F3}, spawn={spawnDSP:F3}");
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
        if (head == null || tail == null || line == null)
        {
            Debug.LogError("[LongNote] Head/Tail/Line �� ������� ���� ������Ʈ�� �ֽ��ϴ�!", this);
            return;
        }

        head.anchoredPosition = startPos;
        tail.anchoredPosition = endPos;

        //�� ���� �մ� ���� ���
        Vector2 dir = endPos - startPos;
        float distance = dir.magnitude;

        //���� �߾� ��ġ
        RectTransform lineRect = line.rectTransform;
        lineRect.anchoredPosition = startPos + dir / 2f;    // ��Ȯ�� �߾�
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        lineRect.localRotation = Quaternion.Euler(0, 0, angle);
        lineRect.sizeDelta = new Vector2(distance, lineRect.sizeDelta.y);

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
            autoGlide = true;

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

        // ���� tail ���� �ణ ���� �� ����
        if (now >= tailTargetDSP + 0.08f)
            FinishLongNote();
    }
    private void UpdateVisuals(float t)
    {
        // head ��� ���� �̵�
        if (head != null)
            head.anchoredPosition = Vector2.Lerp(headStart, headEnd, t);


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
        line.color = isActive ? Color.white : new Color(1, 1, 1, 0.5f);
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
        StartCoroutine(DelayedReturn(0.3f));
    }

    private IEnumerator DelayedReturn(float delay)
    {
        yield return new WaitForSeconds(delay);
        ObjectPool.instance.ReturnNote(poolType, gameObject);

        //TimingManager.instance?.EndHoldJudge(this, 0); // ���� ���� (Ȥ�� �ȳ��� Ȧ�� ����)
    }

    public Vector2 HeadPosition //�ܺ� ���ٿ� ������Ƽ.
    {
        get
        {
            if (head != null)
                return head.anchoredPosition;
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
