using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharactorController : MonoBehaviour
{


    [Header("�ִϸ�����")]
    [SerializeField] private Animator charAnimator;

    private bool isFalling = false;  // Fall ��ƾ �߿� �Է� ����

    void Awake()
    {
        if (charAnimator == null)
            charAnimator = GetComponent<Animator>();
    }

    public void JudgementAct(string result)
    {
        if (isFalling) return; //�Ѿ����� �߿� � ������ ����

        switch (result)
        {
            case "Perfect":
            case "Great":
                break; //�ȱ� ����

            case "Good":
            case "Bad":
                charAnimator.ResetTrigger("Wobble");
                charAnimator.SetTrigger("Wobble");
                break;

            case "Miss":
                StartCoroutine(FallRoutine());
                break;
        }

    }
    private System.Collections.IEnumerator FallRoutine()
    {
        isFalling = true;

        charAnimator.ResetTrigger("Fall");
        charAnimator.SetTrigger("Fall");

        // Fall(2����) + StandUp(4����) = 6���� ����
        float beatTime = 60f / NoteManager.instance.bpm;
        yield return new WaitForSeconds(6f * beatTime);

        charAnimator.ResetTrigger("Walk");
        charAnimator.SetTrigger("Walk");

        isFalling = false;
    }
}


