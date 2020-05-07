using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GM : MonoBehaviour
{
    private void Awake()
    {
        int x = PlayerPrefs.GetInt("TATuSEEsRaRSx1rd0AADexrW7leKZGrqE", 1);
        if (x == 1)
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.SetInt("TATuSEEsRaRSx1rd0AADexrW7leKZGrqE", 0);
        }
    }

    void Start()
    {
        Screen.SetResolution(PlayerPrefs.GetInt("Width", 1920), PlayerPrefs.GetInt("Height", 1080), FullScreenMode.FullScreenWindow);
    }
}