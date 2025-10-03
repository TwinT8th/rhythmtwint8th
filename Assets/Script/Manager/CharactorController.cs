using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharactorController : MonoBehaviour
{


    [Header("�ִϸ�����")]
    [SerializeField] private Animator animator;

    [Header("ĳ���� ����")]
    public bool isDown = false;   // �Ѿ��� �ִ� ����

    void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    //���� ����� ���� �ִϸ��̼� ����
    public void JudgementAct(string result)
    {
        if (isDown) return; // �ٿ� ���¸� ����. ���߿� ü�� ���� ������ ���� �̰� ����
            // Animator�� "Perfect", "Great", "Good", "Bad", "Miss" Ʈ���Ű� �ִٰ� ����
        animator.SetTrigger(result);

    }

    // �ٿ� ���� ����
    public void DownState()
    {
        isDown = true;
        animator.SetBool("IsDown", true);
    }

    // �ٿ� ���¿��� ȸ��
    public void Recover()
    {
        isDown = false;
        animator.SetBool("IsDown", false);
    }
}
