using UnityEngine;
using System.Collections;


[System.Serializable]
public class GameVariables
{
    public int score;
    public int highScore;
    public int diamondBank;
    public int matchCount;
    public int difficultyCounter;
    public int currentDifficultyBracket;
    public int gameStateIndex; 
    public bool isHammerUsed; 
    public bool isHintUsed;

    public GameVariables(int _score, int _highScore, int _diamondBank, int _matchCount, int _difficultyCounter, int _currentDifficultyBracket, int _gameStateIndex, bool _isHammerUsed, bool _isHintUsed)
    {
        score = _score;
        highScore = _highScore;
        diamondBank = _diamondBank;
        matchCount = _matchCount;
        difficultyCounter = _difficultyCounter;
        currentDifficultyBracket = _currentDifficultyBracket;
        gameStateIndex = _gameStateIndex;
        isHammerUsed = _isHammerUsed;
        isHintUsed = _isHintUsed;

    }
}
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           