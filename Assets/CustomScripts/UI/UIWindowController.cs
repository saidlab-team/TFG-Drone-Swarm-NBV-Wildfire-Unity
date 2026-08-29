using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class UIWindowController : MonoBehaviour
{
    public GameObject panelLoadingScreen;
    public GameObject panelHUD;
    public GameObject panelMenu;
    public GameObject panelPopupArea;


    private bool estaPausado = false;

    private InputSystem_Actions m_actions;
    private InputSystem_Actions.DroneActions m_drone_actions;
    private float pauseInput = 0f;
    private MenuValues menuScript;

    void Awake()
    {
        m_actions = new InputSystem_Actions();
        m_drone_actions = m_actions.Drone;
        m_actions.Enable(); 
        menuScript = panelMenu.GetComponent<MenuValues>();
        

    }
    void OnEnable()
    {
        m_actions.Drone.Pause.performed += OnPausePerformed;
    }
    private void OnPausePerformed(InputAction.CallbackContext ctx)
    {

        if (panelLoadingScreen.activeSelf && panelLoadingScreen.GetComponent<LoadingScreenValues>().progressBar.value < 1f)
            return;

        TogglePause();
    }
    public void TogglePause()
    {
        
        if (estaPausado)
        {
            estaPausado = menuScript.Reanudar();
            Debug.Log("Reanudando simulación...");
            if (panelPopupArea.activeInHierarchy)
            {
                Debug.Log("Reanudando simulación...");
                Time.timeScale = 0f; 
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Debug.Log("Reanudando simulación...");
                Time.timeScale = 1f; 
                Cursor.lockState = CursorLockMode.Locked; // FIXED
                Cursor.visible = false;
            }
            
        }
        else
        {
            estaPausado = menuScript.Pausar(panelMenu, panelHUD);

            Debug.Log("Pausando simulación...");
            if (panelPopupArea.activeInHierarchy)
            {
                Debug.Log("Pausando simulación...");
                Time.timeScale = 0f; 
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Debug.Log("Pausando simulación...");
                Time.timeScale = 0f; 
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        panelLoadingScreen.SetActive(true);
        panelHUD.SetActive(false);
        panelMenu.SetActive(false);
        panelPopupArea.SetActive(false);
    }

    // Update is called once per frame
    bool hasShownPopup = false;
    void Update()
    {
        if (panelLoadingScreen.GetComponent<LoadingScreenValues>().progressBar.value >= 1f && !hasShownPopup)
        {
            StartCoroutine(panelLoadingScreen.GetComponent<LoadingScreenValues>().FadeOutBackground(panelLoadingScreen));
            panelPopupArea.SetActive(true);
            hasShownPopup = true;
        }
    }


}
