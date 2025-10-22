using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JudgementEventDelay : MonoBehaviour
{
    private Note parentNote;
    private LongNote parentLongNote;

    void Awake()
    {
        parentNote = GetComponentInParent<Note>();
        parentLongNote = GetComponentInParent<LongNote>();
        //Debug.Log($"[JED] Awake receiver on {name}, parent note = {parentNote}", this);
    }

    // 애니메이션 이벤트가 이 함수를 호출
    public void NotifyNoteFinished()
    {
        //Debug.Log("[JED] NotifyNoteFinished called", this);
        if (parentNote != null)
        {
            parentNote.NotifyNoteFinished();
        }
        else if (parentLongNote != null)
        {
            parentLongNote.NotifyNoteFinished(); // LongNote용 새 함수 추가 (아래 참고)
        }
        else
        {
            // 더 이상 에러 로그로 띄우지 말고 경고로만 출력
            Debug.LogWarning("[JudgementEventDelay] 부모 Note/LongNote를 찾을 수 없습니다.", this);
        }
    }


}
