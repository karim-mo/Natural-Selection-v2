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
        PhotonNetwork.SendRate = 20;
        PhotonNetwork.SerializationRate = 20;
        //PhotonNetwork.OfflineMode = true;
        PhotonNetwork.AutomaticallySyncScene = true;
        if (!PhotonNetwork.OfflineMode)
        {
            GameObject player = PhotonNetwork.Instantiate(playerChar.name, spawnPoints[Random.Range(0, 1000 + 1) % spawnPoints.Length].transform.position, Quaternion.identity);
            camBase.GetComponent<CameraController>().CameraFollowObj = player.transform.GetChild(player.transform.childCount - 1).gameObject;
        }
        else
        {
            GameObject player = Instantiate(playerChar, spawnPoints[0].transform.position, Quaternion.identity);
            camBase.GetComponent<CameraController>().CameraFollowObj = player.transform.GetChild(player.transform.childCount - 1).gameObject;
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
