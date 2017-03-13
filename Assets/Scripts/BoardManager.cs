using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Events;

[System.Serializable]
public class GridSize
{
    public int X;
    public int Y;
}
public class BoardManager : MonoBehaviour
{
    #region Variables
    //Grid
    public GridSize gridSize;
    //Initial class implementation
    private BlocksArray blocks;
    private ColorBase colorBase = new ColorBase();
    //Default block color of grid
    public Color defaultBlockColor;
    public int matchesNeededToGiveDiamond;
    int matchCount;
    [Tooltip("Number of diamonds that player will get")]
    public int diamondReward;
    private int diamondBank = 1000;
    public int powerUpDiamondPrice = 250;
    public Text diamondBankText;
    public Image matchCountBar;
    public GameObject deductionPanel;
    //Blocks placed holders
    Block currentBlock;
    Block previousBlock;
    List<Block> blocksPlaced = new List<Block>();
    // players input on how many blocks he dragged
    private int selectionCount = 0;
    //General Game Variables
    public GameObject blockPrefab;
    public RectTransform gamePanel;
    public static GameState gameState;
    //New Blocks Variables
    public RectTransform newBlocksPanel;
    private Image[] newBlockImages;
    List<BlockInfo> newBlocks = new List<BlockInfo>();
    //Score Variables
    public Text scoreText;
    public Text highScoreText;
    private int score;
    private int highScore;
    private int updatedScore;
    private bool isScoreIncreased;
    //Combo and New Color Text
    public Text comboText;
    public Animator newColorAnim;
    public Animator newColorTextAnim;
    //Hint Blocks Variables
    public RectTransform hintBlocksPanel;
    private Image[] hintBlockImages;
    private bool isHintUsed;
    List<BlockInfo> hintBlocks = new List<BlockInfo>();
    //Difficulty settings
    public int[] difficultyBrackets;
    private int difficultyCounter;
    private int currentDifficultyBracket = 0;
    //Unity Events
    public UnityEvent gameOverEvent;

    #region integration safecheck variables
    //hardcoded default values of Game Panel's Grid Layout Group's spacing and cellsize
    float defaultSpacingX = 8;
    float defaultSpacingY = 8;
    float defaultCellSizeX = 64;
    float defaultCellSizeY = 64;
    #endregion

    #endregion

    #region Unity MonoBehaviours 
    void Awake()
    {
        #region safecheck
        if (colorBase.GetTotalDifficulty() != difficultyBrackets.Length)
            Debug.LogError("Difficulty Brackets should be equal with Difficulty Color Count!\nDifficulty Brackets = " + difficultyBrackets.Length +
                          "\nDifficulty Color Count = " + colorBase.GetTotalDifficulty() + "\nFix Difficulty Brackets Size in " + gameObject.name +
                          " or Add/Deduct Difficulty Colors in " + colorBase.ToString() + " class");
        #endregion
        //set grid size taken from editor
        Constants.RowCount = gridSize.X;
        Constants.ColumnCount = gridSize.Y;
        //set default block color taken from editor
        ColorBase.defaultColor = defaultBlockColor;

        newBlockImages = newBlocksPanel.GetComponentsInChildren<Image>();
        hintBlockImages = hintBlocksPanel.GetComponentsInChildren<Image>();
        //disable all hintBlockImages when game starts
        foreach (Image img in hintBlockImages)
        {
            img.gameObject.SetActive(false);
        }
        gameState = GameState.Idle;

        GenerateBoard();

        GridLayoutGroup gamePanelGLG = gamePanel.GetComponent<GridLayoutGroup>();
        gamePanelGLG.constraintCount = Constants.RowCount; // if grid size is changed, adjust constrain count accordingly

        diamondBankText.text = diamondBank.ToString();
        #region safecheck
        if (gamePanelGLG.spacing.x != defaultSpacingX || gamePanelGLG.spacing.y != defaultSpacingY)
        {
            Animator anim = gamePanelGLG.GetComponent<Animator>();
            Debug.LogError("Please update Animator component of [" + anim.runtimeAnimatorController.name + "]'s Board Reset Animation's X and Y spacing values, and [defaultSpacingX] and [defaultSpacingY] values of [" + this.name + " Class]");
        }
        if (gamePanelGLG.cellSize.x != defaultCellSizeX || gamePanelGLG.cellSize.y != defaultCellSizeY)
        {
            Animator anim = gamePanelGLG.GetComponent<Animator>();
            Debug.LogError("Please update [" + anim.runtimeAnimatorController.name + " Animator]'s Board Game Over Animation's X and Y cell size values, and [defaultCellSizeX] and [defaultCellSizeY] values of [" + this.name + " Class]");
        }
        #endregion
    }

