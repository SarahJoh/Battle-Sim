using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CharPreview : MonoBehaviour
{
    public static CharPreview Instance { set; get; }

    private int[] prices = { 2, 1, 2, 2, 3 };

    public int prefabNum = 0;
    private int price;
    public bool clicked = false;
    private Vector2 invisible = new Vector2(0, 0);
    private Vector2 visible = new Vector2(1, 1);

    public void Start()
    {
        Instance = this;
        price = prices[prefabNum];
    }

    // Makes the clicked button inactive.
    public void BuyChar()
    {
        if (MoneyCounter.Instance.amount >= price && BoardManager.Instance.FindFreeTile(0) != -1 && !BoardManager.Instance.finished)
        {
            MoneyCounter.Instance.amount -= price;
            clicked = true;
            this.gameObject.transform.localScale = invisible;
            int freeTile = BoardManager.Instance.FindFreeTile(0);
            BoardManager.Instance.SpawnCharacter(prefabNum, freeTile, 0);
        }
    }

    // Displays a help window.
    public void ShowHowToPlay()
    {
        this.gameObject.transform.localScale = visible;
    }

    // Hides the help window.
    public void HideHowToPlay()
    {
        this.gameObject.transform.localScale = invisible;
    }

    // Shows the button that closes the help window.
    public void ShowCloseBtn()
    {
        this.gameObject.transform.localScale = new Vector2(0.2f, 1.97f);
    }

    // Hides all buttons if help window is shown.
    public void HideAllBtns()
    {
        if (BoardManager.Instance.running)
        {
            foreach (GameObject gobj in BoardManager.Instance.clones)
            {
                if (gobj != null && gobj.tag == "Button")
                {
                    gobj.gameObject.transform.localScale = invisible;
                }
            }
        }
    }

    // Shows the hidden buttons.
    public void ShowAllBtns()
    {
        if (BoardManager.Instance.running)
        {
            foreach (GameObject gobj in BoardManager.Instance.clones)
            {
                if (BoardManager.Instance.running && gobj != null && gobj.tag == "Button" && !BoardManager.Instance.finished)
                {
                    gobj.gameObject.transform.localScale = new Vector2(1.2f, 1.2f);
                }
            }
        }
    }

    // Displays the options after a finished round.
    public void FinishedRoundBtns()
    {
        GameObject exitBtn = GameObject.Find("Exit");
        GameObject restartBtn = GameObject.Find("New Round");

        exitBtn.transform.localScale = visible;
        restartBtn.transform.localScale = visible;
    }

    // Reloads the game.
    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Starts the game
    public void StartGame()
    {
        this.transform.localScale = invisible;
        BoardManager.Instance.running = true;
        BoardManager.Instance.SpawnEnemy();
        BoardManager.Instance.GeneratePreview();
    }

    // Exits the game.
    public void Exit()
    {
        BoardManager.Instance.running = false;
        Application.Quit();
    }
}
