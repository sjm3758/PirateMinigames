﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SRMiniGame : MonoBehaviour {

    //UI Variables
    public GameObject instructionsScreen;
    public GameObject gameScreen;
    public GameObject gameOverScreen;
    public Text timerUI;
    public Text scoreUI;
    public Text goldUI;

    //Game Components
    public GameObject background;
    public GameObject boardCollider;
    public GameObject[] bounds;
    public GameObject hat;
    public GameObject hatPrefab;
    public GameObject billPrefab;
    public GameObject coinPrefab;

    //Game Variables
    private int score;
    private int gold;

    private int numCoins;

    private float spawnTime;
    private float spawnTimer;
    private int spawnCounter;

    private float gameTimer;

    private bool start;
    private bool gameOver;

    public int Score
    {
        get { return score; }
        set { score = value; }
    }

    // Use this for initialization
    void Start()
    {
        score = 0;
        gold = 0;

        numCoins = 6;

        spawnTime = 0.3f;
        spawnTimer = spawnTime;
        spawnCounter = 0;

        gameTimer = 30.0f;

        start = false;
        gameOver = false;
    }

    // Update is called once per frame
    void Update()
    {
        //Resize background if needed
        SetBackgroundToScreen();

        //Play the game until time is up
        if (start && !gameOver)
        {
            Play();
        }
    }

    /// <summary>
    /// Resizes the background image to fit the screen
    /// </summary>
    private void SetBackgroundToScreen()
    {
        /* 
            This only works if the camera is orthographic and the x and y of the
            background position is set to 0.
            In Commented Useful Code, SetBackgroundToScreen2() works for all cases.
        */

        //Precheck to avoid setting background to screen over and over
        Vector3 backgroundToScreen = Camera.main.WorldToScreenPoint(new Vector3(-background.transform.localScale.x / 2.0f, background.transform.localScale.y / 2.0f, 0.0f));
        if (backgroundToScreen.x == Screen.width && backgroundToScreen.y == Screen.height) //background already set to screen
        {
            return;
        }

        //Get screen width and height
        float width = Screen.width;
        float height = Screen.height;

        //Convert to world coordinates
        Vector3 screenToWorld = Camera.main.ScreenToWorldPoint(new Vector3(width, height, 0.0f));

        //Get distance from background's center to the world coordinates
        float xDistance = Mathf.Abs(background.transform.position.x - screenToWorld.x); //halfwidth.x
        float yDistance = Mathf.Abs(background.transform.position.y - screenToWorld.y); //halfwidth.y

        //Scale the board by this distance
        background.transform.localScale = new Vector3(xDistance * 2.0f, yDistance * 2.0f, background.transform.localScale.z);

        //Make sure the collider is wide enough to account for all aspect ratios
        //This ensures that the coins will always enter and exit the board collider during mid-game resizing
        boardCollider.transform.localScale = new Vector3(xDistance * 50.0f, yDistance * 2.0f, boardCollider.transform.localScale.z);

        //Make sure the bounds stay on the side of the board
        for (int i = 0; i < bounds.Length; i++)
        {
            //Left Bounds
            if (i == 0)
            {
                bounds[i].transform.position = new Vector3(background.transform.position.x + xDistance, 0.0f, 1.0f);
            }

            //Right Bounds
            else
            {
                bounds[i].transform.position = new Vector3(background.transform.position.x - xDistance, 0.0f, 1.0f);
            }

            bounds[i].transform.localScale = new Vector3(0.5f, background.transform.localScale.y, 1.0f);
        }

        //Make sure the hat remains on the screen
        if (hat != null)
        {
            hat.transform.position = new Vector3(hat.transform.position.x, background.transform.position.y - background.transform.localScale.y / 2.0f, 1.0f);
        }
    }

    /// <summary>
    /// Runs when the play button is clicked.
    /// Starts the game.
    /// </summary>
    public void StartGame()
    {
        //Start the game
        start = true;

        //Hide the instructions screen
        instructionsScreen.SetActive(false);

        //Show the game screen
        gameScreen.SetActive(true);

        //Spawn the hat
        hat = Instantiate(hatPrefab, new Vector3(0.0f, background.transform.position.y - background.transform.localScale.y / 2.0f, 1.0f), Quaternion.identity);
    }

    public void ExitGame()
    {
        
        //In this case, the player with the advantage is the player who the ship belongs to
        //Determine which player controls this ship
        if (PlayerInfo.instance.Advantage == PlayerAdvantage.Player1)
        {
            //Retrieve the ship data and get the current gold
            ShipData temp = PlayerInfo.instance.ShipsP1[PlayerInfo.instance.SinglePlayerShip];
            int currentGold = temp.goldAmount;

            //Calculate the new gold amount based on the winnings
            int newGold = currentGold + gold;

            //Clamp the gold to a maximum of 9
            if(newGold > 9)
            {
                newGold = 9;
            }
            
            //Give the new ship data to the PlayerInfo singleton
            temp.goldAmount = newGold;
            PlayerInfo.instance.ShipsP1[PlayerInfo.instance.SinglePlayerShip] = temp;
        }
        else if (PlayerInfo.instance.Advantage == PlayerAdvantage.Player2)
        {
            //Retrieve the ship data and get the current gold
            ShipData temp = PlayerInfo.instance.ShipsP2[PlayerInfo.instance.SinglePlayerShip];
            int currentGold = temp.goldAmount;

            //Calculate the new gold amount based on the winnings
            int newGold = currentGold + gold;

            //Clamp the gold to a maximum of 9
            if (newGold > 9)
            {
                newGold = 9;
            }

            //Give the new ship data to the PlayerInfo singleton
            temp.goldAmount = newGold;
            PlayerInfo.instance.ShipsP2[PlayerInfo.instance.SinglePlayerShip] = temp;
        }
        //Write code to switch to interface scene
        SceneManager.LoadScene("MainScene");
    }

    /// <summary>
    /// Runs the game.
    /// </summary>
    private void Play()
    {
        //Decrease game timer
        DecreaseTimer();

        //Clamp score
        if (score < 0)
        {
            score = 0;
        }

        //Update UI display
        UpdateUI();

        //Decrease coin spawn timer
        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0)
        {
            //Reset timer
            spawnTimer = spawnTime;

            //Spawn coins
            SpawnCoinInterval();

            //Increase spawn counter
            spawnCounter++;

            if (spawnCounter == 4)
            {
                //Spawn bill
                SpawnBillInterval();

                //Reset counter
                spawnCounter = 0;
            }
        }
    }

    /// <summary>
    /// Updates the UI to show the score and timer
    /// </summary>
    private void UpdateUI()
    {
        timerUI.text = gameTimer.ToString("F1") + "s";
        scoreUI.text = "Score: " + score.ToString();
    }

    /// <summary>
    /// Decreases the game timer
    /// Transitions to the game over state once the timer
    /// is finished.
    /// </summary>
    private void DecreaseTimer()
    {
        //Decrease game timer
        gameTimer -= Time.deltaTime;

        if (gameTimer <= 0)
        {
            //Set to game over
            gameOver = true;

            //Set the amount of gold that the player won
            if (score < 30)
            {
                gold = 0;
            }
            else if (score < 60)
            {
                gold = 1;
            }
            else if (score < 90)
            {
                gold = 2;
            }
            else if (score < 120)
            {
                gold = 3;
            }
            else
            {
                gold = 4;
            }

            //Hide the game screen
            gameScreen.SetActive(false);

            //Show the game over screen
            gameOverScreen.SetActive(true);

            //Show how much gold the player won
            goldUI.text = "You collected " + score.ToString() + " silver coins" + "\n";
            goldUI.text += "Congratulations, you won " + gold.ToString() + " gold";
        }
    }

    /// <summary>
    /// Spawns the coins in the game
    /// </summary>
    private void SpawnCoins()
    {
        for (int i = 0; i < 1; i++)
        {
            //Get top of the background
            float top = background.transform.position.y + background.transform.localScale.y;

            //Set height offset to spawn the coins slightly above the background
            float offset = 1.0f;

            //Get a random position along the background's width
            float x = Random.Range(background.transform.position.x - background.transform.localScale.x / 2.0f + 1.0f, background.transform.position.x + background.transform.localScale.x / 2.0f - 1.0f);

            //Spawn the coin
            //Note: Spawn at z = 1.0f because all 2D gameobjects will be moving on z = 1.0f
            GameObject coin = Instantiate(coinPrefab, new Vector3(x, top + offset, 1.0f), Quaternion.identity);

            //Set reference to minigame script
            coin.GetComponent<SRCoin>().Game = this;
        }
    }

    private void SpawnCoinInterval()
    {
        float time = 1.0f;

        for (int i = 0; i < numCoins; i++)
        {
            Invoke("SpawnCoins", time);

            time += 0.7f;
        }
    }

    private void SpawnBill()
    {
        for (int i = 0; i < 1; i++)
        {
            //Get top of the background
            float top = background.transform.position.y + background.transform.localScale.y;

            //Set height offset to spawn the bill slightly above the background
            float offset = 1.0f;

            //Get a random position along the background's width
            float x = Random.Range(background.transform.position.x - background.transform.localScale.x / 2.0f + 1.0f, background.transform.position.x + background.transform.localScale.x / 2.0f - 1.0f);

            //Spawn the bill
            //Note: Spawn at z = 1.0f because all 2D gameobjects will be moving on z = 1.0f
            GameObject bill = Instantiate(billPrefab, new Vector3(x, top + offset, 1.0f), Quaternion.identity);

            //Set reference to minigame script
            bill.GetComponent<SRBill>().Game = this;
        }
    }

    private void SpawnBillInterval()
    {
        float time = 1.0f;

        for (int i = 0; i < 5; i++)
        {
            Invoke("SpawnBill", time);

            time += 0.3f;
        }
    }

    #region Commented Useful Code
    private void SetBackgroundToScreen2()
    {
        //Set this value to the z distance between the background and the camera position
        float distanceFromCamera = -26.0f;

        //Get background's distance from the camera according to the viewport
        Vector3 bottomLeftCamera = new Vector3(0.0f, 0.0f, distanceFromCamera);

        //Get the bottom left point of the viewport based on the distance
        Vector3 bottomLeftWorld = Camera.main.ViewportToWorldPoint(bottomLeftCamera);

        //Get the x distance from the background's center to the left of the viewport
        float xDistance = Mathf.Abs(background.transform.position.x - bottomLeftWorld.x);

        //Get the background's distance from the camera according to the viewport
        Vector3 topRightCamera = new Vector3(1.0f, 1.0f, distanceFromCamera);

        //Get the top right point of the viewport based on the distance
        Vector3 topRightWorld = Camera.main.ViewportToWorldPoint(topRightCamera);

        //Get the y distance from the background's center to the top of the viewport
        float yDistance = Mathf.Abs(background.transform.position.y - topRightWorld.y);

        //Scale the background to fit the viewport
        background.transform.localScale = new Vector3(xDistance * 2.0f, yDistance * 2.0f, background.transform.localScale.z);

    }
    #endregion
}
