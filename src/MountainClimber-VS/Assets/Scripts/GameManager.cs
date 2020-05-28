// GameManager.cs - This controls the camera motion. It's also the score keeping logic and the crate generation and countdown.
// Maintained by: Juan Villasenor - Antonio-Angel Medel as of 05-27-2020
// AM: 05-27-2020: Added inheritance to reduce duplicate code
// JV: 05-15-2020: Added background disable for both child cameras
// JV: 05-08-2020: Added speedup animation play upon camera speedup
// JV: 05-03-2020: Added countdown with animation
// JV: 04-28-2020: Implemented player 2 into script & small fixes
// JV: 04-21-2020: Fixed too high up glitch
// AM: 04-20-20 tried to make a few names more human readable
// JV: 04-16-2020: Added game over screen with animation
// JV: 04-08-2020: Added UI elements & level restart
// JV: 04-06-2020: Creation of this script
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
public class GameManager : GameManagerSingle
{
    #region Field Declarations
    // References player objects
    [Space]
    [Header("Referance to Player 2 Object")]
    public GameObject player2;

    // Scores
    [Space]
    [Header("Player 2 Scores")]
    public int p2Score = 0;
    // Bonus Scores from breaking crates
    public int p2BonusScore = 0;
    // keeping track of the loser
    private string loser;

    // References main camera
    [Space]
    [Header("References to Both Player Cameras")]
    public Camera cam1; // Player 1
    public Camera cam2; // Player 2

    // UI Mechanics
    [Space]
    [Header("UI Mechanics for player 2")]
    public TextMeshProUGUI score2;
    public TextMeshProUGUI speedup2;
    public Canvas bg2;
    #endregion

    #region Unity Event Methods
    void Start()
    {
        // Get stuff ready
        score1.enabled = true;
        score2.enabled = true;
        gameover_ui.SetActive(false);
        show = Random.Range(0, 4);
        gameover = false;
        count_time = counter;
        speedup1.GetComponent<Animator>().enabled = false;
        speedup2.GetComponent<Animator>().enabled = false;

        // Disable scripts and child cams for countdown on shared screen
        player1.GetComponent<PlayerMovement>().enabled = false;
        player2.GetComponent<PlayerMovement>().enabled = false;
        cam1.GetComponent<scroll>().enabled = false;
        cam2.GetComponent<scroll>().enabled = false;
        main_cam.GetComponent<Camera>().enabled = true;
        cam1.GetComponent<Camera>().enabled = false;
        cam2.GetComponent<Camera>().enabled = false;

        GameObject[] crates = GameObject.FindGameObjectsWithTag("Crate");
        total = crates.Length;
    }

