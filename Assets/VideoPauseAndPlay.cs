using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoPauseAndPlay : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (VideoManager.instance != null)
            VideoManager.instance.PauseVideo();
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (VideoManager.instance != null)
            VideoManager.instance.PlayVideo();
    }
}
