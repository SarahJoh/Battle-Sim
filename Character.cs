using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class Character : MonoBehaviour
{
    public Character Instance { set; get; }

    public int CurrentX { set; get; }
    public int CurrentY { set; get; }
    public bool isPlayer = true;
    public bool isActive = true;
    public string type;
    public int price;
    public int prefabNum;
    public int health;
    private int damage;
    private bool collision = false;
    public bool hasTarget = false;
    public GameObject target;
    Animator anim;

    public List<GameObject> collidedEnemies;
    public GameObject healthbar;
    public GameObject[] healthbarPrefabs = new GameObject[2];
    private Slider slider;

    public float speed = 1.0f;
    public Rigidbody rb;

    public void Start()
    {
        Instance = this;
        health = 5000;
        rb = GetComponent<Rigidbody>();
        collidedEnemies = new List<GameObject>();
        anim = GetComponent<Animator>();

        healthbar = Instantiate(healthbarPrefabs[Side()], new Vector3(CurrentX + 0.5f, 1.5f, CurrentY + 0.5f),
            Quaternion.Euler(45, 0, 0)) as GameObject;
        BoardManager.Instance.clones.Add(healthbar);
        healthbar.transform.SetParent(transform);

        slider = (Slider)FindObjectOfType(typeof(Slider));
    }

    public void FixedUpdate()
    {
        // Assures that the healthbar won't rotate with the model.
        healthbar.transform.rotation = Quaternion.Euler(45, 0, 0);

        // Checks if there are any enemies left.
        if (GameObject.FindGameObjectsWithTag("Enemy").Length > 0 && GameObject.FindGameObjectsWithTag("Player").Length > 0)
        {
            // Moves to a nearby enemy
            if (isActive && !collision && BoardManager.Instance.fightPhase)
            {
                //anim.SetBool("running", true);
                if (this.tag == "Player")
                {
                    Vector3 direction = (FindClosestEnemy(0) - transform.position).normalized;
                    rb.MovePosition(transform.position + direction * speed * Time.deltaTime);
                    Vector3 newDir = Vector3.RotateTowards(transform.forward, direction, 2 * speed * Time.deltaTime, 0.0f);
                    transform.rotation = Quaternion.LookRotation(newDir);
                }
                else if (this.tag == "Enemy")
                {
                    Vector3 direction = (FindClosestEnemy(1) - transform.position).normalized;
                    rb.MovePosition(transform.position + direction * speed * Time.deltaTime);
                    Vector3 newDir = Vector3.RotateTowards(transform.forward, direction, 2 * speed * Time.deltaTime, 0.0f);
                    transform.rotation = Quaternion.LookRotation(newDir);
                }
            }
            // Attacks the enemy if a collision is detected.
            else if (isActive && collision && BoardManager.Instance.fightPhase)
            {
                foreach (GameObject gobj in collidedEnemies)
                {
                    if (gobj != null && gobj.GetComponent<Character>().health > 0 && this.health > 0)
                    {
                        if (gobj.GetComponent<Character>().target == this.gameObject)
                        {
                            int damagePerFrame = TypeCorrelations(gobj.GetComponent<Character>().type, this.type);
                            this.health -= damagePerFrame;
                            slider.value = this.health;
                        }
                    }
                }
                // Marks a defeated enemy so that the victor can move to the next target.
                if(this.health <= 0)
                {

                    this.isActive = false;
                    this.tag = "Untagged";
                    this.transform.localScale = new Vector3(0, 0, 0);
                }
                else if (target == null || target.GetComponent<Character>().health <= 0)
                {
                    collidedEnemies.Remove(target);
                    this.hasTarget = false;
                    collision = false;
                    if (collidedEnemies.Count != 0)
                    {
                        this.target = collidedEnemies[0];
                        collision = true;
                    }
                } 
            }
        } 
        // Checks if the game has been won.
        if (GameObject.FindGameObjectsWithTag("Enemy").Length == 0 || GameObject.FindGameObjectsWithTag("Player").Length == 0 && BoardManager.Instance.fightPhase)
        {
            if (GameObject.FindGameObjectsWithTag("Player").Length == 0)
            {
                BoardManager.Instance.finished = true;
                CharPreview.Instance.HideAllBtns();
                CharPreview.Instance.FinishedRoundBtns();
                BoardManager.Instance.winner = false;
            } 
            else if(GameObject.FindGameObjectsWithTag("Enemy").Length == 0 && RoundCounter.Instance.round == 5)
            {
                BoardManager.Instance.finished = true;
                CharPreview.Instance.HideAllBtns();
                CharPreview.Instance.FinishedRoundBtns();
                BoardManager.Instance.winner = true;
            } 
            else
            {
                MoneyCounter.Instance.amount += 2;
                RoundCounter.Instance.round += 1;
                BoardManager.Instance.RestoreBoard();
                BoardManager.Instance.SpawnEnemy();
                BoardManager.Instance.GeneratePreview();
            }
            BoardManager.Instance.fightPhase = false;
        }
    }

    // Sets the current x and y locations of the character. 
    public void SetPosition(int x, int y)
    {
        CurrentX = x;
        CurrentY = y;
    }

    // Checks the characters side to save some code when spawning the healthbar. 
    public int Side()
    {
        if (isPlayer)
        {
            return 0;
        }
        else
        {
            return 1;
        }
    }

    // Calculates all possible moves based on the current board. 
    public virtual bool[,] PossibleMove()
    {
        bool[,] r = new bool[8, 8];

        if (BoardManager.Instance.BelowCharLimit() || CurrentY != 0)
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (BoardManager.Instance.Characters[i, j] == null)
                        r[i, j] = true;
                }
            }
        } 
        else
        {
            if (CurrentY == 0)
            {
                for (int i = 0; i < 8; i++)
                {
                    if (BoardManager.Instance.Characters[i, 0] == null)
                        r[i, 0] = true;
                }
            }
        }
        return r;
    }

    // Checks for collisions with enemy players. 
    private void OnCollisionEnter(Collision c)
    {
        if (rb != null && isActive && c.collider.name != "ChessPlane" && this.tag != c.gameObject.GetComponent<Character>().tag)
        {
            collision = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            collidedEnemies.Add(c.gameObject);

            if (!hasTarget)
            {
                hasTarget = true;
                target = c.gameObject;
            }
        }
    }

    // Returns the location of the closest enemy of this character. 
    private Vector3 FindClosestEnemy(int mode)
    {
        float distToClosestEnemy = Mathf.Infinity;
        GameObject closestEnemy = null;
        GameObject[] allEnemies = null;

        if (mode == 0)
        {
            allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        } else if (mode == 1)
        {
            allEnemies = GameObject.FindGameObjectsWithTag("Player");
        }

        foreach (GameObject currEnemy in allEnemies)
        {
            float distToEnemy = (currEnemy.transform.position - this.transform.position).sqrMagnitude;
            if(distToEnemy < distToClosestEnemy)
            {
                distToClosestEnemy = distToEnemy;
                closestEnemy = currEnemy;
            }
        }
        return closestEnemy.transform.position;
    }

    // Will correct a champions path if he is stuck.
    private void CorrectCurrentPath()
    {
        // tbd
    }

    // Calculates the damage dealt per second depending on the characters type. 
    private int TypeCorrelations(string attacker, string defender)
    {
        // Case if both heroes have the same type.
        if (string.Equals(attacker, defender))
        {
            return 10;
        }

        // WATER
        if (string.Equals(attacker, "Water"))
        {
            if (string.Equals(defender, "Earth") || string.Equals(defender, "Poison"))
            {
                return 8;
            }
            else if (string.Equals(defender, "Fire") || string.Equals(defender, "Air"))
            {
                return 15;
            }
        }
        // FIRE
        else if (string.Equals(attacker, "Fire"))
        {
            if (string.Equals(defender, "Water") || string.Equals(defender, "Poison"))
            {
                return 8;
            }
            else if (string.Equals(defender, "Earth") || string.Equals(defender, "Air"))
            {
                return 15;
            }
        }
        // EARTH
        else if (string.Equals(attacker, "Earth"))
        {
            if (string.Equals(defender, "Fire") || string.Equals(defender, "Poison"))
            {
                return 8;
            }
            else if (string.Equals(defender, "Water") || string.Equals(defender, "Air"))
            {
                return 15;
            }
        }
        // AIR
        else if (string.Equals(attacker, "Air"))
        {
            if (string.Equals(defender, "Water") || string.Equals(defender, "Fire") || string.Equals(defender, "Earth"))
            {
                return 8;
            }
            else if (string.Equals(defender, "Poison"))
            {
                return 15;
            }
        }
        // POISON
        else if (string.Equals(attacker, "Poison"))
        {
            if (string.Equals(defender, "Air"))
            {
                return 8;
            }
            else if (string.Equals(defender, "Water") || string.Equals(defender, "Fire") || string.Equals(defender, "Earth"))
            {
                return 15;
            }
        }
        return -1;
    }
}
