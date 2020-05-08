using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using TMPro;

public class PlayerEntries : MonoBehaviour
{
    public Text PlayerNameText;
    public Button Rdy;

    private int ID;
    private bool ready;

    private void Start()
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber != ID)
        {
            Rdy.gameObject.SetActive(false);
            //Rdy.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Ready?";
        }
        else
        {
            ready = false;
            Hashtable initProps = new Hashtable() { { "PlayerReady", ready } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(initProps);

            Rdy.onClick.AddListener(() =>
            {
                ready = true;
                Hashtable props = new Hashtable() { { "PlayerReady", ready } };
                PhotonNetwork.LocalPlayer.SetCustomProperties(props);
                Rdy.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Ready!";
                FindObjectOfType<NetworkManager>().check();
            });

        }

    }
    public void Initialize(int playerId, string playerName)
    {
        ID = playerId;
        PlayerNameText.text = playerName;
    }

}
