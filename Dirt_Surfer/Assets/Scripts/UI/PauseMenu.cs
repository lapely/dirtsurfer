using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public static bool gameIsPaused;
    public GameObject pauseMenuUI;
    public GameObject inGameUI;
    public GameObject playerCar;
    public PlayerInput carInput;
    private InputAction openMenu;

    private bool paused = false;
    private bool keyPressed = false;

    CountDown countdown;

    Vector3 startPosition;
    private void Start()
    {
        startPosition = playerCar.transform.position;
        countdown = GetComponent<CountDown>();
        //Debug.Log(countdown);
    }

    private void Update()
    {
        
        openMenu = carInput.actions["OpenPauseMenu"];

        if (openMenu.IsPressed() && keyPressed == false)
        {
            keyPressed = true;
            if(paused)
            {
                Resume();
                paused = false;
            } 
            else
            {
                Pause();
                paused = true;
            }

        }
        else if(!openMenu.IsPressed())
        {
            keyPressed = false;
        }
    }
    public void Resume()
    {
        Time.timeScale = 1f;
        pauseMenuUI.SetActive(false);
        inGameUI.SetActive(true);
        gameIsPaused = false;
    }

    void Pause()
    {
        Time.timeScale = 0f;
        pauseMenuUI.SetActive(true);
        inGameUI.SetActive(false);
        gameIsPaused = true;
    }
    public void restartTheRace()
    {
        playerCar.transform.position = startPosition;
        countdown.theCountDown();
    }
    public void LoadSetting()
    {

    }   
    public void LoadMenu()
    {
        
    }
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }
}
