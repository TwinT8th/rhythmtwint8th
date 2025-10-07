using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class CharactorController : MonoBehaviour
{


    [Header("애니메이터")]
    [SerializeField] private Animator charAnimator;
    [SerializeField] private float baseBPM = 180f; //애니메이션이 실제로는 180BPM 곡에 맞는 속도로 재생되고 있는 셈

    private bool isFalling = false;  // Fall 루틴 중엔 입력 무시

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
        if (isFalling) return; //넘어지는 중엔 어떤 판정도 무시

        switch (result)
        {
            case "Perfect":
            case "Great":
                break; //걷기 유지

            case "Good":
            case "Bad":
                if (VideoManager.instance != null) //안전검사
                    VideoManager.instance.PauseVideo();
                charAnimator.ResetTrigger("Wobble");
                charAnimator.SetTrigger("Wobble");
                break;

            case "Miss":
                if (isFalling) return; // 이미 넘어지는 중이면 무시
                StartCoroutine(FallRoutine());
                break;
        }

    }
    private System.Collections.IEnumerator FallRoutine()
    {
        isFalling = true;

        if (VideoManager.instance != null) //안전검사
            VideoManager.instance.PauseVideo();

        charAnimator.ResetTrigger("Fall");
        charAnimator.SetTrigger("Fall");

        // Fall(2박자) + StandUp(4박자) = 6박자 길이
        float beatTime = (NoteManager.instance != null) //삼항 연산자는 값을 반환할 때만, 즉 대입식에서만 쓸 수 있음. (조건식) ? (조건이 참일 때 값) : (조건이 거짓일 때 값)
            ? 60f / NoteManager.instance.bpm
            : 60f / baseBPM;

        yield return new WaitForSeconds(6f * beatTime);
        
        //VideoManager.instance.PlayVideo(); - statemachine으로 해결
        charAnimator.ResetTrigger("Walk");
        charAnimator.SetTrigger("Walk");

        isFalling = false;

    }

    public void UpdateAnimatorSpeed(float currentBPM)
    {

        // 단순 비례 계산식
        float speed = currentBPM / baseBPM;
        charAnimator.speed = speed;

        Debug.Log($"[CharactorController] Animator speed set to {speed:F2} (BPM={currentBPM})");
    }
}


