using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTimer : MonoBehaviour
{
    public static GameTimer instance;
    public float gameTime;

    private void Awake()
    {
        if (instance != null) Destroy(instance);
        else
            instance = this;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        
    }
}
