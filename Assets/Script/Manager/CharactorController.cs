using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class CharactorController : MonoBehaviour
{


    [Header("�ִϸ�����")]
    [SerializeField] private Animator charAnimator;
    [SerializeField] private float baseBPM = 180f; //�ִϸ��̼��� �����δ� 180BPM � �´� �ӵ��� ����ǰ� �ִ� ��

    private bool isFalling = false;  // Fall ��ƾ �߿� �Է� ����

    void Awake()
    {
        if (charAnimator == null)
            charAnimator = GetComponent<Animator>();
    }

    void Start()
    {
        UpdateAnimatorSpeed(NoteManager.instance.bpm);
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
                if (VideoManager.instance != null) //�����˻�
                    VideoManager.instance.PauseVideo();
                charAnimator.ResetTrigger("Wobble");
                charAnimator.SetTrigger("Wobble");
                break;

            case "Miss":
                if (isFalling) return; // �̹� �Ѿ����� ���̸� ����
                StartCoroutine(FallRoutine());
                break;
        }

    }
    private System.Collections.IEnumerator FallRoutine()
    {
        isFalling = true;

        if (VideoManager.instance != null) //�����˻�
            VideoManager.instance.PauseVideo();

        charAnimator.ResetTrigger("Fall");
        charAnimator.SetTrigger("Fall");

        // Fall(2����) + StandUp(4����) = 6���� ����
        float beatTime = (NoteManager.instance != null) //���� �����ڴ� ���� ��ȯ�� ����, �� ���ԽĿ����� �� �� ����. (���ǽ�) ? (������ ���� �� ��) : (������ ������ �� ��)
            ? 60f / NoteManager.instance.bpm
            : 60f / baseBPM;

        yield return new WaitForSeconds(6f * beatTime);
        
        //VideoManager.instance.PlayVideo(); - statemachine���� �ذ�
        charAnimator.ResetTrigger("Walk");
        charAnimator.SetTrigger("Walk");

        isFalling = false;

    }

    public void UpdateAnimatorSpeed(float currentBPM)
    {

        // �ܼ� ��� ����
        float speed = currentBPM / baseBPM;
        charAnimator.speed = speed;

        Debug.Log($"[CharactorController] Animator speed set to {speed:F2} (BPM={currentBPM})");
    }
}


