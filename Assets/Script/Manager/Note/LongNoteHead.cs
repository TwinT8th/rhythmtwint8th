using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LongNoteHead : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private bool isHolding = false;

    private double holdStartTime;       // 홀드 시작 시각 (DSP 기준)
    private LongNote parentNote;        // 부모 롱노트 참조

    [SerializeField] private float followRadius = 60f; // 1080p 기준 40~80px 추천
    [SerializeField] private float graceMs = 120f;     // 반경 벗어나도 이 시간 동안은 유예
    private double outOfRangeSince = -1;


    void Start()
    {
        parentNote = GetComponentInParent<LongNote>();
        if(parentNote == null )
        {
            Debug.LogError("[LongNoteHead] 부모 LongNote를 찾을 수 없습니다!");
        }
    }

    public void OnPointerDown(PointerEventData e)
    {

        isHolding = true;
        parentNote.StartAutoGlide();
        holdStartTime = AudioSettings.dspTime;
        Debug.Log($"Hold Start at {holdStartTime:F3}s");

        parentNote.NotifyHoldStarted();


        // 타이밍 매니저에 홀드 시작 알림 (추후 점수 연동용)
        if (TimingManager.instance != null)
            TimingManager.instance.StartHoldJudge(parentNote);

        // 시각 효과나 사운드 피드백
        parentNote.ShowHoldEffect(true);
    }



    public void OnDrag(PointerEventData e) 
    {

        if (!isHolding) return;

        //현재 헤드 위치와의 거리 계산
        RectTransform parentRt = parentNote.transform as RectTransform;
        Vector2 pointerLocal;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRt, e.position,e.pressEventCamera,out pointerLocal);

        float dist = Vector2.Distance(pointerLocal, parentNote.HeadPosition); // HeadPosition 프로퍼티 추가
        bool inRange = dist <= followRadius;

        parentNote.ReportFingerInRange(inRange);

        if (inRange)
        {
            outOfRangeSince = -1;
            //(선택) 점수 누적용 틱은 TimingManager에서
        }

        else
        {
            if (outOfRangeSince < 0) outOfRangeSince = AudioSettings.dspTime;
            double ms = (AudioSettings.dspTime - outOfRangeSince) * 1000.0;
            if (ms > graceMs)
            {
                // 유예 초과: 홀드 끊김 처리(감점/콤보 끊김/롱노트 강제 종료 등 설계대로)
                TimingManager.instance?.BreakHold(parentNote);
                isHolding = false;
                parentNote.ShowHoldEffect(false);
                // (설계에 따라) 여기서 바로 FinishLongNote() 하거나, Bad/Miss로 종료
                parentNote.FinishLongNote();
            }
        }

    }
    


    public void OnPointerUp(PointerEventData e)
    {
        if (!isHolding) return;
        isHolding = false;

        double releaseTime = AudioSettings.dspTime; //이거 시간 정확할지 의문. 노트 매니저와 연결짓는 것보다 바로 받아오는게 정확한가?
        Debug.Log($"Hold End at {releaseTime:F3}s");

        // 홀드 지속시간 계산
        double holdDuration = releaseTime - holdStartTime;
        Debug.Log($"Hold Duration: {holdDuration:F2}s");

        // 타이밍 매니저에 홀드 종료 전달 (판정)
        if (TimingManager.instance != null)
            TimingManager.instance.EndHoldJudge(parentNote, holdDuration);

        // 시각 효과 종료
        parentNote.ShowHoldEffect(false);

        // 롱노트 반납
        parentNote.FinishLongNote();
        parentNote.ReportFingerInRange(false);
    }
}
