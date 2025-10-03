using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharactorController : MonoBehaviour
{


    [Header("애니메이터")]
    [SerializeField] private Animator animator;

    [Header("캐릭터 상태")]
    public bool isDown = false;   // 넘어져 있는 상태

    void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    //판정 결과에 따른 애니메이션 실행
    public void JudgementAct(string result)
    {
        if (isDown) return; // 다운 상태면 무시. 나중에 체력 누적 게이지 만들어서 이거 쓰기
            // Animator에 "Perfect", "Great", "Good", "Bad", "Miss" 트리거가 있다고 가정
        animator.SetTrigger(result);

    }

    // 다운 상태 진입
    public void DownState()
    {
        isDown = true;
        animator.SetBool("IsDown", true);
    }

    // 다운 상태에서 회복
    public void Recover()
    {
        isDown = false;
        animator.SetBool("IsDown", false);
    }
}
