using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoPlay : StateMachineBehaviour
{
    private StageVideoController stageVideo;
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stageVideo == null)
            stageVideo = Object.FindObjectOfType<StageVideoController>();

            stageVideo.PlayVideo();
    }
}
