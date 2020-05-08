using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.SceneManagement;

public class PlayerHUD : MonoBehaviourPunCallbacks
{
    public static PlayerHUD instance;

    public TextMeshProUGUI currentAmmo;
    public TextMeshProUGUI currentReserve;

    public Slider healthBar;
    public Slider staminaBar;

    public Image[] weaponImages;

    public TextMeshProUGUI Timer;
    public float gameTime;
    private float currTime = 0;
    private float _timer = 0;
    private bool gameEnded = false;


    public GameObject killFeed;
    public GameObject killEntryPrefab;
    public GameObject scoreBoard;
    public GameObject scoreBoardEntryPrefab;
    public GameObject scoreBoardParent;

    private PlayerController player;
    private StaminaHandling playerStamina;
    private void Awake()
    {
        if (instance != null) Destroy(instance);
        else
            instance = this;
    }
    void Start()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject p in players)
        {
            //Debug.Log(p);
            if (p.GetComponent<PlayerController>().isMine())
            {
                player = p.GetComponent<PlayerController>();
                playerStamina = p.GetComponent<StaminaHandling>();
                break;
            }
        }

        if (PhotonNetwork.IsMasterClient)
        {
            //Debug.Log("Yes");
            currTime = (float)PhotonNetwork.Time;
            Hashtable timerProp = new Hashtable() { { "GameTimer", currTime } };
            PhotonNetwork.CurrentRoom.SetCustomProperties(timerProp);
        }
        else
        {
            if(PhotonNetwork.CurrentRoom.CustomProperties["GameTimer"] != null)
                currTime = (float)PhotonNetwork.CurrentRoom.CustomProperties["GameTimer"];
        }
    }

    void Update()
    {
        healthBar.value = player.currHealth / player.maxHealth;
        staminaBar.value = playerStamina.m_currStamina / playerStamina.maxStamina;
        //Stamina setting


        if (currTime <= 0 && !PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.CurrentRoom.CustomProperties["GameTimer"] != null)
                currTime = (float)PhotonNetwork.CurrentRoom.CustomProperties["GameTimer"];
            return;
        }
        float timer = (float)PhotonNetwork.Time - currTime;
        _timer = gameTime - timer;

        if (_timer <= 0) _timer = 0;

        Timer.text = ((int)_timer).ToString();

        transitionToEnd();

        if (player.weapon.currWeapon == null)
        {
            currentAmmo.text = "0";
            currentReserve.text = "0";

            foreach (Image image in weaponImages)
            {
                image.gameObject.SetActive(false);
            }
            return;
        }
        currentAmmo.text = player.weapon.currWeapon.currBullets.ToString();
        currentReserve.text = player.weapon.currWeapon.currReserve.ToString();

        foreach(Image image in weaponImages)
        {
            if(image.name == player.weapon.currWeapon.name)
            {
                image.gameObject.SetActive(true);
            }
            else
            {
                image.gameObject.SetActive(false);
            }
        }

        
    }

    public void transitionToEnd()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (_timer <= 0 && !gameEnded)
        {
            gameEnded = true;
            StartCoroutine("EndGame");
        }
    }

    public void addToFeed(string player1, string player2)
    {
        GameObject entry = Instantiate(killEntryPrefab, killFeed.transform);
        entry.GetComponent<KillFeedEntry>().Initialize(player1, player2);

        Destroy(entry, 10f);
    }

    IEnumerator EndGame()
    {
        photonView.RPC("activateScoreBoard", 
            RpcTarget.All
            );
        PhotonNetwork.SendAllOutgoingCommands();
        yield return new WaitForSeconds(20f);
        photonView.RPC("_EndGame",
            RpcTarget.All
            );
        PhotonNetwork.SendAllOutgoingCommands();
    }

    [PunRPC]
    public void activateScoreBoard()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject p in players)
        {
            if (p.GetComponent<PlayerController>().isMine())
            {
                p.GetComponent<PlayerController>().canMove = false;
                break;
            }
        }
        scoreBoard.SetActive(true);
        foreach(GameObject p in players)
        {
            GameObject entry = Instantiate(scoreBoardEntryPrefab, scoreBoardParent.transform);
            entry.GetComponent<ScoreboardEntry>().Initialize(p.GetComponent<PhotonView>().Owner.NickName, p.GetComponent<PlayerController>().kills.ToString(), p.GetComponent<PlayerController>().deaths.ToString());
        }
    }

    [PunRPC]
    public void _EndGame()
    {
        PlayerPrefs.SetInt("scene", 0);
        PlayerPrefs.SetString("LSText", "Connecting to lobby");
        //SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        WeaponDB.instance = null;
        instance = null;
        SceneManager.LoadScene(1);
    }
}
