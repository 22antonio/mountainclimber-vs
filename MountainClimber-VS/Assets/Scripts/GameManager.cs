﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
// Maintained by: Juan Villasenor
// AM: 04-20-20 tried to make a few names more human readable
public class GameManager : MonoBehaviour
{
    // References player objects
    public GameObject player1;
    public GameObject player2;

    // Scores
    public int p1Score = 0;
    public int p2Score = 0;
    // Bonus Scores from breaking crates
    public int p1BonusScore = 0;
    public int p2BonusScore = 0;
    private string loser;

    // References main camera
    public Camera main_cam;
    public Camera cam1; // Player 1
    public Camera cam2; // Player 2

    // Score offset
    public const float scoreOffset = 5.6f;

    // Max distance player can be from cam before being considered out of bounds
    public const float verticalMaxDist = 18.2f;
    public const float horizontalMaxDist = 15.0f;

    // UI Mechanics
    public float secondsTilRestart = 2f;
    public TextMeshProUGUI score1;
    public TextMeshProUGUI score2;
    public GameObject gameover_ui;
    public TextMeshProUGUI gameover_text;
    string[] gameover_displays = new string[4] { "Gameover!\nOOOFFFF", "Gameover!\nBetter luck next time :O\n", "!gAmeOVer?\n", "At least you tried :)\n"};
    int show;

    void Start()
    {
        score1.enabled = true;
        score2.enabled = true;
        gameover_ui.SetActive(false);
        show = Random.Range(0, 4);
    }

    void Update()
    {
        // Update scores every frame
        int new_player1_score = (int)(player1.transform.position.y - scoreOffset);
        if (new_player1_score > p1Score)
        {
            p1Score = new_player1_score;
        }

        int new_p2Score = (int)(player2.transform.position.y - scoreOffset);
        if (new_p2Score > p2Score) p2Score = new_p2Score;

        // Player 1
        float xdist = Mathf.Abs(player1.transform.position.x - cam1.transform.position.x);

        if(player1.transform.position.y <= cam1.transform.position.y-verticalMaxDist || xdist > horizontalMaxDist)
        {
            // Player 1 is out of bounds
            Debug.Log("Player 1 is out of bounds");
            loser = "Player 1";
            // Stop scrolling
            cam1.GetComponent<scroll>().enabled = false;
            cam2.GetComponent<scroll>().enabled = false;
            // Stop Player 1 movement
            player1.GetComponent<PlayerMovement>().enabled = false;
            player2.GetComponent<PlayerMovement>().enabled = false;

            StartCoroutine(DelayTilRestart());
        }

        // Player 2 
        xdist = Mathf.Abs(player2.transform.position.x - cam2.transform.position.x);
        
        if(player2.transform.position.y <= cam2.transform.position.y-verticalMaxDist || xdist > horizontalMaxDist)
        {
            // Player 2 is out of bounds
            Debug.Log("Player 2 is out of bounds");
            loser = "Player 2";
            // Stop scrolling
            cam1.GetComponent<scroll>().enabled = false;
            cam2.GetComponent<scroll>().enabled = false;
            // Stop Player movement
            player1.GetComponent<PlayerMovement>().enabled = false;
            player2.GetComponent<PlayerMovement>().enabled = false;

            StartCoroutine(DelayTilRestart());
        }

        // Players broke crates, reward with bonus points
        if(player1.GetComponent<PlayerMovement>().checkBonus())
        {
            p1BonusScore += 10;
        }
        
        if(player2.GetComponent<PlayerMovement>().checkBonus())
        {
        p2BonusScore += 10;
        }
         
        /*
       if(p1Score > p2Score)
       {
           score.text = "Player 1 is beating Player 2 :O\nPlayer 1: " + (p1Score+p1BonusScore) + "\nPlayer 2: " + (p2Score+p2BonusScore);
       } else if(p2Score > p1Score)
       {
           score.text = "Player 2 is owning Player 1 :O\nPlayer 1: " + (p1Score+p1BonusScore) + "\nPlayer 2: " + (p2Score+p2BonusScore);
       } else
       {
           score.text = "The race is neck to neck :O\nPlayer 1: " + (p1Score+p1BonusScore) + "\nPlayer 2: " + (p2Score+p2BonusScore);
       }
       */

        score1.text = "Player 1: " + (p1Score + p1BonusScore) + "\n\n\nPlayer 2: " + (p2Score + p2BonusScore);
        score2.text = "Player 1: " + (p1Score + p1BonusScore) + "\n\n\nPlayer 2: " + (p2Score + p2BonusScore);
    }

    IEnumerator DelayTilRestart()
    {
        main_cam.enabled = true;
        gameover_text.text = gameover_displays[show] + "\n" + loser + " fell off!" +
            "\n\nPlayer 1: " + (p1Score+p1BonusScore).ToString() + "\nPlayer 2: " + (p2Score+p2BonusScore).ToString();

        // Display some death message
        gameover_ui.SetActive(true);
        score1.enabled = false;
        score2.enabled = false;

        // Wait
        yield return new WaitForSeconds(secondsTilRestart);

        // Restart the scene
        RestartScene();
    }

    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