    void Update()
    {
        //Score Text Animation
        if (isScoreIncreased)
        {
            score = Utilities.IntLerp(score, updatedScore, 0.03f);
            scoreText.text = score.ToString();
            if (score == updatedScore)
                isScoreIncreased = false;
        }
    }
    #endregion

    #region Save & Load Operations 
    public void FillBlocksArray(BlocksArray loadedBlocks, List<BlockInfo> currentBlockInfos)
    {
        if (loadedBlocks != null)
            blocks = loadedBlocks;
        if (currentBlockInfos != null)
            CreateNewBlocksFromSave(currentBlockInfos);
    }

    public List<BlockInfo> GetBlocks(BlockCreationType type)
    {
        List<BlockInfo> blocks = new List<BlockInfo>();
        switch (type)
        {
            case BlockCreationType.Actual:
                blocks = newBlocks;
                break;
            case BlockCreationType.Hint:
                blocks = hintBlocks;
                break;
            default:
                break;
        }
        return blocks;
    }
    private void CreateNewBlocksFromSave(List<BlockInfo> blockInfos)
    {
        newBlocks = blockInfos;
        newBlockImages = ProcessBlocks(newBlockImages, newBlocks);
    }
    public void CreateHintBlocksFromSave(List<BlockInfo> blockInfos)
    {
        hintBlocks = blockInfos;
        hintBlockImages = ProcessBlocks(hintBlockImages, hintBlocks);
    }

    public BlocksArray GetBlocksArray() { return blocks; }

    public GameVariables GetGameVariables()
    {
        bool isHammerUsed = gameState == GameState.HammerPowerUp ? true : false;
        return new GameVariables(updatedScore, highScore, diamondBank, matchCount, difficultyCounter, currentDifficultyBracket, (int)gameState, isHammerUsed, isHintUsed);
    }

    public void SetGameVariables(GameVariables gameVariables)
    {
        SetScore(gameVariables.score, false);
        UpdateHighScore(gameVariables.highScore);
        UpdateDiamondBank(gameVariables.diamondBank);
        UpdateMatchCount(gameVariables.matchCount);
        difficultyCounter = gameVariables.difficultyCounter;
        currentDifficultyBracket = gameVariables.currentDifficultyBracket;
        gameState = (GameState)gameVariables.gameStateIndex;
        if (gameVariables.isHammerUsed)
            ProcessHammerPowerUp(true);

        isHintUsed = gameVariables.isHintUsed;
        colorBase.IncreaseDifficulty(currentDifficultyBracket);
        if (gameState == GameState.GameOver)
        {
            gameOverEvent.Invoke();
        }

    }
    #endregion

    #region Grid Management 
    void GenerateBoard()
    {
        blocks = new BlocksArray();
        //Instantiate block prefabs and assign them to blocks 2D array.
        for (int i = 0; i < Constants.ColumnCount; i++)
        {
            for (int j = 0; j < Constants.RowCount; j++)
            {
                GameObject tempBlock = Instantiate(blockPrefab, Vector3.zero, Quaternion.identity) as GameObject;
                tempBlock.transform.SetParent(gamePanel);
                tempBlock.transform.localScale = Vector3.one; // Instantiated objects has different scale value than 1 somehow?
                blocks[j, i] = tempBlock;

                tempBlock.GetComponent<Block>().FillInfo(j, i, ColorBase.defaultColor); //Set object's row and column
            }
        }
        #region safecheck
        if (blockPrefab.GetComponent<Image>().color != Color.white)
            Debug.LogError("Please set [" + blockPrefab.name + " Prefab]s Color attribute back to white. You can change color of blocks in [" + gameObject.name + " Game Object]'s [Default Block Color] property in Editor.");
        #endregion
        CreateBlocks(BlockCreationType.Actual);
    }
    
