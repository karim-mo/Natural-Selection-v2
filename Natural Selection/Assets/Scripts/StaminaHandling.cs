using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class StaminaHandling : MonoBehaviourPun
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

    [HideInInspector]
    public float m_currStamina;


    [HideInInspector]
    public int currState;
    [HideInInspector]
    public GameObject staminaBar;

    private PlayerController player;
    void Start()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected) return;
        player = GetComponent<PlayerController>();
        m_currStamina = maxStamina;
        currState = 0;
    }

    void Update()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected) return;

        if(m_currStamina <= 0 && currState != States.MAX_RECHARGE)
        {
            stopRecharge();
            m_currStamina = 0;
            currState = States.MAX_RECHARGE;
            StartCoroutine("Recharge");
        }

        if(!player.isGroundDashing && !player.isJumpDashing && currState != States.MAX_RECHARGE)
        {      
            currState = States.RECHARGE;
            if (m_currStamina >= maxStamina) return;
            StartCoroutine("Recharge");
        }
    }

    public void stopRecharge()
    {
        StopAllCoroutines();
    }
    public bool canUseStamina()
    {
        return m_currStamina > 0 && currState != States.MAX_RECHARGE;
    }

    IEnumerator Recharge()
    {
        float time = currState == States.MAX_RECHARGE ? staminaMaxRechargeSpeed : staminaRechargeSpeed;
        for (float i = (m_currStamina / maxStamina); m_currStamina <= maxStamina; i += 0.01f)
        {
            m_currStamina = i * maxStamina;
            yield return new WaitForSeconds(time / 100);
        }
        m_currStamina = maxStamina;
        currState = States.NO_RECHARGE;
    }
}
