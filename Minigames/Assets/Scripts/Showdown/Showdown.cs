﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Showdown : MonoBehaviour {

    public GameObject instructionScreen;
    public GameObject gameScreen;
    public GameObject gameOver;

    public Text gameOverText;
    public GameObject pOne;
    public GameObject pTwo;

    public GameObject playerSprite;
    public GameObject cannonballSprite;

	// Use this for initialization
	void Start () {
        //instructionScreen.SetActive(false);
        //gameOver.SetActive(false);
        //gameScreen.SetActive(true);
        SpawnPlayers();
        
	}
	
	// Update is called once per frame
	void Update () {
        GameOver();
	}

    public void SpawnPlayers()
    {
        pOne = Instantiate(playerSprite, new Vector3(-3, 0, 0), Quaternion.identity);
        pOne.name = "PlayerOne";
        pOne.AddComponent<Player>();
        pTwo = Instantiate(playerSprite, new Vector3(3, 0, 0), Quaternion.identity);
        pTwo.name = "PlayerTwo";
        pTwo.AddComponent<Player>();
    }

    public void GameOver()
    {
        if (pOne.GetComponent<Player>().Health == 0)
        {
            gameOverText.text = "Player Two Wins!";
        }
        else if (pTwo.GetComponent<Player>().Health == 0)
        {
            gameOverText.text = "Player One Wins!";
        }
        //gameOverText = Instantiate(gameOverText, new Vector3(0, 2, 0), Quaternion.identity);
    }
}
