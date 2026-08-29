using UnityEngine;
using TMPro;
// <a href="https://www.flaticon.es/iconos-gratis/caso-de-drone" title="caso-de-drone iconos">Caso-de-drone iconos creados por Freepik - Flaticon</a>
// <a href="https://www.flaticon.es/iconos-gratis/siguiente" title="siguiente iconos">Siguiente iconos creados por Vector Squad - Flaticon</a>

public class OrientationIndicatorScript : MonoBehaviour
{
    public Camera dron3D; // El dron que estás controlando en la escena
    public Transform dronAcompanante;
    public RectTransform iconoOrientacionUI; // La flecha o icono del dron en el Canvas

    public TextMeshProUGUI textoHUD; // Texto para mostrar la distancia
    // public float offsetImagen = 90f; 
    void Update()
    {
        if (dronAcompanante == null || iconoOrientacionUI == null) return;

        Transform camara = dron3D.transform;

        // 1. Distancia real en metros
        float distancia = Vector3.Distance(camara.position, dronAcompanante.position);
        if (textoHUD != null) {
            textoHUD.text = $"{distancia:F0} m";
        }

        // 2. Vector dirección en el plano del suelo (XZ)
        Vector3 dirHaciaTarget = dronAcompanante.position - camara.position;
        dirHaciaTarget.y = 0; // Ignoramos la diferencia de altura para que no oscile al subir/bajar

        Vector3 vistaCamara = camara.forward;
        vistaCamara.y = 0; // Solo nos importa hacia dónde miras en horizontal

        // 3. Ángulo relativo a TUS OJOS/CÁMARA
        float angulo = Vector3.SignedAngle(vistaCamara, dirHaciaTarget, Vector3.up);

        // 4. Rotamos la flecha UI corrigiendo el offset de la imagen
        iconoOrientacionUI.localEulerAngles = new Vector3(0, 0, -angulo + 90f);
    }
}
