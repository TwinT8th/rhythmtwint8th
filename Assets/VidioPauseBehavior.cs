using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VidioPauseBehavior : StateMachineBehaviour
{
    public bool pauseOnEnter = true;
    public bool playOnExit = true;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (pauseOnEnter && VideoManager.instance != null)
            VideoManager.instance.PauseVideo();
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (playOnExit && VideoManager.instance != null)
            VideoManager.instance.PlayVideo();
    }
}
