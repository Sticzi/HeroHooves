using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public static bool isPaused;
    private GameObject gameMaster;

    [Header("Input Settings")]
    [SerializeField] private InputActionReference pauseAction;

    private void Awake()
    {
        pauseAction.action.Enable();
        pauseAction.action.performed += OnPauseInput;
    }

    public void Start()
    {
        gameMaster = GameObject.FindGameObjectWithTag("GM").GetComponent<GameMaster>().gameObject;
    }

    private void OnDestroy()
    {
        pauseAction.action.performed -= OnPauseInput;
    }

    private void OnPauseInput(InputAction.CallbackContext context)
    {
        TogglePause();
    }

    private void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void ResumeGame()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    private void PauseGame()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public bool IsGamePaused() => isPaused;

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        Destroy(gameMaster);
        SceneManager.LoadScene("MainMenuFinal");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}