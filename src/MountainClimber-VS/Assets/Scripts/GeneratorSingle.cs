// GeneratorSingle.cs - is a derivative of the original Generator script. 
// This is what controls the generation of walls for the player(s). Our platforms are procedurally generated and this is the logic for it.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorSingle : MonoBehaviour
{

    //JL 3-20: added the game start instantiation
    //JL 4-20: added generation on update
    //JL 4-26-20: added distance checking for wall/platform generation
    //JL 4-28-20: added wall generation and 2nd player platforms
    //AM 5-08-20: updated original generator script to be for single player
    //AM 5-16-20: added timer and variable blocks from vs mode
    //AM 5-16-20: updated comments to reflect single player
    //AM 5-16-20: fixed issue where starting block was starting to close
    //AM 5-27-20: updated private variables to protected so they could be inherited by another class
    //AM 5-27-20: Added headers and space to make components more readable in the unity editor. 
    // Added regions to make code more managable in code editor.


    #region Field Declarations
    [Header("BlockLists for block prefabs")]
    //List of prefabs to generate platforms
    [SerializeField] protected List<Transform> blockList;
    [SerializeField] protected List<Transform> rockList;
    [SerializeField] protected List<Transform> iceList;
    [SerializeField] protected List<Transform> currentList;
    
    //The starting prefabs
    [Space]
    [Header("Starter Prefabs")]
    [SerializeField] protected Transform start;
    //The starting walls
    [SerializeField] protected Transform wall;
    [SerializeField] protected Transform middle;
    
    //The prefabs used to generate walls
    [Space]
    [Header("Prefabs for generating walls")]
    [SerializeField] protected Transform lBlock;
    [SerializeField] protected Transform mBlock;
    [SerializeField] protected Transform lRock;
    [SerializeField] protected Transform mRock;
    [SerializeField] protected Transform lIce;
    [SerializeField] protected Transform mIce;
    [SerializeField] protected Transform left;
    [SerializeField] protected Transform mid;
    
    //Player
    [Space]
    [Header("Player 1")]
    [SerializeField] protected Transform player;

    //number of block immediately generated
    protected int init = 2;

    //end positions of each of the 3 block types (p1, 2 walls)
    protected Vector3 endPos;
    protected Vector3 endWall;
    protected Vector3 endMid;

    //variables tracking and concerning game length
    public int currentTime, interval = 13, stage = 1, counter = 0;
    #endregion

    #region Unity Event Methods/Functions
    protected virtual void Awake()
    {
        //finds the end positions of the starting block
        endPos = start.Find("End").position+ new Vector3(0, 2);
        //generate two more blocks
        for (int i = 0; i < init; i++)
        {
            generateBlock();
        }
        //finds end positions of the wall
        endWall = wall.Find("End").position;
        endMid = middle.Find("End").position;
    }
    
    //checks player location relative to current end points and generate walls/platforms accordingly
    protected virtual void Update()
    {
        currentTime = (int)Time.timeSinceLevelLoad;
        if (counter > 3)
        {
            if (stage < 3)
            {
                stage += 1;
            }
            counter = 0;
        }
        if (Vector3.Distance(player.position, endPos) < 100f)
        {
            generateBlock();
            counter += 1;
        }
        if (Vector3.Distance(player.position, endWall) < 100f)
        {
            generateWall();
        }
    }
    #endregion

    #region Methods/Functions
    //generates new walls and grabs the new end points
    protected virtual void generateWall()
    {
        if (stage == 1)
        {
            left = lBlock;
            mid = mBlock;
        }
        else if (stage == 2)
        {
            left = lRock;
            mid = mRock;
        }
        else
        {
            left = lIce;
            mid = mIce;
        }
        Transform nextWall;
        nextWall = Instantiate(left, endWall, Quaternion.identity);
        endWall = nextWall.Find("End").position;
        Transform middleWall;
        middleWall = Instantiate(mid, endMid, Quaternion.identity);
        endMid = middleWall.Find("End").position;
    }

    //instantiates and returns the new platform
    protected Transform generateBlock(Transform block, Vector3 position)
    {
        Transform nextPart;
        nextPart = Instantiate(block, position, Quaternion.identity);
        return nextPart;
    }
    
    //generates new platforms and grabs the new end points
    protected virtual void generateBlock()
    {
        if (stage == 1)
        {
            currentList = blockList;
        }
        else if (stage == 2)
        {
            currentList = rockList;
        }
        else
        {
            currentList = iceList;
        }
        Transform chosen = currentList[Random.Range(0, blockList.Count)];
        Transform nextPart = generateBlock(chosen, endPos);
        endPos = nextPart.Find("End").position + new Vector3(0, 6);
    }
    #endregion
}
