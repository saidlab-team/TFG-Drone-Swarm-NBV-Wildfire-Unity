using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuValues : MonoBehaviour
{
    
    private GameObject panelMenu;
    private GameObject panelHUD;
    public UIWindowController uIWindowController;
    
    public bool Pausar(GameObject panelMenu_p, GameObject panelHUD_p)
    {
        panelMenu = panelMenu_p;
        panelHUD = panelHUD_p;
        
        panelMenu.SetActive(true);
        panelHUD.SetActive(false); // Ocultamos el HUD para limpiar la pantalla

        return true; 
    }

    public bool Reanudar()
    {

        panelMenu.SetActive(false);
        panelHUD.SetActive(true);

        return false;
    }

    public void ContinuarSimulacion()
    {
        
    }

    public void Salir()
    {
        Application.Quit();
        Debug.Log("Saliendo del programa...");
    }
}
