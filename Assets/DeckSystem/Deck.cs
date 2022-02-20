using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    #region Declarations
    Dictionary<string, Sprite> Sprites;
    public string CardDataToRead;
    public string CardFaceSpritesName;
    public string CardBackSpritesName;
    public GameObject CardObject;
    public Vector2 gridSize;
    public Vector2 gridSpacing;
    public bool faceUp;
    public bool visible;
    GameRules rulesObject;
    #endregion

    #region Game Script
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

    #region Script Commands
    #region Deck Setup
    // **************
    // ReadCardData
    // **************
    // Takes the string name established in the variable "CardDataToRead" and uses this to load CSV data with the following column structure:
    // 0: ID
    // 1: name
    // 2: suit
    // 3: faceValue
    // 4: faceSprite
    // 5: backSprite

    // TODO: Find a nice generic way to structure this data
    // TODO: Add a column for the name of the Prefab used for the Cards

    public void ReadCardData()
    {
        //cardObjects.Clear();
        //cards.Clear();

        Sprites = new Dictionary<string, Sprite>();

        LoadSprites(CardFaceSpritesName);
        LoadSprites(CardBackSpritesName);
        /*foreach (string spriteName in Sprites.Keys)
        {
            Debug.Log("Loaded "+Sprites[spriteName]+" as "+spriteName);
        }*/

        TextAsset loadedCardData = Resources.Load<TextAsset>(CardDataToRead); // Load specified CSV file
        string[] rows = loadedCardData.text.Split(new char[]{'\n'}); // split text of file into rows

        for (int rowNumber = 1; rowNumber < rows.Length -1; rowNumber++)
        {
            string[] rowData = rows[rowNumber].Split(new char[]{','});
            GameObject newCardObject = Instantiate(CardObject, transform.position, transform.rotation);
            newCardObject.GetComponent<Card>().ID = rowData[0].Trim('\r','\n');
            newCardObject.GetComponent<Card>().name = rowData[0].Trim('\r','\n');
            newCardObject.GetComponent<Card>().suit = rowData[1].Trim('\r','\n');
            newCardObject.GetComponent<Card>().faceValue = rowData[2].Trim('\r','\n');
            newCardObject.GetComponent<Card>().faceSprite = GetSpriteByName(rowData[4].Trim('\r','\n'));
            newCardObject.GetComponent<Card>().backSprite = GetSpriteByName(rowData[5].Trim('\r','\n'));
            newCardObject.GetComponent<Card>().faceUp = faceUp;
            newCardObject.GetComponent<BoxCollider2D>().size=newCardObject.GetComponent<SpriteRenderer>().bounds.size;
            newCardObject.GetComponent<Card>().Init();
            newCardObject.transform.SetParent(this.gameObject.transform, false);
            //cardObjects.Add(newCardObject);
            //cards.Add(newCardObject.GetComponent<Card>());
        }
        HideCards();
    }

    // *********
    // Shuffle
    // *********
    // Uses the Fisher Yates Shuffle to reorganise the cards in "cards" into a random order
    public void Shuffle()
    {
        int i = 0;
        foreach (Card card in Fisher_Yates_CardDeck_Shuffle(cards))
        {
            cards[i].gameObject.transform.SetSiblingIndex(cards.IndexOf(card));
        }
    }
    #endregion
    #region Dealing Cards
    // *************************************
    // DealToHand(targetDeck, cardsToDeal)
    // *************************************
    // Transfers the number of cards specified in "cardsToDeal" to the deck specified by "targetDeck"
    public void DealToHand(Deck targetDeck, int cardsToDeal)
    {
        for (int currentCard = 0; currentCard < cardsToDeal; currentCard++)
        {
            cards[0].faceUp = targetDeck.faceUp;
            cards[0].gameObject.transform.SetParent(targetDeck.gameObject.transform, false);
            if(targetDeck.visible){cards[0].gameObject.GetComponent<Card>().ShowCard();}else{cards[0].gameObject.GetComponent<Card>().HideCard();}
        }
    }

    // *************************************
    // DealUpTo(targetDeck, deckSize)
    // *************************************
    // Transfers cards to the deck specified by "targetDeck" until the targetDeck size is deckSize or larger
    public void DealUpTo(Deck targetDeck, int deckSize)
    {
        while (targetDeck.cards.Count < deckSize)
        {
            cards[0].faceUp = targetDeck.faceUp;
            cards[0].gameObject.transform.SetParent(targetDeck.gameObject.transform, false);
            if(targetDeck.visible){cards[0].gameObject.GetComponent<Card>().ShowCard();}else{cards[0].gameObject.GetComponent<Card>().HideCard();}
        }
    }

    // *************************************
    // DealHandUpTo(targetDeck, deckSize)
    // *************************************
    // Transfers cards to the deck specified by "targetDeck" until this deck's size is deckSize
    public void DealDownTo(Deck targetDeck, int deckSize)
    {
        while (cards.Count > deckSize)
        {
            cards[0].faceUp = targetDeck.faceUp;
            cards[0].gameObject.transform.SetParent(targetDeck.gameObject.transform, false);
            if(targetDeck.visible){cards[0].gameObject.GetComponent<Card>().ShowCard();}else{cards[0].gameObject.GetComponent<Card>().HideCard();}
        }
    }

    // *************************************
    // DealToHands(targetDecks, cardsToDeal)
    // *************************************
    // Transfers the number of cards specified in "cardsToDeal" to each of the decks specified by "targetDecks"
    public void DealToHands(Deck[] targetDecks, int cardsToDeal)
    {
        foreach (Deck targetDeck in targetDecks)
        {
            for (int currentCard = 0; currentCard < cardsToDeal; currentCard++)
            {
                cards[0].faceUp = targetDeck.faceUp;
                cards[0].gameObject.transform.SetParent(targetDeck.gameObject.transform, false);
                if(targetDeck.visible){cards[0].gameObject.GetComponent<Card>().ShowCard();}else{cards[0].gameObject.GetComponent<Card>().HideCard();}
            }
        }
    }

    // *************************************
    // Deal To Players(targetDecks, cardsToDeal)
    // *************************************
    // Assumes all player objects contain a deck with a name specified by "deckName".
    // Transfers the number of cards specified in "cardsToDeal" to the (first found) deck specifed by "deckName" under each of the players specified by "players"
    public void DealToPlayers(List<Player> players, string deckName, int cardsToDeal)
    {
        foreach (Player player in players)
        {
            Deck playerDeck = null;
            Deck[] foundDecks = player.GetComponentsInChildren<Deck>(false);
            foreach (Deck foundDeck in foundDecks)
            {
                if(foundDeck.name == deckName){
                    playerDeck = foundDeck;
                }
            }
            if(playerDeck!=null)
            {
                for (int currentCard = 0; currentCard < cardsToDeal; currentCard++)
                {
                    cards[0].faceUp = playerDeck.faceUp;
                    cards[0].gameObject.transform.SetParent(playerDeck.gameObject.transform, false);
                    if(playerDeck.visible){cards[0].gameObject.GetComponent<Card>().ShowCard();}else{cards[0].gameObject.GetComponent<Card>().HideCard();}
                }
            }
        }
    }


    #endregion
    #region Showing, Hiding and Flipping Cards
    
    // ***********
    // LayOnTable 
    // ***********
    // Shows Cards
    // Positions cards on the table based on the grid size specified by "gridSize", spaced out as specified by "gridSpacing"
    // Excess cards are not displayed
    public void LayOnTable()
    {
        //Debug.Log("Laying "+cards.Count+" Cards On Table (no bool)");
        ShowCards();
        Vector2 position = new Vector2(0,0);
        int row=1;
        int col=1;
        foreach (Card cardToBeLayed in cards)
        {
            //Debug.Log("Putting card "+cardToBeLayed.gameObject.name+" at position "+position);
            cardToBeLayed.gameObject.transform.localPosition = position;
            position.x += gridSpacing.x;
            col++;
            if(col > gridSize.x){row++; col = 1; position.x = 0; position.y -= gridSpacing.y;}
            if(row > gridSize.y){row--; col=(int)gridSize.x; break;}
        }
        //Debug.Log("Final grid size is "+col+"x"+row);
        Vector3 offset = new Vector3((gridSpacing.x * ((float)col-1f))/2,(gridSpacing.y * ((float)row-1f))/2,0);
        //Debug.Log("Offset calculated to "+offset.ToString());
        offset.y = offset.y*-1;
        foreach (Card card in cards)
        {
            card.gameObject.transform.localPosition -= offset;
        }
    }

    // ****************************
    // LayOnTable (bool facingUp)
    // ****************************
    // As above, but specifes whether cards are face up or face down as specified by "facingUp"

    public void LayOnTable(bool facingUp)
    {
        //Debug.Log("Laying Cards On Table (no bool)");
        ShowCards();
        Vector2 position = new Vector2(0,0);
        int row=1;
        int col=1;
        foreach (Card card in cards)
        {
            card.faceUp = facingUp;
            card.gameObject.transform.localPosition = position;
            position.x += gridSpacing.x;
            col++;
            if(col > gridSize.x){row++; col = 1; position.x = 0; position.y -= gridSpacing.y;}
            if(row > gridSize.y){row--; col=(int)gridSize.x; break;}
        }
        //Debug.Log("Final grid size is "+col+"x"+row);
        Vector3 offset = new Vector3((gridSpacing.x * ((float)col-1f))/2,(gridSpacing.y * ((float)row-1f))/2,0);
        //Debug.Log("Offset calculated to "+offset.ToString());
        offset.y = offset.y*-1;
        foreach (Card card in cards)
        {
            card.gameObject.transform.localPosition -= offset;
        }
    }

    // **********
    // FlipCards
    // **********
    // Flips all cards in "cards"
    public void FlipCards()
    {
        foreach (Card card in cards)
        {
            card.FlipCard();
        }
    }

    // **************************
    // FlipCards (CardsToFlip)
    // **************************
    // Flips a number of cards in "cards" specified by CardsToFlip
    public void FlipCards(int CardsToFlip)
    {
        for (int i = 0; i < CardsToFlip; i++)
        {
            cards[i].FlipCard();
        }
    }

    // **********
    // ShowCards
    // **********
    // Shows all cards in "cards"
    public void ShowCards()
    {
        foreach (Card card in cards)
        {
            card.ShowCard();            
        }
    }

    // **************************
    // ShowCards (CardsToShow)
    // **************************
    // Shows a number of cards in "cards" specified by CardsToShow
    public void ShowCards(int CardsToShow)
    {
        for (int i = 0; i < CardsToShow; i++)
        {
            cards[i].ShowCard();
        }
    }

    // (See above, but for hiding)
    public void HideCards()
    {
        visible = false;
        foreach (Card card in cards)
        {
            card.HideCard();
        }
    }
    public void HideCards(int CardsToHide)
    {
        for (int i = 0; i < CardsToHide; i++)
        {
            cards[i].HideCard();
        }
    }
    #endregion
    #region Working with Card Actions
    public void EnableCards()
    {
        foreach (Card card in cards)
        {
            card.cardEnabled = true;
        }
    }
    public void DisableCards()
    {
        foreach (Card card in cards)
        {
            card.cardEnabled = false;
        }
    }
    #endregion
    #region General Game Scripts
    // *********
    // EndPhase
    // *********
    // Tells the system that the actions for the current phase are complete. The system will progress when it receives the EndPhase message from every object in the game.
    public void Ready()
    {
        rulesObject.SendMessage("ReportingDone",gameObject.name);
    }
    #endregion
    #endregion

    #region Intermediate Tools and Other Scripts
    // System stuff from here on!!!!

    // Lifted from https://answers.unity.com/questions/486626/how-can-i-shuffle-alist.html
    
    //================================================================//
     //===================Fisher_Yates_CardDeck_Shuffle====================//
     //================================================================//
 
     /// With the Fisher-Yates shuffle, first implemented on computers by Durstenfeld in 1964, 
     ///   we randomly sort elements. This is an accurate, effective shuffling method for all array types.
 
    public void Init()
    {
        //cards = new List<Card>();
    }
     private static List<Card> Fisher_Yates_CardDeck_Shuffle (List<Card>aList) {
 
         System.Random _random = new System.Random ();
 
         Card myGO;
 
         int n = aList.Count;
         for (int i = 0; i < n; i++)
         {
             // NextDouble returns a random number between 0 and 1.
             // ... It is equivalent to Math.random() in Java.
             int r = i + (int)(_random.NextDouble() * (n - i));
             myGO = aList[r];
             aList[r] = aList[i];
             aList[i] = myGO;
         }
 
         return aList;
     }

    // Modified from https://gamedev.stackexchange.com/questions/102268/accessing-the-child-sprites-of-a-sprite-sheet-in-unity
    private void LoadSprites(string spriteSheetName) {
        Sprite[] SpritesData = Resources.LoadAll<Sprite>(spriteSheetName);
        for (int i = 0; i < SpritesData.Length; i++)
        {
            Sprites.Add(SpritesData[i].name, SpritesData[i]);
        }
    }

    public Sprite GetSpriteByName(string name) {
        
        if (Sprites.ContainsKey(name)){
            //Debug.Log("Found it!");
            return Sprites[name];
        }
        else {
            //Debug.Log("Didn't find it!");
            return null;
        } 
            
    }

    public void CheckForListener(string senderName) {
        //GameObject.Find(senderName).SendMessage("checkIn",gameObject.name);
        rulesObject = GameObject.Find(senderName).GetComponent<GameRules>();
        rulesObject.watchedObjects.Add(gameObject);
    }

    public void ExecuteForGameState(GameRules.GameState currentState) {
        Invoke(currentState.ToString(),0);
    }

    public List<Card> cards {
        get {
            List<Card> returnList = new List<Card>();
            foreach (Transform child in transform)
            {
                if(child.TryGetComponent(out Card card)){
                    returnList.Add(card);
                }

            }
            return returnList;
        }
    }

    #endregion

}
