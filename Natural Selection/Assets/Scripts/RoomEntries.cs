using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class RoomEntries : MonoBehaviour
{
    private string roomName;

    public Text RoomNameText;
    public Text RoomPlayersText;
    public Button JoinRoomButton;

    public void Start()
    {
        JoinRoomButton.onClick.AddListener(() =>
        {
            if (PhotonNetwork.InLobby)
            {
                PhotonNetwork.LeaveLobby();
            }

            PhotonNetwork.JoinRoom(roomName);
        });
    }


    public void Initialize(string name, byte currentPlayers, byte maxPlayers)
    {
        roomName = name;

        RoomNameText.text = name;
        RoomPlayersText.text = currentPlayers + " / " + maxPlayers;
    }
}
