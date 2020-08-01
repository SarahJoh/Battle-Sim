using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

public class RoundCounter : MonoBehaviour
{
    public static RoundCounter Instance { set; get; }

    Text text;
    public int round = 1;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        text = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        text.text = round.ToString();
    }
}