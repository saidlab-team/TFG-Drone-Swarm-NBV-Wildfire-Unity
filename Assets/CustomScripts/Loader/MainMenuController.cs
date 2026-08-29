using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour

{
    public void LoadForestSimulationScene()
    {
        SceneManager.LoadScene("forestSimulation"); // Revisar si conviene ponerlo comno attr publicop
    }
    public void LoadCitySimulationScene()
    {
        SceneManager.LoadScene("citySimulation"); // Revisar si conviene ponerlo comno attr publicop
    }
    public void ExitScene (){
        Application.Quit();
    }
}
