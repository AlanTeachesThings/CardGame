using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    public string ID;
    public string suit;
    public string faceValue;
    public string playValue;
    public Sprite faceSprite;
    public Sprite backSprite;
    private bool faceUpValue;
    public bool clicked = false;
    public bool cardEnabled = false;
    GameRules rulesObject;

    #region Card Actions
    public void CardActions()
    {
        //Debug.Log("Card "+gameObject.name+" is enabled!");
    }

    public void ClickActions()
    {
        //Debug.Log("You clicked on "+gameObject.name);
        DealToDeck(rulesObject.FindDeckByName("DiscardDeck"));
        rulesObject.EndPhase();
    }
    #endregion
    #region Game Rules (ARE WE USING THIS?)
    void GameInit()
    {
        Ready();
    }
    
    void RoundInit()
    {
        Ready();
    }

    void TurnInit()
    {
        Ready();
    }

    void TurnActions()
    {
        Ready();
    }

    void TurnCleanUp()
    {
        Ready();
    }

    void RoundCleanUp()
    {
        Ready();
    }
    #endregion
    #region System Stuff From Here
    public void Init()
    {
        if (!faceUp)
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = backSprite;
        }
        else
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = faceSprite;
        }
    }

    public void FlipCard()
    {
        if (faceUp)
        {
            faceUp = false;
        }
        else
        {
            faceUp = true;
        }
    }

    public void DealToDeck(Deck targetDeck)
    {
            //Debug.Log("Dealing "+this.name+" to "+targetDeck.name);
            //targetDeck.cards.Add(gameObject.GetComponent<Card>());
            this.gameObject.transform.SetParent(targetDeck.gameObject.transform, false);
            if(targetDeck.visible){ShowCard();}else{HideCard();}
            //currentDeck.cards.Remove(this);
            targetDeck.LayOnTable();
    }
    public void CheckForListener(string senderName) {
        //GameObject.Find(senderName).SendMessage("checkIn",gameObject.name);
        rulesObject = GameObject.Find(senderName).GetComponent<GameRules>();
        rulesObject.watchedObjects.Add(gameObject); // ToDo: replace this list with a dictionary of GameObjects and Bool flags?
    }

    public void ExecuteForGameState(GameRules.GameState currentState) {
        Invoke(currentState.ToString(),0);
    }
    public void Ready()
    {
        clicked = false;
        rulesObject.SendMessage("ReportingDone",gameObject.name);
        //ToDo - try and replace SendMessge with a public dictionary of GameObjects and bool flags!
    }
    public void ShowCard()
    {
        gameObject.GetComponent<SpriteRenderer>().enabled = true;
    }
    public void HideCard()
    {
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
    }
    public bool faceUp
    {
        get{
            return faceUpValue;
        }
        set{
            if (faceUp)
            {
                gameObject.GetComponent<SpriteRenderer>().sprite = backSprite;
            }
            else
            {
                gameObject.GetComponent<SpriteRenderer>().sprite = faceSprite;
            }
                faceUpValue = faceUp;
            }
    }
    public void EndPhase()
    {
        //Debug.Log("Card "+gameObject.name+" attempting to end the phase via "+rulesObject.gameObject.name);
        rulesObject.EndPhase();
    }

    public void Update()
    {
        if(cardEnabled)
        {
            CardActions();
        }
        Invoke(rulesObject.currentState.ToString(),0f);
    }

    public void OnClicked()
    {
        if(cardEnabled){ClickActions();}
    }
    public Deck currentDeck
    {
        get
        {
            return gameObject.GetComponentInParent<Deck>();
        }
        set
        {
            gameObject.transform.SetParent(value.gameObject.transform);
        }
    }
    #endregion
}
