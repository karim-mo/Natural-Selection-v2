using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class LoadingScreen : MonoBehaviour
{
    public GameObject loadingBar;
    //public Image loading;
    public TextMeshProUGUI text;

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        SceneLoad(PlayerPrefs.GetInt("scene", 0));
        text.text = PlayerPrefs.GetString("LSText", "Connecting to match");
    }

    private void Update()
    {
        //loading.color = new Color(loading.color.r, loading.color.g, loading.color.b, Mathf.PingPong(Time.time, 1));
    }
    public void SceneLoad(int index)
    {
        StartCoroutine(LoadNextScene(index));
    }

    IEnumerator LoadNextScene(int index)
    {
        yield return new WaitForSeconds(1);
        //AsyncOperation Loading = SceneManager.LoadSceneAsync(index);
        //Loading.allowSceneActivation = false;
        if (PhotonNetwork.IsMasterClient || PlayerPrefs.GetInt("DisconnectState", 0) == 1)
        {
            PlayerPrefs.SetInt("DisconnectState", 0);
            PhotonNetwork.LoadLevel(index);
        }


        while (PhotonNetwork.LevelLoadingProgress <= 1.0f)
        {
            loadingBar.GetComponent<Slider>().value = PhotonNetwork.LevelLoadingProgress;
            if (PhotonNetwork.LevelLoadingProgress >= 0.9f)
            {
                break;
            }

            yield return null;
        }
        yield return new WaitForSeconds(1);
        loadingBar.GetComponent<Slider>().value = 0.9f;
        yield return new WaitForSeconds(1);
        loadingBar.GetComponent<Slider>().value = 1f;
        yield return new WaitForSeconds(1);

        //Loading.allowSceneActivation = true;
    }
}
