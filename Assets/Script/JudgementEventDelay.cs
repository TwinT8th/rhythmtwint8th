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

    // �ִϸ��̼� �̺�Ʈ�� �� �Լ��� ȣ��
    public void NotifyNoteFinished()
    {
        Debug.Log("[JED] NotifyNoteFinished called", this);
        if (parentNote != null)
        {
            parentNote.NotifyNoteFinished();
        }

        else
        {
            Debug.LogError("[Proxy] Note �θ� ã�� �� �����ϴ�.", this);
        }
    }
}
