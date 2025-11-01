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
    private bool isResolved = false; // true�� �Ǹ� �� ��Ʈ�� �� �̻� ����/������ ���� ����
    private bool isJudged = false; //�̹� ������ �������°�? 
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

    public double TailTargetDSP => tailTargetDSP;
   // public double TailReachDSP => tailReachDSP;
    public bool IsJudged => isJudged;
    public double HeadTargetDSP => headTargetDSP;

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
         
        }

        if (autoGlide)
        {
            // �����: headTarget �� tailTarget
            float t = Mathf.Clamp01((float)((now - headTargetDSP) / expectedHoldDuration));
            UpdateVisuals(t);
        }

        // --- [Tail Ÿ�ּ̹�Ŭ: bpm ���� 2���� ������ ����] ---
        double tailShrinkStartDSP = tailTargetDSP - (shrinkBeats * (60.0 / bpm));
        if (!hasPlayedTailAnim && now >= tailShrinkStartDSP)
        {
            hasPlayedTailAnim = true;
            if (tailTimingCircleAnim)
            {
                tailTimingCircleAnim.bpm = bpm;
                tailTimingCircleAnim.beatsToPlay = shrinkBeats;
                tailTimingCircleAnim.Play();
            }
        }

        // @ --- [������ ��ȯ ����: ���� CSV ��� ����] ---
        if (isReverse && now >= tailTargetDSP)
        {
            isReverse = true;
            (headStart, headEnd) = (headEnd, headStart);
            headTargetDSP = now;
            tailTargetDSP = now + expectedHoldDuration;
        }

        if (!isJudged && AudioSettings.dspTime >= tailTargetDSP + 0.3f)
        {
            // ���� ���� ���� ������ Miss
            TimingManager.instance?.ForceTimeoutMiss(this);
            // ���⼭ FinishLongNote() ���� �θ��� �� ��!
            // Miss�� OnHoldJudgeEnd(4) �� FinishLongNote() �帧���� �Ϸ��.
            return;
        }

  
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

        if (index <= 2)
        {
            StartCoroutine(HeadGlidePulse(0.15f)); // 0.15�� ���� Ŀ���ٰ� ����
        }
        else
        {
            StartCoroutine(ShakeEffect(0.2f, 8f, 3f));
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

        if (!isJudged)
        {
            if (isBrokenHold || !wasHeld)
            {
                TimingManager.instance?.MissRecord();
                TimingManager.instance?.CharactorAct("Miss");
                ShowJudgementEffect(4); // Miss
                Debug.Log($"[LongNote] Miss shown at {AudioSettings.dspTime:F3}s ");
            }

        }
        // �ణ�� ���� �� �ݳ�
        StartCoroutine(DelayedReturn(0.8f));
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

    private bool isBrokenHold = false;  // �߰�

    public void NotifyHoldBroken()
    {
        isBrokenHold = true;  // �巡�� �� ���� ���Ҵٰ� ǥ��
    }

    public void NotifyHoldEnded()
    {
        isJudged = true; //������ �̹� ���������� ǥ��(�巡�� �� �� ������ �� ���� �ߺ� �ȵǰ�)
    }

    //��� �ð�/�ݳ� ������ �߽�
    public void OnHoldJudgeEnd(int result)
    {
        if (isResolved) return;
        isJudged = true;

        // headGlide�� tail ������ ������ ��ٷȴٰ� ���� ����
        StartCoroutine(WaitForTailThenJudge(result));
    }

    private IEnumerator WaitForTailThenJudge(int result)
    {
        // ���� DSP �ð��� tail ���� �ð� ���� ���
        double now = AudioSettings.dspTime;
        float remain = Mathf.Max(0f, (float)(tailTargetDSP - now));

        // ���� ���̸� ���� �ð���ŭ ���
        if (remain > 0f)
            yield return new WaitForSeconds(remain);

        // headGlide�� tail���� �̵��� �ڿ� ���� ����
        ShowJudgementEffect(result);

           //���� ������ ���� �ݳ�
            StartCoroutine(FinishAtTail());
    }
    private IEnumerator FinishAtTail()
    {
        double now = AudioSettings.dspTime;
        float remain = Mathf.Max(0f, (float)(tailTargetDSP - now));

        // tailTargetDSP�� �̹� �����ٸ� 0.2�ʸ� ��ٷȴ� �ݳ�
        if (remain < 0.2f)
            remain = 0.2f;

        yield return new WaitForSeconds(remain);

        // tail ���� �� ���� �ݳ�
        FinishLongNote();
    }
    private IEnumerator HeadGlidePulse(float duration = 0.2f)
    {
        if (headGlide == null) yield break;

        float timer = 0f;
        Vector3 startScale = headGlide.localScale;
        Vector3 targetScale = startScale * 1.25f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            float eased = t * t * (3 - 2 * t); // ease-in-out ����

            // ó�� ������ Ŀ����, �Ĺ� ������ ���� ũ��� ����
            float scale = (t < 0.5f)
                ? Mathf.Lerp(1f, 1.25f, eased * 2f)
                : Mathf.Lerp(1.25f, 1f, (eased - 0.5f) * 2f);

            headGlide.localScale = startScale * scale;

            yield return null;
        }

        headGlide.localScale = startScale;
    }

    private IEnumerator ShakeEffect(float duration, float shakeMagnitude, float shakeFrequency)
    {
        if (headGlide == null) yield break;

        Transform target = headGlide.transform;
        Vector3 originalPos = target.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            // ���� (ó���� ����, ���� �پ��)
            float intensity = (1f - progress);

            // �����Ӹ��� ���� ����
            float offsetX = Mathf.Sin(Time.time * shakeFrequency * 20f) * shakeMagnitude * intensity;
            float offsetY = Mathf.Cos(Time.time * shakeFrequency * 15f) * shakeMagnitude * intensity;

            target.localPosition = originalPos + new Vector3(offsetX, offsetY, 0f);

            yield return null;
        }

        target.localPosition = originalPos;
    }


}
