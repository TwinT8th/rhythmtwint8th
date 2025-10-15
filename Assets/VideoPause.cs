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
            //Debug.Log("[VideoPause] StageVideoController ���� �Ͻ����� �����");
        }
        else
        {
            Debug.LogWarning("[VideoPause] StageVideoController�� ã�� �� �����ϴ�.");
        }
    }
}
