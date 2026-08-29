using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using TMPro;

public class PopupValues : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public GameObject panelPopupArea;
    public GameObject panelPopup;

    public Slider droneSelectorSlider;
    public Slider areaSizeSlider;
    public Button setButton;
    public MapDroneManager mapDroneManager;
    public NPCSettings npcSettings;
    public GameObject panelHUD;

    private int selectedDroneIndex = 0;
    private float selectedAreaSize = 0f;
    // <a href="https://www.flaticon.es/iconos-gratis/punto" title="punto iconos">Punto iconos creados por Freepik - Flaticon</a>

    public TextMeshProUGUI droneSelectorText;
    public TextMeshProUGUI areaSizeText;
    

    
    void OnEnable()
    {
        Time.timeScale = 0f; 
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        panelPopup.SetActive(true);
        panelPopupArea.SetActive(false);
    }



    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {   
        selectedAreaSize = areaSizeSlider.value;
        selectedDroneIndex = (int)droneSelectorSlider.value;
        droneSelectorText.text = selectedDroneIndex.ToString();
        areaSizeText.text = selectedAreaSize.ToString("F2");
    }

    public void AutoOnClick()
    {
        // Nada??
        setButton.gameObject.SetActive(false);
    }

    public void ManualOnClick()
    {
        setButton.gameObject.SetActive(true);
    }

    public void SetOnClick()
    {
        panelPopup.SetActive(false);
        // Aquí se podrían guardar los valores seleccionados en algún sitio para que el simulador los use
        mapDroneManager.GenerarDrones(selectedDroneIndex, selectedAreaSize);
        panelPopupArea. SetActive(true);
        


    }

    public void CrearDroneContainer()
    {
        
    }

    public void AtrasOnClick()
    {
        mapDroneManager.LimpiarMapa();
        panelPopupArea.SetActive(false);
        panelPopup.SetActive(true);
    }
    public void ConfirmCAmbiosOnClick()
    {
        panelPopupArea.SetActive(false);
        panelPopup.SetActive(true);

    }

    public void ConfirmOnClick()
    {
        npcSettings.SetupCreatNPCs();
        Time.timeScale = 1f; 
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        panelPopup.SetActive(false);
        panelHUD.SetActive(true);
        panelPopupArea.SetActive(false);
        this.gameObject.SetActive(false);


    }
}
