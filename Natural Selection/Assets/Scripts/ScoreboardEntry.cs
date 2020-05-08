using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using TMPro;

public class ScoreboardEntry : MonoBehaviour
{
    public TextMeshProUGUI Player;
    public TextMeshProUGUI Kills;
    public TextMeshProUGUI Deaths;


    public void Initialize(string player, string kills, string deaths)
    {
        Player.text = player;
        Kills.text = kills;
        Deaths.text = deaths;
    }

}
