using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoPause : StateMachineBehaviour
{

    private StageVideoController stageVideo;
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stageVideo == null)
            stageVideo = Object.FindObjectOfType<StageVideoController>();

        if (stageVideo != null && stageVideo.isActiveAndEnabled)
        {
            stageVideo.PauseVideo();
            //Debug.Log("[VideoPause] StageVideoController 영상 일시정지 실행됨");
        }
        else
        {
            Debug.LogWarning("[VideoPause] StageVideoController를 찾을 수 없습니다.");
        }
    }
}