    public void ResetBoard() // used from GameOverPanel(Game object in hierarchy)
    {
        if (gameState == GameState.HammerPowerUp)
            ProcessHammerPowerUp(false);
        if (isHintUsed)
            ClearHintBlocks();
        gameState = GameState.Idle;
        for (int i = 0; i < Constants.ColumnCount; i++)
        {
            for (int j = 0; j < Constants.RowCount; j++)
            {
                blocks[j, i].GetComponent<Block>().Clear();
                blocks[j, i].GetComponent<Block>().info.Clear();
            }
        }
        UpdateHighScore();
        SetScore(0, false); // save if highscore and set score to 0
        ResetDifficulty(); // resets difficulty
        CreateBlocks(BlockCreationType.Actual); // create new blocks
        Animator boardAnimator = gamePanel.GetComponent<Animator>();
        boardAnimator.SetTrigger("reset");
    }


    #endregion

    #region Block Creation & Destruction

    public enum BlockCreationType
    { Actual, Hint }
  
    void CreateBlocks(BlockCreationType type)
    {
        List<BlockInfo> blocksInfo = new List<BlockInfo>();
        Image[] blockImages = new Image[0];
        switch (type) //select which blocks to process
        {
            case BlockCreationType.Actual:
                blocksInfo = newBlocks;
                blockImages = newBlockImages;
                break;
            case BlockCreationType.Hint:
                blocksInfo = hintBlocks;
                blockImages = hintBlockImages;
                break;
            default:
                break;
        }
        BlocksCreator blocksCreator = new BlocksCreator();
        if (isHintUsed) //transfer hintblocks to new blocks
        {
            blocksInfo = new List<BlockInfo>(hintBlocks); // create it as new list so we don't reference the hintBlocks and can reset it
            ClearHintBlocks(); //we don't need hint blocks lists anymore so clear it
        }
        else // create random new blocks  here
        {
            blocksInfo.Clear();
            blocksInfo = blocksCreator.GetRandomBlocks(); // bring new random blocks
            colorBase.FillColorInfo(blocksInfo); // color newly introduced blocks randomly
        }
        blockImages = ProcessBlocks(blockImages, blocksInfo);
        switch (type) // push processed blocks back to global holder(whichever it is)
        {
            case BlockCreationType.Actual:
                newBlocks = blocksInfo;
                newBlockImages = blockImages;
                break;
            case BlockCreationType.Hint:
                hintBlocks = blocksInfo;
                hintBlockImages = blockImages;
                break;
            default:
                break;
        }

    }
    private void ClearHintBlocks()
    {
        hintBlocks.Clear();
        hintBlockImages = ProcessBlocks(hintBlockImages, hintBlocks);
        isHintUsed = false;
    }

    Image[] ProcessBlocks(Image[] blockImages, List<BlockInfo> blocksInfo)
    {
        for (int i = 0; i < blockImages.Length; i++)
            blockImages[i].gameObject.SetActive(false);

        for (int i = 0; i < blocksInfo.Count; i++)
        {
            blockImages[i].gameObject.SetActive(true);
            blockImages[i].color = blocksInfo[i].BlockColor.GetColor();
        }
       return blockImages;
    }
  
