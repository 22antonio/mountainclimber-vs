// GeneratorDouble.cs - This is what controls the generation of walls for the player(s). Our platforms are procedurally generated and this is the logic for it.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorDouble : GeneratorSingle
{

    //JL 3-4-20: added the game start instantiation
    //JL 3-30-20: added generation on update
    //JL 4-19-20: small fixes
    //JL 4-26-20: added distance checking for wall/platform generation, blocks now spawn infinitely
    //JL 4-28-20: added wall generation and 2nd player platforms
    //JL 4-29-20: modified to support a second player and its platforms
    //JL 5-11-20: added counter, variable, and functionality for platform biome change
    //JL 5-11-20: implemented settingsmanager from game menu to import unique terrain boolean
    //AM 5-16-20: updated file description
    //AM 5-27-20: Adjusted GeneratorDouble to inherate from GeneratorSingle to reduce duplicate code
    //AM 5-27-20: Added headers and space to make components more readable in the unity editor. 
        // Added regions to make code more managable in code editor.

    #region Field Declarations
    [Space]
    [Header("Transform Fields for 2 Player")]
    
    //The two starting prefabs
    [Space]
    [Header("Starter Prefabs For Player 2")]
    [SerializeField] private Transform start2;
    //The three starting walls
    [SerializeField] private Transform wall2;

    //The prefabs used to generate walls
    [Space]
    [Header("Prefab for generating walls for player 2")]
    [SerializeField] private Transform rBlock;
    [SerializeField] private Transform rRock;
    [SerializeField] private Transform rIce;
    [SerializeField] private Transform right;
    
    //Players
    [Space]
    [Header("Player 2")]
    [SerializeField] private Transform player2;
    
    //end positions of each of the 5 block types (p1, p2, 3 walls)
    private Vector3 endPos2;
    private Vector3 endWall2;
    
    //for generation settings
    [Space]
    [Header("Unique Player Terrain Setting")]
    public bool unique = false;
    GameObject settings;
    #endregion

    #region Unity Event Methods/Functions
    protected override void Awake()
    {
        //Finds the settingsmanager object and its 'uniqueOn' variable
        settings = GameObject.Find("SettingsManager");
        SettingsManager uniqueToggle = settings.GetComponent<SettingsManager>();
        unique = uniqueToggle.uniqueOn;
        base.Awake();
        //finds the end positions of the two starting blocks
        endPos2 = start2.Find("End").position + new Vector3(0, 2);
        //finds end positions of the wall
        endWall2 = wall2.Find("End").position;
    }

    //checks player locations relative to current end points and generate walls/platforms accordingly
    protected override void Update()
    {
        currentTime = (int)Time.timeSinceLevelLoad;
        if(counter > 3)
        {
            if (stage < 3)
            {
                stage += 1;
            }
            counter = 0;
        }
        if ((Vector3.Distance(player.position, endPos) < 100f) || (Vector3.Distance(player2.position, endPos2) < 100f))
        {
            generateBlock();
            counter += 1;
        }
        if ((Vector3.Distance(player.position, endWall) < 100f) || (Vector3.Distance(player2.position, endWall2) < 100f))
        {
            generateWall();
        }
    }
    #endregion

    #region Methods/Functions
    //generates new walls and grabs the new end points
    protected override void generateWall()
    {
        if (stage == 1)
        {
            left = lBlock;
            right = rBlock;
            mid = mBlock;
        }
        else if(stage == 2)
        {
            left = lRock;
            right = rRock;
            mid = mRock;
        }
        else
        {
            left = lIce;
            right = rIce;
            mid = mIce;
        }
        Transform nextWall;
        nextWall = Instantiate(left, endWall, Quaternion.identity);
        endWall = nextWall.Find("End").position;
        Transform nextWall2;
        nextWall2 = Instantiate(right, endWall2, Quaternion.identity);
        endWall2 = nextWall2.Find("End").position;
        Transform middleWall;
        middleWall = Instantiate(mid, endMid, Quaternion.identity);
        endMid = middleWall.Find("End").position;
    }

    //generates new platforms and grabs the new end points
    // changed to call from base class as to reduce on rewritten code
    protected override void generateBlock()
    {
        base.generateBlock();

        Transform chosen = currentList[Random.Range(0, blockList.Count)];
        Transform chosen2;

        if (unique)
        {
            chosen2 = currentList[Random.Range(0, currentList.Count)];
        }
        else
        {
            chosen2 = chosen;
        }

        Transform part2 = generateBlock(chosen2, endPos2);
        endPos2 = part2.Find("End").position + new Vector3(0, 6);
    }
    #endregion
}
