using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class MoneyCounter : MonoBehaviour
{
    public static MoneyCounter Instance { set; get; }

    Text text;
    public int amount = 3;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        text = GetComponent<Text>();
    }

    // Update is called once per frame.
    void Update()
    {
        text.text = amount.ToString();
    }

    // Reduces the players balance. 
    public void ReduceAmount(int price)
    {
        amount -= price;
    }
}
