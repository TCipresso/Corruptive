using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ChessPiece : MonoBehaviour
{
    /*public string currentPosition;
    protected GameBoard gameBoard;
    public static ChessPiece selectedPiece;
    public GameObject highlightPrefab;
    protected List<GameObject> moveHighlights = new List<GameObject>();

    protected virtual void Start()
    {
        gameBoard = FindObjectOfType<GameBoard>();
    }

    protected abstract IEnumerable<string> GetLegalMoves();

    protected abstract void MoveTo(string newPosition);

    public void ClearHighlights()
    {
        foreach (var highlight in moveHighlights)
        {
            Destroy(highlight);
        }
        moveHighlights.Clear();
    }

    // Add other common methods and properties...*/
}
