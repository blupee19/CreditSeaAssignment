using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    [SerializeField] private Canvas mainMenu;
    [SerializeField] private Canvas startMenu;

    // Main Menu Buttons
    void Start()
    {
        mainMenu.gameObject.SetActive(true); startMenu.gameObject.SetActive(false);
    }

    public void StartButton()
    {
        mainMenu.gameObject.SetActive(false); startMenu.gameObject.SetActive(true);
    }

    public void ExitButton()
    {
        Application.Quit();
    }

    public void PayToPlayButton()
    {
        SceneManager.LoadScene(1);
    }

    public void BackToMenu() 
    {
        mainMenu.gameObject.SetActive(true); startMenu.gameObject.SetActive(false);
    }
}
