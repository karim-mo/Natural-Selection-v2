using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class HolsterBehaviour : StateMachineBehaviour
{

    public static bool isRifleUp = true;

    
    
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
        isRifleUp = false;
        Debug.Log("Holstered Rifle!");
    }

}