    private void ExplodeBlocks()
    {
        List<List<Block>> matchedBlocksList = blocks.GetMatchedBlockLists(); // retrieve matched block list

        int scoreMultiplier = matchedBlocksList.Count;
        int score = 0;
        int totalScore = 0;
        //List<Block> allCollectedAdjacentBlocks = new List<Block>();
        //List<int> scoresOfCollectedAdjacentBlocks = new List<int>();
        foreach (List<Block> adjacentBlocksWithSameColor in matchedBlocksList)
        {
            score = adjacentBlocksWithSameColor.Count * scoreMultiplier; // score per block
            totalScore += adjacentBlocksWithSameColor.Count * score; //score for total blocks added
            foreach (Block adjacentBlock in adjacentBlocksWithSameColor)
            {
                StartTextAnimation(adjacentBlock, score);
                adjacentBlock.Clear();
            }
        }
        //StartCoroutine(ExplosionSequencer(allCollectedAdjacentBlocks, scoresOfCollectedAdjacentBlocks));

        int matchCountAddition = matchedBlocksList.Count * matchedBlocksList.Count; // if player does combo give more matchCount addition
        IncreaseMatchCount(matchCountAddition);
        // if player does combo, show it
        if (matchedBlocksList.Count > 1)
        {
            comboText.text = "x" + matchedBlocksList.Count.ToString();
            comboText.GetComponent<Animator>().SetTrigger("combo");
        }

        ControlDifficulty();
        AddToScore(totalScore); // add total collected score in this turn and add it to our total score
        blocks.ClearMatchedBlockLists(); // we are done with the list and we can clear it now for further turns
    }
  
    #endregion

    #region Touch & Drag & Release detection of 
    public void StartSelection(Block selectedBlock)
    {
        //Checks if block is white
        if (blocks.CheckIfBlockAvailable(selectedBlock.info) && gameState == GameState.Idle)
        {
            gameState = GameState.SelectionStarted; //Start selection process
            currentBlock = selectedBlock;
            previousBlock = selectedBlock;

            UpdateSelectedBlock(selectedBlock);
            UpdateNewBlocksAnimation(newBlockImages[0].gameObject, true);
        }
        //if game state is hammer power up and the touched block is a colored one.
        else if (!blocks.CheckIfBlockAvailable(selectedBlock.info) && gameState == GameState.HammerPowerUp)
            RemoveBlock(selectedBlock);
    }

    private void UpdateSelectedBlock(Block selectedBlock)
    {
        selectedBlock.SetColor(newBlocks[selectionCount].BlockColor.GetColor());//Set color of selected block to queued new block
        blocksPlaced.Add(selectedBlock); // add currently selected block to the list of placed blocks(we will need it later)
        selectionCount++; // increase the selection count so next time the queued block will be placed
    }

   public void SetSelectedBlock(Block selectedBlock)
    {
        //if player intends to add new block on queue to grid which is adjacent to previous one 
        if (blocks.IsBlocksAdjacentAndAvailable(currentBlock.info, selectedBlock.info))
        {
            // if number of selected blocks is not bigger than newly created blocks
            if (selectionCount < newBlocks.Count)
            {
                // Purpose of this to track current and previous blocks 
                // In order to revert our selection
                previousBlock = currentBlock; // Set current block as previous block as it will become previous block after this selection
                currentBlock = selectedBlock; // Set newly selected block as current block
                UpdateNewBlocksAnimation(newBlockImages[selectionCount].gameObject, true);
                UpdateSelectedBlock(selectedBlock); // hover to method to see description

            }
        }
        // if player intends to revert his placed block by going backwards in placed blocks.
        else if (blocksPlaced.Contains(selectedBlock) && selectedBlock == previousBlock)
        {
            //player's finger is on previous block so remove the last placed block
            int selectedBlockIndex = blocksPlaced.IndexOf(selectedBlock);//get index of previous block

            //this check is required to prevent error when there is only one block exists and therefore index+1 is out of range
            if (selectedBlockIndex + 1 < blocksPlaced.Count) // if the last placed block's index is smaller than number of blocksPlaced
            {
                //remove last placed block and update selection count accordingly
                blocksPlaced[selectedBlockIndex + 1].Clear();
                blocksPlaced.RemoveAt(selectedBlockIndex + 1);
                UpdateNewBlocksAnimation(newBlockImages[selectedBlockIndex + 1].gameObject, false);
                selectionCount--;
            }
            currentBlock = selectedBlock; // previously selected block is now currently selected block
            if (selectedBlockIndex > 0) //this check is required to prevent error when there is only one block exists and therefore index-1 is out of range
                previousBlock = blocksPlaced[selectedBlockIndex - 1]; // update previousBlock

        }
    }

   
    public void TerminateSelection()
    {
        if (selectionCount > 0)// check if any selection made //
        {
            if (blocksPlaced.Count < newBlocks.Count) // if player didn't place all of given blocks yet
            {
                //remove all blocks that are temporarily placed to grid
                for (int i = 0; i < blocksPlaced.Count; i++)
                {
                    blocksPlaced[i].Clear();
                    UpdateNewBlocksAnimation(newBlockImages[i].gameObject, false);
                }
                
                gameState = GameState.Idle; //selection is over so set it back to idle
            }
            else // player placed all of given blocks to grid
                AppendSelection();
            //regardless of the conditions above, the player is released fingers
            //and we need to reset the selectionCount and blocksPlaced list
            selectionCount = 0;
            blocksPlaced.Clear();
        }
    }
   
