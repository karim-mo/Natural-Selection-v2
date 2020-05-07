using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks
{
    public GameObject escapePanel;
    public GameObject escapeSettings;

    public Slider mouseSens;

    private bool cursorOn = false;
    private PlayerController player;

    void Start()
    {
        Screen.SetResolution(PlayerPrefs.GetInt("Width", 1920), PlayerPrefs.GetInt("Height", 1080), FullScreenMode.FullScreenWindow);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject p in players)
        {
            //Debug.Log(p);
            if (p.GetComponent<PlayerController>().isMine())
            {
                player = p.GetComponent<PlayerController>();
                break;
            }
        }
    }

    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            cursorOn = !cursorOn;
        }

        Cursor.visible = cursorOn;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!Cursor.visible)
            {
                Cursor.visible = true;
                cursorOn = true;
            }
            player.canMove = false;
            escapePanel.SetActive(!escapePanel.activeSelf);
            if (!escapePanel.activeInHierarchy)
            {
                player.canMove = true;
                cursorOn = false;
                Cursor.visible = false;
            }
        }
    }

    public void Settings()
    {
        mouseSens.GetComponent<Slider>().value = PlayerPrefs.GetFloat("MOUSE_SENS", 1f);
        escapeSettings.SetActive(true);
        escapePanel.SetActive(false);
    }

    public void ExitMatch()
    {
        PlayerPrefs.SetInt("scene", 0);
        PlayerPrefs.SetString("LSText", "Connecting to lobby");
        WeaponDB.instance = null;
        PlayerHUD.instance = null;
        PlayerPrefs.SetInt("DisconnectState", 1);
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.LoadLevel("LoadingScreen");
    }

    public void optBack()
    {
        PlayerPrefs.SetFloat("MOUSE_SENS", mouseSens.GetComponent<Slider>().value);
        escapePanel.SetActive(true);
        escapeSettings.SetActive(false);
    }
}