    void Update()
    {
        // Countdown
        if (count_time > 1)
        {
            countdown.GetComponent<Animator>().ResetTrigger("Animate");
            countdown.text = count_time.ToString("0");
            countdown.GetComponent<Animator>().SetTrigger("Animate");
            count_time -= 1f * Time.deltaTime;
            return;
        }
        else if (count_time > 0.5)
        {
            countdown.text = "GO!";
            count_time -= 1f * Time.deltaTime;
            return;
        }
        else
        {
            if (!rearranged)
            {
                // Set all scripts to true and change to splitscreen
                countdown.enabled = false;
                player1.GetComponent<PlayerMovement>().enabled = true;
                player2.GetComponent<PlayerMovement>().enabled = true;
                cam1.GetComponent<scroll>().enabled = true;
                cam2.GetComponent<scroll>().enabled = true;
                main_cam.GetComponent<Camera>().enabled = false;
                cam1.GetComponent<Camera>().enabled = true;
                cam2.GetComponent<Camera>().enabled = true;
                rearranged = true;
            }
        }
        
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

        if (player1.transform.position.y <= cam1.transform.position.y - verticalMaxDist || xdist > horizontalMaxDist)
        {
            // Player 1 is out of bounds
            //Debug.Log("Player 1 is out of bounds");
            loser = "Player 1";
            // Stop scrolling
            cam1.GetComponent<scroll>().enabled = false;
            cam2.GetComponent<scroll>().enabled = false;
            // Stop Player 1 movement
            player1.GetComponent<PlayerMovement>().enabled = false;
            player2.GetComponent<PlayerMovement>().enabled = false;
            bg1.enabled = false;
            bg2.enabled = false;

            StartCoroutine(DelayTilRestart());
        }

        // Player 2 
        xdist = Mathf.Abs(player2.transform.position.x - cam2.transform.position.x);

        if (player2.transform.position.y <= cam2.transform.position.y - verticalMaxDist || xdist > horizontalMaxDist)
        {
            // Player 2 is out of bounds
            //Debug.Log("Player 2 is out of bounds");
            loser = "Player 2";
            // Stop scrolling
            cam1.GetComponent<scroll>().enabled = false;
            cam2.GetComponent<scroll>().enabled = false;
            // Stop Player movement
            player1.GetComponent<PlayerMovement>().enabled = false;
            player2.GetComponent<PlayerMovement>().enabled = false;
            bg1.enabled = false;
            bg2.enabled = false;

            StartCoroutine(DelayTilRestart());
        }

        // Players broke crates, reward with bonus points
        if (player1.GetComponent<Powerup>().CheckBonus())
        {
            p1BonusScore += 10;
        }

        if (player2.GetComponent<Powerup>().CheckBonus())
        {
            p2BonusScore += 10;
        }

        score1.text = "Score: " + (p1Score + p1BonusScore);
        score2.text = "Score: " + (p2Score + p2BonusScore);

        // Check if either player picked up a powerup
        if(player1.GetComponent<Powerup>().CheckEnemySpeedup())
        {
            // Speed up player 2's cam
            Debug.Log("Speeding up player 2 cam");
            speedup2.GetComponent<Animator>().enabled = true;
            speedup2.GetComponent<Animator>().Play("Speedup2", -1, 0);
            cam2.GetComponent<scroll>().speed += 0.01f;
        } else if(player1.GetComponent<Powerup>().CheckCamSlowdown())
        {
            // Slow down player 1's cam
            if(!(cam1.GetComponent<scroll>().speed - 0.01f <= min_scroll_speed))
            {
                Debug.Log("Slowing down player 1 cam");
                speedup1.GetComponent<Animator>().Play("Speedup");
                cam1.GetComponent<scroll>().speed -= 0.01f;
            }
        }

        if(player2.GetComponent<Powerup>().CheckEnemySpeedup())
        {
            // Speed up player 1's cam
            Debug.Log("Speeding up player 1 cam");
            speedup1.GetComponent<Animator>().enabled = true;
            speedup1.GetComponent<Animator>().Play("Speedup", -1, 0);
            cam1.GetComponent<scroll>().speed += 0.01f;
        } else if(player2.GetComponent<Powerup>().CheckCamSlowdown())
        {
            // Slow down player 2's cam
            if (!(cam2.GetComponent<scroll>().speed - 0.01f <= min_scroll_speed))
            {
                Debug.Log("Slowing down player 2 cam");
                cam2.GetComponent<scroll>().speed -= 0.01f;
            }
        }

        /* Super Jump is handled in player's power up script */
        
        // Generate a crate

        // Crate for player 1
        int selected = Random.Range(0, chance);
        if (selected == 1 && total < max_crates)
        {
            float minx = cam1.transform.position.x - horizontalMaxDist;
            float maxx = cam1.transform.position.x + horizontalMaxDist;
            float miny = cam1.transform.position.y + verticalMaxDist;
            float maxy = cam1.transform.position.y + (2 * verticalMaxDist);
            ground = GameObject.FindGameObjectsWithTag("Ground");
            int i;
            for(i = 0; i<ground.Length; i++)
            {
                // Find good position to place crate
                float xpos = ground[i].transform.position.x;
                float ypos = ground[i].transform.position.y;
                if(xpos > minx && xpos < maxx && ypos > miny && ypos < maxy)
                {
                    break;
                }
            }
            float x = ground[i].transform.position.x + crate_offsetx;
            float y = ground[i].transform.position.y + crate_offsety;

            Vector2 pos = new Vector2(x, y);
            Transform new_crate = Instantiate(crate, pos, Quaternion.identity);
            total++;
        }
        // Crate for player 2
        else if(selected == 2 && total < max_crates)
        {
            float minx = cam2.transform.position.x - horizontalMaxDist;
            float maxx = cam2.transform.position.x + horizontalMaxDist;
            float miny = cam2.transform.position.y + verticalMaxDist;
            float maxy = cam2.transform.position.y + (2 * verticalMaxDist);
            ground = GameObject.FindGameObjectsWithTag("Ground");
            int i;
            for (i = 0; i < ground.Length; i++)
            {
                // Find good position to place crate
                float xpos = ground[i].transform.position.x;
                float ypos = ground[i].transform.position.y;
                if (xpos > minx && xpos < maxx && ypos > miny && ypos < maxy)
                {
                    break;
                }
            }
            float x = ground[i].transform.position.x + crate_offsetx;
            float y = ground[i].transform.position.y + crate_offsety;

            Vector2 pos = new Vector2(x, y);
            Transform new_crate = Instantiate(crate, pos, Quaternion.identity);
            total++;
        }

        // Clean up old crates
        GameObject[] crates = GameObject.FindGameObjectsWithTag("Crate");
        total = crates.Length;
        for(int i=0; i<crates.Length; i++)
        {
            float c1y = cam1.transform.position.y;
            float c2y = cam2.transform.position.y;
            float cratey = crates[i].transform.position.y;
            if(cratey < c1y-verticalMaxDist && cratey < c2y-verticalMaxDist)
            {
                Destroy(crates[i]);
                total--;
            }
        }
    }
    #endregion

    #region Methods
    IEnumerator DelayTilRestart()
    {
        main_cam.enabled = true;
        if(!gameover)
        {
            gameover_text.text = gameover_displays[show] + "\n" + loser + " fell off!" +
            "\n\nPlayer 1: " + (p1Score + p1BonusScore).ToString() + "\nPlayer 2: " + (p2Score + p2BonusScore).ToString();

            // Display some death message
            gameover_ui.SetActive(true);
            score1.enabled = false;
            score2.enabled = false;
            gameover = true;
        }

        // Wait
        yield return new WaitForSeconds(secondsTilRestart);

        // Restart the scene
        RestartScene();
    }

    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    #endregion
}
