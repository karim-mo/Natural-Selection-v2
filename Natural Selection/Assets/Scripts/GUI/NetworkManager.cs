using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Player = Photon.Realtime.Player;
using TMPro;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    string gameVersion = "1";
    bool isConnecting;
    private Dictionary<string, RoomInfo> cachedRoomList;
    private Dictionary<string, GameObject> roomListEntries;
    private Dictionary<int, GameObject> playerListEntries;


    [Header("Menu")]
    public GameObject mainMenu;
    public GameObject label;

    [Header("Name Phase")]
    public GameObject namePhase;
    public GameObject _textInput;
    public GameObject Join_Create;

    [Header("Join/Create")]
    public GameObject Room;
    public GameObject RoomList;

    [Header("RoomList")]
    public GameObject RoomListEntryPrefab;
    public GameObject RoomListContent;

    [Header("Room")]
    public GameObject PlayerListEntryPrefab;
    public GameObject PlayerListContent;
    public GameObject startBtn;


    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        cachedRoomList = new Dictionary<string, RoomInfo>();
        roomListEntries = new Dictionary<string, GameObject>();
        if (PhotonNetwork.IsConnected) PhotonNetwork.Disconnect();
        PhotonNetwork.NetworkingClient.State = ClientState.Disconnected;
        if(PlayerPrefs.GetString("NAME", "NULL") != "NULL")
        {
            mainMenu.SetActive(true);
            namePhase.SetActive(false);
        }

    }

    //private void Start()
    //{
    //    if (PhotonNetwork.IsConnected) PhotonNetwork.Disconnect();
    //    PhotonNetwork.NetworkingClient.State = ClientState.Disconnected;
    //}

    public void Connect()
    {
        //AudioManager.Play("Click");
        if (isConnecting) return;
        isConnecting = true;
        label.SetActive(true);
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.NickName = PlayerPrefs.GetString("NAME");
        PhotonNetwork.ConnectUsingSettings();
    }


    #region namephase
    public void nameEnter()
    {
        //AudioManager.Play("Click");
        string text = _textInput.GetComponent<TMP_InputField>().text;
        if (text == "" || text.Length > 15 || text.Length < 5)
        {
            return;
        }
        PhotonNetwork.NetworkingClient.State = ClientState.Disconnected;
        PlayerPrefs.SetString("NAME", text);
        mainMenu.SetActive(true);
        namePhase.SetActive(false);
    }
    #endregion

    public void JoinRoom()
    {
        //AudioManager.Play("Click");
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }
        RoomList.SetActive(true);
        Join_Create.SetActive(false);
    }
    public void CreateRoom()
    {
        //AudioManager.Play("Click");
        RoomOptions options = new RoomOptions { MaxPlayers = 10 };

        PhotonNetwork.CreateRoom(PhotonNetwork.NickName + "'s Room", options, null);

        Room.SetActive(true);
        Join_Create.SetActive(false);
    }

    public void joincreateBack()
    {
        //AudioManager.Play("Click");
        isConnecting = false;
        if (PhotonNetwork.IsConnected) PhotonNetwork.Disconnect();
        PhotonNetwork.NetworkingClient.State = ClientState.Disconnected;
        mainMenu.SetActive(true);
        Join_Create.SetActive(false);
    }

    #region RoomList
    public void roomListBack()
    {
        //AudioManager.Play("Click");
        PhotonNetwork.LeaveLobby();
        Join_Create.SetActive(true);
        RoomList.SetActive(false);
    }
    #endregion


    public override void OnConnectedToMaster()
    {
        if (PhotonNetwork.IsConnected && RoomList.activeInHierarchy)
        {
            PhotonNetwork.JoinLobby();
            return;
        }
        Debug.Log("Connected!");
        label.GetComponent<TextMeshProUGUI>().text = "Connected Succesfully!";
        StartCoroutine("JoinOrCreate");
    }


    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarningFormat("Disconnected with reason {0}", cause);
    }




    #region Join/Create

    

    public override void OnJoinedRoom()
    {
        Room.SetActive(true);
        RoomList.SetActive(false);
        startBtn.SetActive(PhotonNetwork.IsMasterClient && CheckAllReady());
        if (playerListEntries == null)
        {
            playerListEntries = new Dictionary<int, GameObject>();
        }

        foreach (Photon.Realtime.Player p in PhotonNetwork.PlayerList)
        {
            GameObject entry = Instantiate(PlayerListEntryPrefab, PlayerListContent.transform);
            entry.GetComponent<PlayerEntries>().Initialize(p.ActorNumber, p.NickName);



            playerListEntries.Add(p.ActorNumber, entry);
        }
        startBtn.gameObject.SetActive(CheckAllReady());
    }

    public override void OnLeftRoom()
    {
        foreach (GameObject entry in playerListEntries.Values)
        {
            Destroy(entry.gameObject);
        }
        startBtn.gameObject.SetActive(CheckAllReady());
        playerListEntries.Clear();
        playerListEntries = null;
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        GameObject entry = Instantiate(PlayerListEntryPrefab, PlayerListContent.transform);


        entry.GetComponent<PlayerEntries>().Initialize(newPlayer.ActorNumber, newPlayer.NickName);

        playerListEntries.Add(newPlayer.ActorNumber, entry);
        startBtn.gameObject.SetActive(CheckAllReady());
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        Destroy(playerListEntries[otherPlayer.ActorNumber].gameObject);
        playerListEntries.Remove(otherPlayer.ActorNumber);
    }

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, Hashtable changedProps)
    {
        startBtn.gameObject.SetActive(CheckAllReady());
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("kms");
    }
    public override void OnLeftLobby()
    {
        Debug.Log("kms2");
    }
    

    #endregion




    #region room

    public void check()
    {
        startBtn.gameObject.SetActive(CheckAllReady());
    }

    private bool CheckAllReady()
    {
        if (!PhotonNetwork.IsMasterClient) return false;

        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
        {
            object isReady;
            if (player.CustomProperties.TryGetValue("PlayerReady", out isReady))
            {
                //object text;
                //if(player.CustomProperties.TryGetValue("PlayerReadyText", out text))
                //{
                //    RectTransform _text = (RectTransform)text;
                //    _text.GetComponent<TextMeshProUGUI>().text = (bool)isReady ? "Ready!" : "Ready?";
                //}

                if (!(bool)isReady)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

        }
        //Debug.Log("All ready");
        return true;
    }

    public void roomBack()
    {
        //AudioManager.Play("Click");
        PhotonNetwork.LeaveRoom();      
        RoomList.SetActive(true);
        PhotonNetwork.JoinLobby();
        Room.SetActive(false);
    }
    public void startGame()
    {
        //AudioManager.Play("Click");
        PhotonNetwork.OfflineMode = false;
        PlayerPrefs.SetInt("scene", SceneManager.GetActiveScene().buildIndex + 2);
        PlayerPrefs.SetString("LSText", "Connecting to match");
        PhotonNetwork.LoadLevel("LoadingScreen");
    }
    #endregion


    private void Update()
    {
        // Debug.Log(cachedRoomList.Count + " " + roomListEntries.Count);

    }

    private void ClearRoomListView()
    {
        foreach (GameObject entry in roomListEntries.Values)
        {
            Destroy(entry.gameObject);
        }

        roomListEntries.Clear();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        ClearRoomListView();

        UpdateCachedRoomList(roomList);
        UpdateRoomListView();
    }

    private void UpdateCachedRoomList(List<RoomInfo> roomList)
    {
        foreach (RoomInfo info in roomList)
        {

            if (!info.IsOpen || !info.IsVisible || info.RemovedFromList)
            {
                if (cachedRoomList.ContainsKey(info.Name))
                {
                    cachedRoomList.Remove(info.Name);
                }

                continue;
            }

            // Update cached room info
            if (cachedRoomList.ContainsKey(info.Name))
            {
                cachedRoomList[info.Name] = info;
            }

            else
            {
                cachedRoomList.Add(info.Name, info);
            }
        }
    }

    private void UpdateRoomListView()
    {
        foreach (RoomInfo info in cachedRoomList.Values)
        {
            GameObject entry = Instantiate(RoomListEntryPrefab, RoomListContent.transform);
            entry.GetComponent<RoomEntries>().Initialize(info.Name, (byte)info.PlayerCount, info.MaxPlayers);

            roomListEntries.Add(info.Name, entry);
        }
    }


    IEnumerator JoinOrCreate()
    {
        yield return new WaitForSeconds(1.5f);
        label.GetComponent<TextMeshProUGUI>().text = "Connecting to Server...";
        label.SetActive(false);
        Join_Create.SetActive(true);
        mainMenu.SetActive(false);
    }
}
