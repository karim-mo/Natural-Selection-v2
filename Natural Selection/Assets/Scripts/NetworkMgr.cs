using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkMgr : MonoBehaviour
{

    public GameObject camBase;
    public GameObject playerChar;

    public GameObject[] spawnPoints;

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        GameObject player = PhotonNetwork.Instantiate(playerChar.name, spawnPoints[0].transform.position, Quaternion.identity);
        camBase.GetComponent<CameraController>().CameraFollowObj = player.transform.GetChild(player.transform.childCount - 1).gameObject;
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
