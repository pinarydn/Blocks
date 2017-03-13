using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    public GameObject gameOverPanel;
    public BoardManager boardManager;
    public Transform gameBoardCanvas;
    public Animator hammerButtonAnim;
    public Animator hintButtonAnim;
    public Animator skipButtonAnim;

    public void GameOver()
    {
        gameOverPanel.SetActive(true);
    }

    public void RestartGame()
    {
        gameOverPanel.SetActive(false);
    }

    public void HammerPressed()
    {
        hammerButtonAnim.SetTrigger("isPressed");
    }
    public void HintPressed()
    {
        hintButtonAnim.SetTrigger("isPressed");
    }
    public void SkipPressed()
    {
        skipButtonAnim.SetTrigger("isPressed");
    }
}
