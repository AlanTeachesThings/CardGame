using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRules : MonoBehaviour
{
    #region Declarations
    public int numPlayers;
    //public GameObject mainDeck;
    //public List<GameObject> playerHands;
    public GameObject cardObject;

    public enum GameState {GameInit,RoundInit,TurnInit,TurnActions,TurnCleanUp,RoundCleanUp, GameEnd};
    public GameState currentState = GameState.GameInit;
    GameState nextState;
    public List<GameObject> watchedObjects;
    int reportedObjectCount = 0;
    bool messagesSent = false;
    bool actionsRun =false;
    bool phaseDone =false;
    int roundNumber = 1;
    List<Player> players;
    int currentPlayerNumber = 0;

    Deck mainDeck;
    Deck playerDeck;
    Deck discardDeck;
    #endregion

    #region Game Script
    void GameInit()
    {
        Debug.Log("Beginning Game Initialisation");
        mainDeck = FindDeckByName("MainDeck");
        discardDeck = FindDeckByName("DiscardDeck");
        players = GetPlayers();

        mainDeck.Shuffle();
        mainDeck.LayOnTable();
        mainDeck.DealToPlayers(players,"Hand",5);

        EndPhase();
    }

    void GameInitContinuous()
    {

    }
    
    void RoundInit()
    {
        Debug.Log("Beginning Round Initialisation for Round "+roundNumber);
        EndPhase();
    }

    void RoundInitContinuous()
    {
    }

    void TurnInit()
    {
        Debug.Log("Round "+roundNumber+": Beginning Turn Initialisation for Player "+currentPlayerNumber);
        playerDeck = GetCurrentPlayerDeck(players,"Hand");
        mainDeck.DealUpTo(playerDeck,5);
        playerDeck.DealDownTo(discardDeck,5);
        playerDeck.LayOnTable();
        playerDeck.ShowCards();
        EndPhase();
    }

    void TurnInitContinuous()
    {

    }

    void TurnActions()
    {
        Debug.Log("Round "+roundNumber+": Beginning Turn Actions for Player "+currentPlayerNumber);
        playerDeck.EnableCards();
        //EndPhase();
        
    }

    void TurnActionsContinuous()
    {
    }

    void TurnCleanUp()
    {
        Debug.Log("Round "+roundNumber+": Beginning Turn Clean-Up for Player "+currentPlayerNumber);
        playerDeck.HideCards();
        playerDeck.DisableCards();
        EndPhase();
    }

    void TurnCleanUpContinuous()
    {

    }

    void RoundCleanUp()
    {
        Debug.Log("Performing Round Clean-Up for Round "+roundNumber);
        EndPhase();
    }

    void RoundCleanUpContinuous()
    {

    }
    
    #endregion

    #region Script Commands & Other Stuff
    // GameState Logic after here
    void Start()
    {
        foreach (Deck deck in Resources.FindObjectsOfTypeAll(typeof(Deck)))
        {
            deck.Init();
            if(!string.IsNullOrEmpty(deck.CardDataToRead))
            {
                deck.ReadCardData();
            }
        }

        //Get all objects, and add anything with a "CheckForListener" function to the "WatchedObjects" list
        watchedObjects = new List<GameObject>();
        foreach (GameObject foundObject in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            foundObject.SendMessage("CheckForListener",gameObject.name,SendMessageOptions.DontRequireReceiver);
        }

        //Get all players, sort by turn order and then set Next Player to be the highest player number
    }

    // Update is called once per frame
    void Update()
    {
        //Read object clicky and send ClickAction message to them <--- NEEDS TO BE BEFORE GAME RULES
        if (Input.GetMouseButtonDown(0))
        {
            //Debug.Log("Clicky! Sending Ray ...");
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 10;
            Vector3 screenPosition = Camera.main.ScreenToWorldPoint(mousePos);
            RaycastHit2D hit = Physics2D.Raycast(screenPosition, Vector2.zero);
            if (hit)
            {
                hit.collider.gameObject.SendMessage("OnClicked",SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                //No hit logic goes here (audio FX, etc)
            }
        }


        //Do Logic based on Game state
        switch (currentState)
        {
            case GameState.GameInit:
            {
                doGameLogic();
                nextState = GameState.RoundInit;
                break;
            }
            case GameState.RoundInit:
            {
                doGameLogic();
                nextState = GameState.TurnInit;
                break;
            }
            case GameState.TurnInit:
            {
                doGameLogic();
                nextState = GameState.TurnActions;
                break;
            }
            case GameState.TurnActions:
            {
                doGameLogic();
                nextState = GameState.TurnCleanUp;
                break;
            }
            case GameState.TurnCleanUp:
            {
                doGameLogic();
                nextState = GameState.RoundCleanUp;
                break;
            }
            case GameState.RoundCleanUp:
            {
                doGameLogic();
                nextState = GameState.RoundInit;
                break;
            }
            
            default:
            {
                break;
            }
        }

    }

    void doGameLogic()
    {
        if(!messagesSent)
        {
            reportedObjectCount = 0;
            foreach (GameObject watchedObject in watchedObjects)
            {
                watchedObject.SendMessage("ExecuteForGameState",currentState);
            }
            messagesSent = true;
        }
        if (!actionsRun)
        {
            //Debug.Log("Should be about to Invoke"+currentState.ToString());
            Invoke(currentState.ToString(),0);
            actionsRun = true;
        }
        else
        {
            Invoke(currentState.ToString()+"Continuous",0);
        }
        if ((reportedObjectCount >= watchedObjects.Count)&&(phaseDone==true))
        {
            Debug.Log("Moving into next state: "+nextState);
            actionsRun = false;
            messagesSent = false;
            phaseDone = false;
            currentState = nextState;
        }
    }

    public void checkIn(string senderName)
    {
        //Debug.Log("Found a listener on "+senderName);
    }

    public void ReportingDone(string senderName)
    {
        reportedObjectCount++;
        //Debug.Log(senderName+" checked in: "+reportedObjectCount+" of "+watchedObjects.Count+" checked in finished with "+currentState.ToString());
    }

    public Deck FindDeckByName(string deckName)
    {
        Deck foundDeck = GameObject.Find(deckName).GetComponent<Deck>();
        return foundDeck;
    }

    public Deck[] FindDecksByTag(string tagName)
    {
        GameObject[] foundObjects = GameObject.FindGameObjectsWithTag(tagName);
        Deck[] returnDecks = new Deck[foundObjects.Length];
        for (int i = 0; i < foundObjects.Length; i++)
        {
            returnDecks[i] = foundObjects[i].GetComponent<Deck>();
        }
        return returnDecks;
    }

    public void EndPhase()
    {
        //Debug.Log("Ending Phase "+currentState);
        phaseDone = true;
        if (currentState == GameState.TurnCleanUp)
        {
            //Check if we're on the last player, and either end the phase as normal or switch the phase back to TurnInit
            if(currentPlayerNumber == players.Count-1)
            {
                currentPlayerNumber = 0;
                roundNumber++;
            }
            else
            {
                currentPlayerNumber++;
                nextState = GameState.TurnInit;
            }
        }
    }

    public void OrganisePlayers()
    {
        players.Sort(SortByTurnOrder);
    }

    static List<Player> GetPlayers()
    {
        Object[] foundPlayers = Resources.FindObjectsOfTypeAll(typeof(Player));
        List<Player> returnedPlayers = new List<Player>();
        foreach (var foundPlayer in foundPlayers)
        {
            returnedPlayers.Add((Player)foundPlayer);
        }
        Debug.Log("Found "+returnedPlayers.Count+" players");
        returnedPlayers.Sort(SortByTurnOrder);
        return(returnedPlayers);
    }

    Deck GetCurrentPlayerDeck(List<Player> players, string deckName)
    {
        //Debug.Log("Getting the deck "+players[currentPlayerNumber].GetComponentInChildren<Deck>().name+" for player number "+currentPlayerNumber+" called "+players[currentPlayerNumber].name);
        Deck[] foundPlayerDecks = players[currentPlayerNumber].GetComponentsInChildren<Deck>();
        Deck returnDeck = null;
        foreach (Deck foundPlayerDeck in foundPlayerDecks)
        {
            if (foundPlayerDeck.name == deckName)
            {
                //Debug.Log("Found "+deckName+" as "+foundPlayerDeck.name);
                returnDeck = foundPlayerDeck;
            }
        }

        return returnDeck;
    }

    static int SortByTurnOrder(Player p1, Player p2)
    {
        return p1.turnOrder.CompareTo(p2.turnOrder);
    }

    #endregion
}
