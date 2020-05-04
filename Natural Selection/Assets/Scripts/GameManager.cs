using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GameManager : MonoBehaviourPun
{
    private bool cursorOn = false;

    void Start()
    {
        Screen.SetResolution(PlayerPrefs.GetInt("Width", 1920), PlayerPrefs.GetInt("Height", 1080), FullScreenMode.FullScreenWindow);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }

    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            cursorOn = !cursorOn;
        }

        Cursor.visible = cursorOn;
    }
}
