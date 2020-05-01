using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;

public class LoadingScreen : MonoBehaviour
{
    public GameObject loadingBar;
    public Image loading;

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        SceneLoad(PlayerPrefs.GetInt("scene", 0));
    }

    private void Update()
    {
        loading.color = new Color(loading.color.r, loading.color.g, loading.color.b, Mathf.PingPong(Time.time, 1));
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
        if (PhotonNetwork.IsMasterClient) PhotonNetwork.LoadLevel(index);


        while (PhotonNetwork.LevelLoadingProgress <= 1.0f)
        {
            loadingBar.GetComponent<Image>().fillAmount = PhotonNetwork.LevelLoadingProgress;
            if (PhotonNetwork.LevelLoadingProgress >= 0.9f)
            {
                break;
            }

            yield return null;
        }
        yield return new WaitForSeconds(1);
        loadingBar.GetComponent<Image>().fillAmount = 0.9f;
        yield return new WaitForSeconds(1);
        loadingBar.GetComponent<Image>().fillAmount = 1f;
        yield return new WaitForSeconds(1);

        //Loading.allowSceneActivation = true;
    }
}
