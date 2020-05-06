using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using TMPro;

public class KillFeedEntry : MonoBehaviour
{
    public Text Player1;
    public Text Player2;

    
    public void Initialize(string player1, string player2)
    {
        Player1.text = player1;
        Player2.text = player2;
    }

}
