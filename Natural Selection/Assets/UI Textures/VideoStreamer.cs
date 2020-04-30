using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoStreamer : MonoBehaviour
{
    public RawImage rawimage;
    private VideoPlayer videoPlayer;
    // Start is called before the first frame update
    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        StartCoroutine(PlayVideo());
    }
    IEnumerator PlayVideo()
    {
        videoPlayer.Prepare();
        while (!videoPlayer.isPrepared)
        {
            yield return null;
            
        }
        rawimage.texture = videoPlayer.texture;
        videoPlayer.Play();
    }

}
