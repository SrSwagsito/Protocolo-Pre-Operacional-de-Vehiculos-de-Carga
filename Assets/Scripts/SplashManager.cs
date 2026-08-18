using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class SplashManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;

public string siguienteEscena = "Initial Menu game";

 

void Start()

{

videoPlayer.loopPointReached += FinVideo;

}



void FinVideo(VideoPlayer vp)

{
    
SceneManager.LoadScene(siguienteEscena);
    
}
}
