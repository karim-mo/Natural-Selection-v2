using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reloading : StateMachineBehaviour
{
    //public static bool Reloaded = false;

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PlayerController player = animator.GetComponent<PlayerController>();
        player.R_reloaded = true;
    }

}
