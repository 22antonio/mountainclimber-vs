// GameManagerSingle.cs - is a derivative of the original GameManager Script. 
// This controls the camera motion. It's also the score keeping logic and the crate generation and countdown.
// Maintained by: Antonio-Angel Medel
// AM: 04-20-20: tried to make a few names more human readable
// AM: 05-08-20: updated the original GameManager script to be for single player
// AM: 05-16-20: added crates, speed up animations, background, and updated Game over for single player
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
public class GameManagerSingle : MonoBehaviour
{
    #region Field Declarations 
    [Header("Reference to Player 1 Object")]
    // References player objects
    public GameObject player1;

    // Scores
    [Space]
    [Header("Player 1 Scores")]
    public int p1Score = 0;
    // Bonus Scores from breaking crates
    public int p1BonusScore = 0;

    [Space]
    [Header("References to Main Camera")]
    // References main camera
    public Camera main_cam; // single player view
    public float starting_speed = 0.01f;

    [Space]
    [Header("Player Constants for Bounds Keeping and Score")]
    // Score offset
    public const float scoreOffset = 5.6f;
    // Max distance player can be from cam before being considered out of bounds
    public const float verticalMaxDist = 18.2f;
    public const float horizontalMaxDist = 13.0f;

    // UI Mechanics
    [Space]
    [Header("UI Mechanics for Player 1")]
    public float secondsTilRestart = 2f;
    public TextMeshProUGUI score1;
    public TextMeshProUGUI countdown;
    public GameObject gameover_ui;
    public TextMeshProUGUI gameover_text;
    protected string[] gameover_displays = new string[4] { "Gameover!\nOOOFFFF", "Gameover!\nBetter luck next time :O\n", "!gAmeOVer?\n", "At least you tried :)\n" };
    protected int show;
    protected bool gameover;
    public TextMeshProUGUI speedup1;
    public Canvas bg1;

    // Countdown
    [Space]
    [Header("Countdown")]
    public float counter = 3f;
    protected float count_time;
    protected bool rearranged = false;

    // Crate generation
    [Space]
    [Header("Crate Generation")]
    public Transform crate;
    public int chance = 500;
    protected GameObject[] ground;
    protected float crate_offsety = 0.1f;
    protected float crate_offsetx = 1.0f;
    protected int total;
    public int max_crates = 5;
    [Space]
    [Header("Minimum Scroll Speed")]
    public float min_scroll_speed = 0.01f;
    #endregion

    #region Unity Event Methods
    void Start()
    {
        // Get stuff ready
        score1.enabled = true;
        gameover_ui.SetActive(false);
        show = Random.Range(0, 4);
        gameover = false;
        count_time = counter;
        speedup1.GetComponent<Animator>().enabled = false;

        // Disable scripts and child cams for countdown on shared screen
        player1.GetComponent<PlayerMovement>().enabled = false;
        main_cam.GetComponent<scroll>().enabled = false;

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
                main_cam.GetComponent<scroll>().enabled = true;
                rearranged = true;
            }
        }
        
        // Update scores every frame
        int new_player1_score = (int)(player1.transform.position.y - scoreOffset);
        if (new_player1_score > p1Score)
        {
            p1Score = new_player1_score;
        }


        // Player 1
        float xdist = Mathf.Abs(player1.transform.position.x - main_cam.transform.position.x);

        if (player1.transform.position.y <= main_cam.transform.position.y - verticalMaxDist || xdist > horizontalMaxDist)
        {
            // Player 1 is out of bounds
            //Debug.Log("Player 1 is out of bounds");
            // Stop scrolling
            main_cam.GetComponent<scroll>().enabled = false;
            // Stop Player 1 movement
            player1.GetComponent<PlayerMovement>().enabled = false;
            bg1.enabled = false;

            StartCoroutine(DelayTilRestart());
        }


        // Players broke crates, reward with bonus points
        if (player1.GetComponent<Powerup>().CheckBonus())
        {
            p1BonusScore += 10;
        }

        score1.text = "Score: " + (p1Score + p1BonusScore);

        // AM 05-08-20: Check at a later point to implement power ups in single player
        // AM 05-16-20: Added power up's for single player
        // Check if either player picked up a powerup
       if(player1.GetComponent<Powerup>().CheckCamSlowdown())
        {
            // Slow down player 1's cam
            if(!(main_cam.GetComponent<scroll>().speed - 0.01f <= min_scroll_speed))
            {
                //Debug.Log("Slowing down player 1 cam");
                speedup1.GetComponent<Animator>().Play("Speedup");
                main_cam.GetComponent<scroll>().speed -= 0.01f;
            }
        }

        /* Super Jump is handled in player's power up script */
        // Generate a crate

        // Crate for player 1
        int selected = Random.Range(0, chance);
        if (selected == 1 && total < max_crates)
        {
            float minx = main_cam.transform.position.x - horizontalMaxDist;
            float maxx = main_cam.transform.position.x + horizontalMaxDist;
            float miny = main_cam.transform.position.y + verticalMaxDist;
            float maxy = main_cam.transform.position.y + (2 * verticalMaxDist);
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
        for (int i = 0; i < crates.Length; i++)
        {
            float c1y = main_cam.transform.position.y;
            float cratey = crates[i].transform.position.y;
            if (cratey < c1y - verticalMaxDist)
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
        if(!gameover)
        {
            gameover_text.text = gameover_displays[show] + "\n" + " You fell off!" +
            "\n\nPlayer 1: " + (p1Score + p1BonusScore).ToString();

            // Display some death message
            gameover_ui.SetActive(true);
            score1.enabled = false;
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
