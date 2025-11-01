using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LongNoteHead : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private bool isHolding = false;

    private double holdStartTime;       // Ȧ�� ���� �ð� (DSP ����)
    private LongNote parentNote;        // �θ� �ճ�Ʈ ����

    [SerializeField] private float followRadius = 60f; // 1080p ���� 40~80px ��õ
    [SerializeField] private float graceMs = 120f;     // �ݰ� ����� �� �ð� ������ ����
    private double outOfRangeSince = -1;


    void Start()
    {
        parentNote = GetComponentInParent<LongNote>();
        if(parentNote == null )
        {
            Debug.LogError("[LongNoteHead] �θ� LongNote�� ã�� �� �����ϴ�!");
        }
    }

    private void Update()
    {
        if (!isHolding || parentNote == null) return;

        double now = AudioSettings.dspTime;
        double tailTime = parentNote.TailTargetDSP;

        // tail ���� + 0.3�� ������ ���� �ȵ����� ���� Miss
        if (now >= tailTime + 0.3 && !parentNote.IsJudged)
        {
            Debug.Log("[LongNoteHead] Tail timeout �� forced Miss");

            // Miss ������ �ѱ��, isJudged�� TimingManager�� ó���ϰ� �д�
            TimingManager.instance?.EndHoldJudge(parentNote, now - parentNote.HeadTargetDSP);

            // parentNote.NotifyHoldEnded();  �� ����!!
            parentNote.ShowHoldEffect(false);
            parentNote.ReportFingerInRange(false);

            // FinishLongNote()�� ȣ������ �ʴ´� (LongNote���� ó��)
            isHolding = false;
        }
    }

    public void OnPointerDown(PointerEventData e)
    {

        isHolding = true;
        parentNote.StartAutoGlide();
        holdStartTime = AudioSettings.dspTime;
        Debug.Log($"Hold Start at {holdStartTime:F3}s");

        parentNote.NotifyHoldStarted();


        // Ÿ�̹� �Ŵ����� Ȧ�� ���� �˸� (���� ���� ������)
        if (TimingManager.instance != null)
            TimingManager.instance.StartHoldJudge(parentNote);

        // �ð� ȿ���� ���� �ǵ��
        parentNote.ShowHoldEffect(true);
    }



    public void OnDrag(PointerEventData e) 
    {

        if (!isHolding) return;

        //���� ��� ��ġ���� �Ÿ� ���
        RectTransform parentRt = parentNote.transform as RectTransform;
        Vector2 pointerLocal;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRt, e.position,e.pressEventCamera,out pointerLocal);

        float dist = Vector2.Distance(pointerLocal, parentNote.HeadPosition); // HeadPosition ������Ƽ �߰�
        bool inRange = dist <= followRadius;

        parentNote.ReportFingerInRange(inRange);



        if (inRange)
        {
            outOfRangeSince = -1;
            //(����) ���� ������ ƽ�� TimingManager����
        }

        else
        {
            if (outOfRangeSince < 0) outOfRangeSince = AudioSettings.dspTime;
            double ms = (AudioSettings.dspTime - outOfRangeSince) * 1000.0;
            if (ms > graceMs)
            {
                // ���� �ʰ�: Ȧ�� ���� ó��(����/�޺� ����/�ճ�Ʈ ���� ���� �� ������)
                TimingManager.instance?.BreakHold(parentNote);
                isHolding = false;
                parentNote.ShowHoldEffect(false);
                parentNote.NotifyHoldBroken();  //  �߰��� �޼��� ȣ��
            }
        }

    }
    


    public void OnPointerUp(PointerEventData e)
    {
        if (!isHolding) return;
        isHolding = false;

        double releaseTime = AudioSettings.dspTime; //�̰� �ð� ��Ȯ���� �ǹ�. ��Ʈ �Ŵ����� �������� �ͺ��� �ٷ� �޾ƿ��°� ��Ȯ�Ѱ�?
        Debug.Log($"Hold End at {releaseTime:F3}s");

        // Ȧ�� ���ӽð� ���
        double holdDuration = releaseTime - holdStartTime;
        Debug.Log($"Hold Duration: {holdDuration:F2}s");

        // Ÿ�̹� �Ŵ����� Ȧ�� ���� ���� (����)
        if (TimingManager.instance != null)
            TimingManager.instance.EndHoldJudge(parentNote, holdDuration);

        // ������ �̹� ���������� ǥ��
        parentNote.NotifyHoldEnded();


        // �ð� ȿ�� ����
        parentNote.ShowHoldEffect(false);
        parentNote.ReportFingerInRange(false);

    }
}