    private void AppendSelection()
    {

        if (blocks.CheckAdjacentBlocks(blocksPlaced)) // send placed block list for adjacency check and if there are any 3 or more adjacent block found
            ExplodeBlocks(); // explode blocks if there are any

        CreateBlocks(BlockCreationType.Actual); // create new blocks to continue the game

        if (blocks.CheckEmptyBlocks(newBlocks.Count)) // Use this method after creating blocks!
            gameState = GameState.Idle;
        else // game over here
        {
            gameState = GameState.GameOver;
            gameOverEvent.Invoke();

            Animator boardAnimator = gamePanel.GetComponent<Animator>();
            boardAnimator.SetTrigger("gameOver");
        }
    }
    public void ClearSelection()
    {
        currentBlock = null;
        selectionCount = 0;
    }
    #endregion

    #region Difficulty 
    private void ControlDifficulty()
    {
        if (currentDifficultyBracket != difficultyBrackets.Length) // if we didn't reach our last bracket
        {
            difficultyCounter++; // increase difficulty counter
            if (difficultyCounter == difficultyBrackets[currentDifficultyBracket]) // if we reach our current bracket
            {
                currentDifficultyBracket++; //increase our current bracket
                difficultyCounter = 0; //reset our counter
                colorBase.IncreaseDifficulty();// add another color in our color pool
                newColorAnim.GetComponent<Image>().color = colorBase.GetLatestColor();
                newColorAnim.SetTrigger("newColor");
                newColorTextAnim.SetTrigger("shadow");
            }
        }
    }
   
    private void ResetDifficulty()
    {
        difficultyCounter = 0;
        currentDifficultyBracket = 0;
        colorBase.ResetToDefault();
    }
    #endregion

    #region PowerUp Section
    public void HammerPowerUp()// called from Hammer Button
    {
        //only enable hammer power up button to be clicked if there are any colored blocks exist in grid
        if (gameState == GameState.Idle && AreDiamondsSufficient())
        {
            ProcessHammerPowerUp(true);
        }
        else if (gameState == GameState.HammerPowerUp)
        {
            gameState = GameState.Idle;
            ProcessHammerPowerUp(false);
        }
    }

    public void HintPowerUp()// called from Hint Button
    {
        if ((gameState != GameState.GameOver) && !isHintUsed && AreDiamondsSufficient())
        {
            CreateBlocks(BlockCreationType.Hint);
            isHintUsed = true;
            DeductDiamonds();
        }
    }

