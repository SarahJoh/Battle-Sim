using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading;
using System.Timers;
using UnityEngine;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { set; get; }
    private bool[,] allowedMoves { get; set; }

    public Character[,] Characters { set; get; }
    private Character selectedCharacter;

    public List<GameObject> clones;
    static System.Random rnd = new System.Random();

    private const float TILE_SIZE = 1.0f;
    private const float TILE_OFFSET = 0.5f;

    private int selectionX = -1;
    private int selectionY = -1;

    public List<GameObject> characterPrefabs;
    public List<GameObject> buttonPrefabs;
    private List<GameObject> activeCharacters;
    public Sprite[] previews = new Sprite[5];
    public GameObject canvas;
    public bool running = false;

    private float waitTime = 10.0f;
    public float timer = 0.0f;
    Text countTimer;
    Text wonOrLost;
    public bool finished = false;
    public bool winner = false;

    public bool fightPhase = false;

    public void Start()
    {
            clones = new List<GameObject>();
            countTimer = GameObject.Find("Counter").GetComponent<Text>();
            wonOrLost = GameObject.Find("WonOrLost").GetComponent<Text>();

            Instance = this;
            activeCharacters = new List<GameObject>();
            Characters = new Character[8, 8];
            canvas = GameObject.Find("Canvas");
    }

    private void Update()
    {
        if (running && !fightPhase && !finished)
        {
            TimerCounter();
            if (timer <= waitTime)
            {
                UpdateSelection();

                if (Input.GetMouseButtonDown(0))
                {
                    if (selectionX >= 0 && selectionY >= 0)
                    {
                        if (selectedCharacter == null)
                        {
                            // Select chessman
                            SelectCharacter(selectionX, selectionY);
                        }
                        else
                        {
                            // Move chessman
                            MoveCharacter(selectionX, selectionY);
                        }
                    }
                }
            }
            else
            {
                countTimer.text = "";
                timer = 0.0f;
                fightPhase = true;
            }
        }
        else if (finished)
        { 
            if (!winner)
            {
                wonOrLost.text = "GAME OVER";
            } 
            else
            {
                wonOrLost.text = "YOU HAVE WON!";
            }
        }
    }

    // Updates selection based on mouse position.
    private void UpdateSelection()
    {
        if (!Camera.main)
            return;
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("ChessPlane")))
        {
            selectionX = (int)hit.point.x;
            selectionY = (int)hit.point.z;
        }
        else
        {
            selectionX = -1;
            selectionY = -1;
        }
    }

    // Selects a clicked character.
    private void SelectCharacter(int x, int y)
    {
        if (Characters[x, y] == null)
            return;

        allowedMoves = Characters[x, y].PossibleMove();
        selectedCharacter = Characters[x, y];
    }

    // Moves Character if the chosen move is allowed.
    private void MoveCharacter(int x, int y)
    {
        if(allowedMoves[x,y])
        {
            Characters[selectedCharacter.CurrentX, selectedCharacter.CurrentY].tag = "Player";
            Characters[selectedCharacter.CurrentX, selectedCharacter.CurrentY] = null;
            selectedCharacter.transform.position = GetTileCenter(x, y);
            selectedCharacter.SetPosition(x, y);
            Characters[x, y] = selectedCharacter;

            if (y > 0 && y < 7)
            {
                Characters[x, y].isActive = true;
            } 
            else
            {
                Characters[x, y].isActive = false;
                Characters[selectedCharacter.CurrentX, selectedCharacter.CurrentY].tag = "Untagged";
            }
        }
        // Resets selection if clicked on invalid tile.
        selectedCharacter = null;
    }

    // Spawns given character at given position.
    public void SpawnCharacter(int charNum, int x, int y)
    {
        if (running)
        {
            if (x >= 0)
            {
                GameObject gobj = Instantiate(characterPrefabs[charNum], GetTileCenter(x, y), Quaternion.identity) as GameObject;
                clones.Add(gobj);
                gobj.transform.SetParent(transform);
                Characters[x, y] = gobj.GetComponent<Character>();
                Characters[x, y].SetPosition(x, y);
                activeCharacters.Add(gobj);
                Characters[x, y].tag = "Player";
                if (y == 0)
                {
                    Characters[x, y].tag = "Untagged";
                    Characters[x, y].isActive = false;
                }
            }

            if (y >= 4)
            {
                Characters[x, y].tag = "Enemy";
                if (y == 7)
                {
                    Characters[x, y].tag = "Untagged";
                    Characters[x, y].isActive = false;
                }

                Characters[x, y].transform.Rotate(0.0f, 180.0f, 0.0f, Space.Self);
                Characters[x, y].isPlayer = false;
            }
        }
    }

    // Generates buttons through which characters can be bought.
    public void GeneratePreview()
    {
        for (int i = 0; i < 4; i++) {
            int num = rnd.Next(0, 5); ;
            GameObject btn = Instantiate(buttonPrefabs[i]) as GameObject;
            clones.Add(btn);
            btn.transform.SetParent(canvas.transform, false);
            btn.GetComponent<Image>().sprite = previews[num];
            btn.GetComponent<CharPreview>().prefabNum = num;
        }
    }

    // Calculate a certain tiles center.
    private Vector3 GetTileCenter(int x, int y)
    {
        Vector3 origin = Vector3.zero;
        origin.x = (x + TILE_OFFSET);
        origin.z = (y + TILE_OFFSET);
        return origin;
    }

    // Saerch for a free inventory spot.
    public int FindFreeTile(int side)
    {
        if (side == 0)
        {
            for (int i = 0; i < 8; i++)
            {
                if (Characters[i, 0] == null)
                    return i;
            }
        } 
        else if(side == 1)
        {
            for (int i = 7; i >= 0; i--)
            {
                if (Characters[i, 7] == null)
                    return i;
            }
        }
        return -1;
    }

    // Counts and displays a timer.
    private void TimerCounter()
    {
        int counter = 10 - (int)timer;
        countTimer.text = counter.ToString();

        timer += Time.deltaTime;
    }

    // Checks if any more characters can be placed this round.
    public bool BelowCharLimit()
    {
        int chars = 0;

        for (int i = 0; i < 8; i++)
        {
            for (int j = 1; j < 4; j++)
            {
                if (Characters[i, j] != null)
                    chars++;
            }
        }

        if (RoundCounter.Instance.round > chars)
            return true;

        return false;
    }

    // Spawns a random enemy on a random tile.
    public void SpawnEnemy()
    {
        int prefab = rnd.Next(0, 5);
        int x = rnd.Next(0, 7);
        int y = rnd.Next(4, 6);

        if(Characters[x, y] == null)
        {
            SpawnCharacter(prefab, x, y);
        } 
        else
        {
            SpawnEnemy();
        }
    }

    // Restores the prevously set board.
    public void RestoreBoard()
    {
        foreach(GameObject gobj in clones) {
            Destroy(gobj);
        }

        for(int i = 0; i < 8; i++) 
        {
            for(int j = 0; j < 8; j++) 
            {
                if(Characters[i, j] != null)
                {
                    SpawnCharacter(Characters[i, j].prefabNum, i, j);
                }
            }
        }
    }
}
