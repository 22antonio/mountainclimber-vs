// UIManager.cs - This controls the pause menu. It controls taking input from the user to activate the pause menu and pause all objects including cameras.
// Pause menu allows you to resume play from the moment the player stopped, Restart the level or exit to the main menu.
// Maintained by: Antonio-Angel Medel
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// AM 05-13-20 updated showPaused & hidePaused to pause the cameras. fixes bug where the cameras would keep going
public class UIManager : UIManagerSingle
{
    public GameManager gameManager;

    // AM - 05-08-20 hidePaused() will hide the pause menu by
    // setting to false all the objects with the showOnpause tag
    protected override void hidePaused()
    {
        foreach (GameObject g in pauseObjects)
        {
            g.SetActive(false);
        }

        if (gameManager.cam1.GetComponent<scroll>().enabled == false)
        {
            // resume camera's and scrolling for vs mode
            gameManager.cam1.GetComponent<scroll>().enabled = true;
            gameManager.cam2.GetComponent<scroll>().enabled = true;

            gameManager.cam1.GetComponent<Camera>().enabled = true;
            gameManager.cam2.GetComponent<Camera>().enabled = true;
        }
    }

    // AM - 05-08-20 showPaused() will show the pause menu by setting
    // to true all the objects with the showOnPause tag
    protected override void showPaused()
    {
        foreach (GameObject g in pauseObjects)
        {
            g.SetActive(true);
        }

        if (gameManager.cam1.GetComponent<scroll>().enabled == true)
        {
            // stop camera's and scrolling for vs mode
            gameManager.cam1.GetComponent<scroll>().enabled = false;
            gameManager.cam2.GetComponent<scroll>().enabled = false;

            gameManager.cam1.GetComponent<Camera>().enabled = false;
            gameManager.cam2.GetComponent<Camera>().enabled = false;
        }
    }

    // JL - 5-17-20 Fixed a "sticky" settingsmanager object, now
    // deletes on returning to menu
    protected override void LoadLevel(string level)
    {
        if(level == "Menu")
        {
            GameObject settings;
            settings = GameObject.Find("SettingsManager");
            Destroy(settings);
        }
        SceneManager.LoadScene(level);
    }
}
