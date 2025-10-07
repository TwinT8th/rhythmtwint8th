using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharactorController : MonoBehaviour
{


    [Header("애니메이터")]
    [SerializeField] private Animator charAnimator;

    private bool isFalling = false;  // Fall 루틴 중엔 입력 무시

    void Awake()
    {
        if (charAnimator == null)
            charAnimator = GetComponent<Animator>();
    }

    public void JudgementAct(string result)
    {
        if (isFalling) return; //넘어지는 중엔 어떤 판정도 무시

        switch (result)
        {
            case "Perfect":
            case "Great":
                break; //걷기 유지

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

        // Fall(2박자) + StandUp(4박자) = 6박자 길이
        float beatTime = 60f / NoteManager.instance.bpm;
        yield return new WaitForSeconds(6f * beatTime);

        charAnimator.ResetTrigger("Walk");
        charAnimator.SetTrigger("Walk");

        isFalling = false;
    }
}


