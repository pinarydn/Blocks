using UnityEngine;

public class EventController : MonoBehaviour
{

    private BoardManager boardManager; // Assigned in Editor

    void Awake()
    {
        boardManager = GameObject.FindGameObjectWithTag(Constants.Tag_BoardManager).GetComponent<BoardManager>();
    }

    public void OnPointerEnter()
    {
        if (BoardManager.gameState == GameState.SelectionStarted) // if the selection started during OnPointerEnter event
        {
            Block selectedBlock = GetComponent<Block>();
            boardManager.SetSelectedBlock(selectedBlock);
        }

    }
    public void OnPointerDown()
    {
        Block selectedBlock = GetComponent<Block>();
        boardManager.StartSelection(selectedBlock);
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            boardManager.TerminateSelection();
        }
    }

}