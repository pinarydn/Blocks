using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlocksArray {

    private GameObject[,] blocks = new GameObject[Constants.RowCount, Constants.ColumnCount];

    public GameObject this[int row, int column]
    {
        get
        {
            try
            {
                return blocks[row, column];
            }
            catch (System.Exception exc)
            {
                throw exc;
            }

        }
        set
        {
            blocks[row, column] = value;
        }
    }
    public bool IsBlocksAdjacentAndAvailable(BlockInfo currentBlockInfo, BlockInfo newBlockInfo)
    {
        bool rBool = false;
        bool isNewBlockAdjacent = CheckAdjacentBlocks(currentBlockInfo, newBlockInfo);
        bool isNewBlockAvailable = CheckIfBlockAvailable(newBlockInfo);
        if (isNewBlockAdjacent && isNewBlockAvailable)
        {
            rBool = true;
        }
        return rBool;
    }

    public bool CheckIfBlockAvailable(BlockInfo newBlockInfo)
    {
        bool rBool = false;
        if (newBlockInfo.BlockColor.GetColor() == ColorBase.defaultColor) // if the new block is not empty
        {
            rBool = true;
        }
        return rBool;
    }

    private bool CheckAdjacentBlocks(BlockInfo blockInfo1, BlockInfo blockInfo2)
    {
        bool rBool = false;
        List<BlockInfo> adjacentBlocks = new List<BlockInfo>();
        //Right block check
        if (blockInfo1.Row != Constants.RowCount - 1) // If it's not at the rightmost of the board
            adjacentBlocks.Add(RetrieveInfo(blockInfo1.Row + 1, blockInfo1.Column)); // Add block at the right of the block1
                                                                                     //Left block check
        if (blockInfo1.Row != 0) //If it's not at the leftmost of the board
            adjacentBlocks.Add(RetrieveInfo(blockInfo1.Row - 1, blockInfo1.Column)); // Add block at the left of the block1
                                                                                     //Bottom block check
        if (blockInfo1.Column != Constants.ColumnCount - 1)// If it's not at the bottom of the board
            adjacentBlocks.Add(RetrieveInfo(blockInfo1.Row, blockInfo1.Column + 1)); // Add block under block1
                                                                                     //Top block check
        if (blockInfo1.Column != 0)// If it's not at the top of the board
            adjacentBlocks.Add(RetrieveInfo(blockInfo1.Row, blockInfo1.Column - 1)); // Add block above block1

        foreach (var block in adjacentBlocks)
        {
            if (block == blockInfo2) // if any of adjacent blocks is the block that has been selected by player.
            {
                rBool = true;
            }
        }
        return rBool;
    }

    public BlockInfo RetrieveInfo(int _row, int _column)
    {
        BlockInfo block = null;
        if (_row <= Constants.RowCount - 1 && _row >= 0 && _column <= Constants.ColumnCount - 1 && _column >= 0) //safecheck
        {
            block = blocks[_row, _column].GetComponent<Block>().info;
        }
        return block;
    }

    private List<BlockInfo> adjacentBlocksWithSameColor = new List<BlockInfo>(); //Temporary list to hold all adjacent blocks with same color

    private List<List<Block>> matchedBlockLists = new List<List<Block>>();
    private int adjacencyIndex = 0;

    private bool IsBlockListHasBlocks()
    { return matchedBlockLists.Count > 0 ? true : false; }

    public List<List<Block>> GetMatchedBlockLists()
    { return matchedBlockLists; }

    public void ClearMatchedBlockLists()
    { matchedBlockLists.Clear(); }

    public bool CheckAdjacentBlocks(List<Block> blocksPlaced)
    {
        for (int i = 0; i < blocksPlaced.Count; i++)
            Check3Match(blocksPlaced[i].info);                                             
                                              
        return IsBlockListHasBlocks();                                                                                          
    }

    private void Check3Match(BlockInfo blockInfo, bool isWhiteBlockCheck = false, int adjacentBlockCount = 3) //game rule is to find 3 or more blocks
    {
        Color blockColor = blockInfo.BlockColor.GetColor();
        //only add this block if it's not checked
        if (!blockInfo.IsChecked)
            adjacentBlocksWithSameColor.Add(blockInfo);

        //then control adjacent blocks(left, right, up and down) and add to list if there are any same colors.
        if (blockInfo.Row != Constants.RowCount - 1)
        {
            BlockInfo checkBlock = RetrieveInfo(blockInfo.Row + 1, blockInfo.Column);
            if (blockColor == checkBlock.BlockColor.GetColor() && !checkBlock.IsChecked) //adjacent block should be same color as ours and should not be checked before.
                adjacentBlocksWithSameColor.Add(checkBlock); // add it to our array
        }
        if (blockInfo.Row != 0)
        {
            BlockInfo checkBlock = RetrieveInfo(blockInfo.Row - 1, blockInfo.Column);
            if (blockColor == checkBlock.BlockColor.GetColor() && !checkBlock.IsChecked)
                adjacentBlocksWithSameColor.Add(checkBlock);
        }
        if (blockInfo.Column != Constants.ColumnCount - 1)
        {
            BlockInfo checkBlock = RetrieveInfo(blockInfo.Row, blockInfo.Column + 1);
            if (blockColor == checkBlock.BlockColor.GetColor() && !checkBlock.IsChecked)
                adjacentBlocksWithSameColor.Add(checkBlock);
        }
        if (blockInfo.Column != 0)
        {
            BlockInfo checkBlock = RetrieveInfo(blockInfo.Row, blockInfo.Column - 1);
            if (blockColor == checkBlock.BlockColor.GetColor() && !checkBlock.IsChecked)
                adjacentBlocksWithSameColor.Add(checkBlock);
        }

        if (adjacentBlocksWithSameColor.Count > 1 || (isWhiteBlockCheck && adjacentBlocksWithSameColor.Count > 0)) // if any adjacent same color blocks found
        {                                                                                                      // or code is looking for empty blocks and even 1 block should be sended
                                                                                                               //set all added blocks to checked to avoid adding them again
            foreach (BlockInfo info in adjacentBlocksWithSameColor)
                info.IsChecked = true;

            adjacencyIndex++; // block is checked so increase index

            //if the code is searching for colored blocks, it needs to find every single colored block
            //that are adjacent to eachother and destroy them if they are more than 3.
            if (adjacencyIndex < adjacentBlocksWithSameColor.Count) //continue only if there are more blocks to check
                Check3Match(adjacentBlocksWithSameColor[adjacencyIndex], isWhiteBlockCheck, adjacentBlockCount); //recursive!

            else if (adjacentBlocksWithSameColor.Count >= adjacentBlockCount) // if there are no more blocks to check, check if adjacent blocks are more than given block Count
            {
                List<Block> adjacentBlockComponents = new List<Block>();
                foreach (BlockInfo info in adjacentBlocksWithSameColor) //Transfer them to Block component
                    adjacentBlockComponents.Add(blocks[info.Row, info.Column].GetComponent<Block>());

                // transfer them to main list for BoardManager to destroy them all.
                matchedBlockLists.Add(adjacentBlockComponents);
            }
            else // if there are no more blocks to check and collected adjacent blocks are less than 3
            {
                if (!isWhiteBlockCheck) // only clear checked flags if it's not empty block check because we will clear flags when all checks ended for optimization
                    ClearIsCheckedFlags(adjacentBlocksWithSameColor); // clears isChecked flags
            }
        }
        else //if any adjacent same color blocks not found
        {
            if (!isWhiteBlockCheck) // only clear checked flags if it's not white empty check because we will clear flags when all checks ended for optimization
                ClearIsCheckedFlags(adjacentBlocksWithSameColor); // clears isChecked flags
        }
        PrepareForNextIteration(); // this clears all temporary lists and index
    }

    private void ClearIsCheckedFlags(List<BlockInfo> collectedBlocks)
    {
        foreach (BlockInfo block in collectedBlocks)
            block.IsChecked = false;
    }

    private void PrepareForNextIteration()
    {
        adjacentBlocksWithSameColor.Clear();
        adjacencyIndex = 0;
    }
    public bool CheckEmptyBlocks(int newBlocksCount)
    {
        List<BlockInfo> emptyBlocks = new List<BlockInfo>();
        bool rBool = false;
        //get all white blocks in board
        for (int i = 0; i < Constants.ColumnCount; i++)
        {
            for (int j = 0; j < Constants.RowCount; j++)
            {
                if (blocks[j, i].GetComponent<Block>().info.BlockColor.GetColor() == ColorBase.defaultColor) // if the block is white
                {
                    emptyBlocks.Add(blocks[j, i].GetComponent<Block>().info);
                }
            }
        }
        // now we need to seperate them according to their adjacency
        // we can use our slightly modified version of recursive function Check3Match and matchedBlockLists to store them seperately
        for (int i = 0; i < emptyBlocks.Count; i++)
        {
            if (!emptyBlocks[i].IsChecked) // skip already checked blocks
            {
                Check3Match(emptyBlocks[i], true, newBlocksCount); // this is a white block check so flag it and give custom block count to check accordingly and avoid overstoring!
                                                                   //Check3Match only adds to matchedBlockLists if there are more equal or more adjacent blocks than newBlocksCount
            }
        }
     
        if (matchedBlockLists.Count != 0) //if we have some elements to test
        {
            foreach (List<Block> adjacentBlockList in matchedBlockLists)
            {
                int additionalCount = newBlocksCount >= 4 ? 2 : 0; // if we have 4 or more new blocks to be placed, we need to add 2...
               
                if (adjacentBlockList.Count >= newBlocksCount + additionalCount) // if there are enough empty blocks for user to place new blocks
                {
                    rBool = true; //new blocks can be placed to grid
                    break; // this leaves from foreach loop
                }
            }
        }

        if (!rBool && newBlocksCount >= 4) //only do this if there are not empty blocks for user to place new blocks detected and our newBlocksCount is bigger than 4
        {
            if (matchedBlockLists.Count != 0) // if we have some elements to test
            {
                //Send every block info in every block list to check their patterns
                foreach (List<Block> adjacentBlockList in matchedBlockLists)
                {
                    if (rBool)
                        break;
                    foreach (Block adjacentBlock in adjacentBlockList)
                    {
                        AdvancedEmptyBlockCheck(adjacentBlock.GetComponent<Block>().info, newBlocksCount);
                        if (emptySpaceAvailable) //this is modified within AdvancedEmptyBlockCheck
                        {
                            rBool = true;
                            emptySpaceAvailable = false;
                            break;
                        }
                    }
                }
            }
        }
        //we got what we want so we can reset our variables
        ClearIsCheckedFlags(emptyBlocks);
        ClearMatchedBlockLists();
        PrepareForNextIteration();
        return rBool;
    }


    int nothingAddedCount = 0; // if nothing is added to our adjacentBlocksWithSameColor list for 2 times we terminate recursive method.
    bool emptySpaceAvailable = false; // this bool needs to be global in order for CheckEmptyBlocks to see if this method found any pattern that 4 blocks can be placed into.
    List<BlockInfo> removedBlocks = new List<BlockInfo>(); 
    private void AdvancedEmptyBlockCheck(BlockInfo blockInfo, int newBlocksCount)
    {
        Color blockColor = blockInfo.BlockColor.GetColor();
        int consequentCount = adjacentBlocksWithSameColor.Count; //to ensure we added a new block to our list
        if (!adjacentBlocksWithSameColor.Contains(blockInfo)) // do not add already added blocks
            adjacentBlocksWithSameColor.Add(blockInfo);

        bool isSingleBlockAdded = false; // we want to add single block at a time per run(this way we can ensure code will look for a single path)
        bool isAdjacentToDeadEndBlock = false; // if the block we are looking is adjacent to a "dead end" block
                                               //control adjacent blocks and add to list if there are any same colors.
                                               //whole if block is for looking left, right, up and down of given blockInfo and adding a single block that is same color and
                                               //is not flagged as "dead end" from our previous runs
        if (blockInfo.Row != Constants.RowCount - 1 && !isSingleBlockAdded)
        {
            BlockInfo checkBlock = RetrieveInfo(blockInfo.Row + 1, blockInfo.Column);
            if (blockColor == checkBlock.BlockColor.GetColor() && !adjacentBlocksWithSameColor.Contains(checkBlock) && !checkBlock.IsDeadEnd) //only add it if its same color && it doesn't included in list already && it is not a deadend
            {
                adjacentBlocksWithSameColor.Add(checkBlock); // add it to our array
                isSingleBlockAdded = true;
            }
            else if (checkBlock.IsDeadEnd) isAdjacentToDeadEndBlock = true;
        }
        if (blockInfo.Row != 0 && !isSingleBlockAdded)
        {
            BlockInfo checkBlock = RetrieveInfo(blockInfo.Row - 1, blockInfo.Column);
            if (blockColor == checkBlock.BlockColor.GetColor() && !adjacentBlocksWithSameColor.Contains(checkBlock) && !checkBlock.IsDeadEnd)
            {
                adjacentBlocksWithSameColor.Add(checkBlock);
                isSingleBlockAdded = true;
            }
            else if (checkBlock.IsDeadEnd) isAdjacentToDeadEndBlock = true;
        }
        if (blockInfo.Column != Constants.ColumnCount - 1 && !isSingleBlockAdded)
        {
            BlockInfo checkBlock = RetrieveInfo(blockInfo.Row, blockInfo.Column + 1);
            if (blockColor == checkBlock.BlockColor.GetColor() && !adjacentBlocksWithSameColor.Contains(checkBlock) && !checkBlock.IsDeadEnd)
            {
                adjacentBlocksWithSameColor.Add(checkBlock);
                isSingleBlockAdded = true;
            }
            else if (checkBlock.IsDeadEnd) isAdjacentToDeadEndBlock = true;
        }
        if (blockInfo.Column != 0 && !isSingleBlockAdded)
        {
            BlockInfo checkBlock = RetrieveInfo(blockInfo.Row, blockInfo.Column - 1);
            if (blockColor == checkBlock.BlockColor.GetColor() && !adjacentBlocksWithSameColor.Contains(checkBlock) && !checkBlock.IsDeadEnd)
            {
                adjacentBlocksWithSameColor.Add(checkBlock);
                isSingleBlockAdded = true;
            }
            else if (checkBlock.IsDeadEnd) isAdjacentToDeadEndBlock = true;
        }

        if (consequentCount != adjacentBlocksWithSameColor.Count) // if any blocks added to list
            nothingAddedCount = 0; // reset our counter

        adjacencyIndex++; // block is checked so increase index

        if (adjacentBlocksWithSameColor.Count >= newBlocksCount) // check if adjacent blocks are more than given block Count
        {
            emptySpaceAvailable = true; // we won't continue recursive method because we got what we need
            PrepareForNextIteration(); // this clears all temporary lists and index
        }
        else if (adjacencyIndex < adjacentBlocksWithSameColor.Count) //continue only if there are more blocks to check 
            AdvancedEmptyBlockCheck(adjacentBlocksWithSameColor[adjacencyIndex], newBlocksCount); //recursive!
        else // if there are no more blocks to check and collected adjacent blocks are less than newBlocksCount
        {
            if (nothingAddedCount < 2)
            {
                nothingAddedCount++;
                //This place of code indicates that we are at a "dead end" so we need to flag this block as dead end and
                // remove it from our list because we need to search for another path.
                if (isAdjacentToDeadEndBlock)
                {
                    //We flag our "dead end" block and transfer it to removedBlocks list so it won't bother us anymore for this run
                    adjacentBlocksWithSameColor[adjacentBlocksWithSameColor.Count - 1].IsDeadEnd = true;
                    removedBlocks.Add(adjacentBlocksWithSameColor[adjacentBlocksWithSameColor.Count - 1]);
                    adjacentBlocksWithSameColor.RemoveAt(adjacentBlocksWithSameColor.Count - 1);
                    adjacencyIndex = adjacentBlocksWithSameColor.Count - 1; // update adjacency index to check different blocks
                }
                AdvancedEmptyBlockCheck(adjacentBlocksWithSameColor[adjacentBlocksWithSameColor.Count - 1], newBlocksCount);
            }
            else // if nothing is added for 2 turns(referenced empty block's every possible path is calculated)
            {
                //false every blocks' isDeadEnd so we can start a clean look from a different reference point(a different block's path)
                foreach (BlockInfo block in adjacentBlocksWithSameColor)
                    block.IsDeadEnd = false;
                foreach (BlockInfo block in removedBlocks)
                    block.IsDeadEnd = false;
                removedBlocks.Clear();
                PrepareForNextIteration();
                nothingAddedCount = 0;
            }
        }
        
    }

}
