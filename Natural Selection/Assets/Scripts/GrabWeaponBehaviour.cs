﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabWeaponBehaviour : StateMachineBehaviour
{
    public static bool isRifleUp = false;

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        isRifleUp = true;
        Debug.Log("Grabbed Rifle!");
    }
}
