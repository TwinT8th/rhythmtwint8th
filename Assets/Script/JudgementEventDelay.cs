using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JudgementEventDelay : MonoBehaviour
{
    private Note parentNote;

    void Awake()
    {
        parentNote = GetComponentInParent<Note>();
        Debug.Log($"[JED] Awake receiver on {name}, parent note = {parentNote}", this);
    }

    // 애니메이션 이벤트가 이 함수를 호출
    public void NotifyNoteFinished()
    {
        Debug.Log("[JED] NotifyNoteFinished called", this);
        if (parentNote != null)
        {
            parentNote.NotifyNoteFinished();
        }

        else
        {
            Debug.LogError("[Proxy] Note 부모를 찾을 수 없습니다.", this);
        }
    }
}
