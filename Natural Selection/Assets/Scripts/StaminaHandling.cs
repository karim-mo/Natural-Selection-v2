using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaminaHandling : MonoBehaviour
{
    public class States
    {
        public const int NO_RECHARGE = 0;
        public const int RECHARGE = 1;
        public const int MAX_RECHARGE = 2;
    }

    [Header("Stamina settings")]
    public float maxStamina;
    public float staminaRechargeSpeed;
    public float staminaMaxRechargeSpeed;

    private float m_currStamina;


    [HideInInspector]
    public int currState;
    [HideInInspector]
    public GameObject staminaBar;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public bool canUseStamina()
    {
        return m_currStamina > 0 && currState != States.MAX_RECHARGE;
    }

}