    public void SkipPowerUp()// called from Skip Button
    {
        if (gameState != GameState.GameOver && AreDiamondsSufficient())
        {
            CreateBlocks(BlockCreationType.Actual);
            DeductDiamonds();
        }
    }

   
    private void ProcessHammerPowerUp(bool isActivate) // this check is needed for hammer powerup
    {
        bool isAnyColoredBlockExist = false;
        for (int i = 0; i < Constants.ColumnCount; i++)
        {
            for (int j = 0; j < Constants.RowCount; j++)
            {
                if (blocks[j, i].GetComponent<Block>().info.BlockColor.GetColor() != ColorBase.defaultColor) // if color of block is not white
                {
                    blocks[j, i].GetComponent<Block>().hammerImage.SetActive(isActivate);
                    isAnyColoredBlockExist = true;
                }
            }
        }
        if (isAnyColoredBlockExist && isActivate)
        {
            gameState = GameState.HammerPowerUp;
        }
    }

    private void RemoveBlock(Block selectedBlock)
    {
        DeductDiamonds();
        ProcessHammerPowerUp(false);
        selectedBlock.Clear(); // remove it's colors
        gameState = GameState.Idle;
    }
    #endregion

    #region Currency Section 
    private void IncreaseMatchCount(int increaseAmount)
    {
        matchCount += increaseAmount;
        if (matchCount >= matchesNeededToGiveDiamond)
        {
            matchCount -= matchesNeededToGiveDiamond;
            AddDiamonds();
        }
        UpdateMatchCount();
    }
    
    private void AddDiamonds()
    {
        UpdateDiamondBank(diamondBank + diamondReward);

    }
  
    private bool AreDiamondsSufficient()
    {
        bool rBool = false;
        if (diamondBank >= powerUpDiamondPrice)
        {
            rBool = true;
        }
        return rBool;
    }
   
    private void DeductDiamonds()
    {
        diamondBank -= powerUpDiamondPrice;
        DiamondsDeducted();
        UpdateDiamondBank();
    }

    void DiamondsDeducted()
    {
        deductionPanel.gameObject.SetActive(false);
        deductionPanel.gameObject.SetActive(true); //animation starts 
    }
   
    private void UpdateDiamondBank(int _diamondBank = -1)
    {
        if (_diamondBank != -1)
            diamondBank = _diamondBank;
        diamondBankText.text = diamondBank.ToString();
    }
   
    private void UpdateMatchCount(int _matchCount = -1)
    {
        if (_matchCount != -1)
            matchCount = _matchCount;
        if (matchCount > 0)
            matchCountBar.fillAmount = 1 / (float)((float)matchesNeededToGiveDiamond / (float)matchCount);
        else matchCountBar.fillAmount = 0;
    }
   
    public void GetDiamondsForFree() // Used by AddDiamond button
    {
        UpdateDiamondBank(diamondBank + 1000);
    }
    #endregion

    #region Score 
    private void SetScore(int newScore, bool isAnimated)
    {
        if (isAnimated)
        {
            updatedScore = newScore;
            isScoreIncreased = true;
        }
        else
        {
            score = newScore;
            updatedScore = newScore;
            scoreText.text = newScore.ToString();
        }
    }
   
    private void AddToScore(int addition)
    {
        SetScore(updatedScore + addition, true);

    }
   
    private void UpdateHighScore(int optionalScore = -1)
    {
        if (optionalScore == -1)
        {
            if (updatedScore > highScore)
            {
                highScore = updatedScore;

            }
        }
        else
            highScore = optionalScore;

        highScoreText.text = highScore.ToString();

    }
    #endregion

    #region Animations 
    void UpdateNewBlocksAnimation(GameObject blockGO, bool isPlaced)
    {
        Animator anim = blockGO.GetComponent<Animator>();
        if (blockGO.activeSelf)
        {
            anim.SetBool("shrink", isPlaced);
        }
    }

   
    private void StartTextAnimation(Block block, int points)
    {
        Animator blockAnim = block.GetComponentInChildren<Animator>();
        Text blockText = block.GetComponentInChildren<Text>();
        blockText.color = block.info.BlockColor.GetColor();
        blockText.text = "+" + points.ToString();
        blockAnim.SetTrigger("startFloat");
    }
    #endregion


}
