using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoPauseAndPlay : StateMachineBehaviour
{

    private StageVideoController stageVideo;
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stageVideo == null)
            stageVideo = Object.FindObjectOfType<StageVideoController>();

        stageVideo.PauseVideo();
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stageVideo == null)
            stageVideo = Object.FindObjectOfType<StageVideoController>();

        stageVideo.PlayVideo();
    }
}
