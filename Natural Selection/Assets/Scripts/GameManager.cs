using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private bool cursorOn = false;

    void Start()
    {
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
