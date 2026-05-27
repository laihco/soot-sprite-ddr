using UnityEngine;
using UnityEngine.SceneManagement;

public class MMManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static MMManager _;

    [SerializeField] private bool _debugMode;
    public enum MMButtons { Play, Options, Credits, Quit };
    public enum CreditsBack { Back };
    public enum OptionsBack { Back };

    [SerializeField] private GameObject _MainMenuContainer;
    [SerializeField] private GameObject _CreditsMenuContainer;
    [SerializeField] private GameObject _OptionsMenuContainer;

    [SerializeField] private string _sceneToLoadAfterClickingPlay;
    [SerializeField] private string _sceneToLoadAfterClickingBack;
    public void Awake()
    {
        if (_ == null)
        {
            _ = this;
        }
        else
        {
            Debug.LogError("There are more than 1 MainMenuManager's in the Scene");
        }
    }


    private void Start()
    {
        OpenMenu(_MainMenuContainer);
    }
    public void MMButtonClicked(MMButtons button)
    {
        DebugMessage("Button clicked: " + button.ToString());
        switch (button)
        {
            case MMButtons.Play:
                PlayClicked();
                break;
            case MMButtons.Options:
                OpenOptionsMenu();
                break;
            case MMButtons.Credits:
                Debug.Log("clicking credits button");
                OpenCreditsMenu();
                break;
            case MMButtons.Quit:
                QuitGame();
                break;
            default:
                Debug.LogError("Invalid button clicked: " + button.ToString());
                break;

        }
    }

    public void ReturnToMM()
    {
        SceneManager.LoadScene(_sceneToLoadAfterClickingBack);
    }

    public void CreditsButtonClicked(CreditsBack button)
    {
        switch (button)
        {
            case CreditsBack.Back:
                ReturnToMM();
                break;
        }
    }

    public void OptionsButtonClicked(OptionsBack button)
    {
        switch (button)
        {
            case OptionsBack.Back:
                ReturnToMM();
                break;
        }
    }

    private void PlayClicked()
    {
        SceneManager.LoadScene(_sceneToLoadAfterClickingPlay);
    }


    public void QuitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
        #else
               Application.Quit();
        #endif

    }
    private void DebugMessage(string message)
    {
        if (_debugMode)
        {
            Debug.Log(message);
        }
    }

    public void OpenOptionsMenu()
    {
        OpenMenu(_OptionsMenuContainer);
    }

    public void OpenCreditsMenu()
    {
        Debug.Log("called OpenCreditsMenu");
        OpenMenu(_CreditsMenuContainer);
    }
    public void OpenMenu(GameObject menu)
    {
        Debug.Log("Opening menu: " + menu.name);
        _MainMenuContainer.SetActive(menu == _MainMenuContainer);
        _CreditsMenuContainer.SetActive(menu == _CreditsMenuContainer);
        _OptionsMenuContainer.SetActive(menu == _OptionsMenuContainer);
    }
}
