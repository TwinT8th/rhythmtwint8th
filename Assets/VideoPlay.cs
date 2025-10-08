using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoPlay : StateMachineBehaviour
{
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (VideoManager.instance != null)
            VideoManager.instance.PlayVideo();
    }
}
