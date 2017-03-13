using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Assigning colors to newly created squares and adjusting difficulty (increasing color palette)
public class ColorBase : MonoBehaviour
{

    private List<Color> currentColors = new List<Color>();
    private List<Color> difficultyColors = new List<Color>();
    public static Color defaultColor = Color.white; //default is white unless it is modified in BoardManager
    private bool isDifficultyIncreased;
    public ColorBase()
    {
        ResetToDefault();
    }

    // Default Color state
    public void ResetToDefault()
    {

        currentColors.Clear();
        currentColors.Add(ConvertTo1(164, 243, 11)); // green
        currentColors.Add(ConvertTo1(62, 199, 191)); // blue
        currentColors.Add(ConvertTo1(104, 42, 173)); // purple

        difficultyColors.Clear();
        difficultyColors.Add(ConvertTo1(201, 6, 83)); // red-ish
        difficultyColors.Add(ConvertTo1(251, 227, 22)); // yellow
        difficultyColors.Add(ConvertTo1(242, 93, 12)); // orange
        difficultyColors.Add(ConvertTo1(183, 37, 199)); // pink
        difficultyColors.Add(ConvertTo1(41, 41, 41)); // black-ish
    }

    // Increased Difficulty of the game(adds another color in our color pool
    public void IncreaseDifficulty()
    {
        if (difficultyColors.Count > 0)
        {
            currentColors.Add(difficultyColors[0]);
            difficultyColors.RemoveAt(0);
            isDifficultyIncreased = true;
        }
    }

    public void IncreaseDifficulty(int difficultyBracket)
    {
        for (int i = 0; i < difficultyBracket; i++)
            IncreaseDifficulty();
    }

    //Fills color of given list of block infos randomly, preventing from having 4 blocks with the same color. Includes the new color if difficulty is increased
    public void FillColorInfo(List<BlockInfo> BlockInfos)
    {
        for (int i = 0; i < BlockInfos.Count; i++)
        {
            BlockInfos[i].BlockColor = new SerializableColor(GetRandomColor());
        }

        //if blocks are 4 or more, check if they are the same color
        if (BlockInfos.Count >= 4)
        {
            bool isAllBlocksSameColor = true;
            for (int i = 1; i < BlockInfos.Count; i++)
            {
                if (BlockInfos[0].BlockColor.GetColor() != BlockInfos[i].BlockColor.GetColor())
                {
                    isAllBlocksSameColor = false;
                    break;
                }
            }
            IncludeNewColorOnFirstTime(BlockInfos);

            PreventAllBlocksToBeSameColor(BlockInfos, isAllBlocksSameColor);
        }
    }

    private void IncludeNewColorOnFirstTime(List<BlockInfo> BlockInfos)
    {
        if (isDifficultyIncreased)
        {
            bool isNewColorAdded = false;
            for (int i = 0; i < BlockInfos.Count; i++)
            {
                if (currentColors[currentColors.Count - 1] == BlockInfos[i].BlockColor.GetColor()) 
                {
                    isNewColorAdded = true;
                    break;
                }
            }
            if (!isNewColorAdded)
                BlockInfos[Random.Range(0, BlockInfos.Count - 1)].BlockColor = new SerializableColor(currentColors[currentColors.Count - 1]);
            isDifficultyIncreased = false;
        }
    }

    private void PreventAllBlocksToBeSameColor(List<BlockInfo> BlockInfos, bool isAllBlocksSameColor)
    {
        while (isAllBlocksSameColor)
        {
            int rnd = Random.Range(0, BlockInfos.Count - 1);
            BlockInfos[rnd].BlockColor = new SerializableColor(GetRandomColor()); 
            for (int i = 0; i < BlockInfos.Count; i++)
            {
                if (BlockInfos[0].BlockColor.GetColor() != BlockInfos[i].BlockColor.GetColor())
                {
                    isAllBlocksSameColor = false;
                    break;
                }
            }
        }
    }
    public Color GetLatestColor()
    {
        return currentColors[currentColors.Count - 1];
    }

    private Color GetRandomColor()
    {
        return currentColors[Random.Range(0, currentColors.Count)];
    }
    public int GetTotalDifficulty()
    {
        return difficultyColors.Count;
    }

    private Color ConvertTo1(float r, float g, float b)
    {
        return new Color(r / 255, g / 255, b / 255);
    }
}
