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

    // �ִϸ��̼� �̺�Ʈ�� �� �Լ��� ȣ��
    public void NotifyNoteFinished()
    {
        //Debug.Log("[JED] NotifyNoteFinished called", this);
        if (parentNote != null)
        {
            parentNote.NotifyNoteFinished();
        }
        else if (parentLongNote != null)
        {
            parentLongNote.NotifyNoteFinished(); // LongNote�� �� �Լ� �߰� (�Ʒ� ����)
        }
        else
        {
            // �� �̻� ���� �α׷� ����� ���� ���θ� ���
            Debug.LogWarning("[JudgementEventDelay] �θ� Note/LongNote�� ã�� �� �����ϴ�.", this);
        }
    }


}
