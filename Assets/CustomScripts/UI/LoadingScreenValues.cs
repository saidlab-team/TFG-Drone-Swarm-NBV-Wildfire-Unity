using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingScreenValues : MonoBehaviour
{
    public Slider progressBar;
    public Image backgroundImage;
    public float fadeSpeed = 1f;

    public IEnumerator FadeOutBackground(GameObject panelLoadingScreen)
    {
        Color color = backgroundImage.color;

        while (color.a > 0f)
        {
            color.a -= Time.unscaledDeltaTime * fadeSpeed;
            backgroundImage.color = color;
            yield return null;
        }
        panelLoadingScreen.SetActive(false);
    }
}
