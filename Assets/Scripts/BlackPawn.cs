using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BlackPawn : MonoBehaviour
{
    public string currentPosition { get; set; }
    private static BlackPawn selectedPawn;
    public GameObject highlightPrefab;
    public bool IsAnimationPlaying { get; private set; }

    private GameBoard gameBoard;
    private List<GameObject> moveHighlights = new List<GameObject>();
    [SerializeField] private AudioClip selectWhiteSound;
    [SerializeField] private AudioClip selectBlackSound;
    private AudioSource audioSource;

    void Start()
    {
        gameBoard = FindObjectOfType<GameBoard>();
        if (gameBoard == null)
        {
            Debug.LogError("GameBoard not found");
        }
        gameBoard.piecesOnBoard[currentPosition] = this.gameObject;
    }

    void OnMouseDown()
    {
        GameLogic gameLogic = FindObjectOfType<GameLogic>();

        if ((this.CompareTag("BlackPiece") && gameLogic.currentPlayer != GameLogic.Player.Black))
        {
            return; 
        }

        
        if (SelectionManager.selectedPiece == gameObject)
        {
          
            ClearHighlights();
            SelectionManager.selectedPiece = null;
            Debug.Log(gameObject.name + " on " + currentPosition + " is deselected.");
        }
        else
        {
            
            SelectionManager.DeselectPiece();
            SelectionManager.selectedPiece = gameObject;
            HighlightLegalMoves();
            Debug.Log(gameObject.name + " on " + currentPosition + " is selected.");
        }
    }


    void Update()
    {
        if (SelectionManager.selectedPiece == gameObject && Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 100.0f))
            {
                GameObject hitObject = hit.collider.gameObject;

                if (hitObject.CompareTag("ChessSquare"))
                {
                    string targetPosition = hitObject.name;
                    if (IsLegalMove(targetPosition))
                    {
                        MoveTo(targetPosition);
                        SelectionManager.selectedPiece = null;
                    }
                }
            }
        }
    }
    public void MoveTo(string newPosition)
    {
        if (IsLegalMove(newPosition))
        {
            StartCoroutine(MovePieceAnimation(newPosition));
            currentPosition = newPosition;
            gameBoard.piecesOnBoard[currentPosition] = this.gameObject;
        }
        else
        {
            Debug.Log("Illegal move for " + gameObject.name);
        }
    }


    private bool IsLegalMove(string newPosition)
    {
        int currentRow = int.Parse(currentPosition.Substring(1));
        int newRow = int.Parse(newPosition.Substring(1));
        char currentColumn = currentPosition[0];
        char newColumn = newPosition[0];

        
        if (currentColumn == newColumn && newRow == currentRow - 1 && IsSquareEmpty(newPosition))
        {
            return true;
        }

        
        if (currentColumn == newColumn && currentRow == 7 && newRow == currentRow - 2
            && IsSquareEmpty(newPosition) && IsSquareEmpty(currentColumn.ToString() + (currentRow - 1)))
        {
            return true;
        }

        
        if (Math.Abs(newColumn - currentColumn) == 1 && newRow == currentRow - 1
            && IsOpponentPieceAt(newPosition))
        {
            return true;
        }

        return false;
    }


    private bool IsSquareEmpty(string position)
    {
        bool isEmpty = !gameBoard.piecesOnBoard.ContainsKey(position);
        Debug.Log($"Checking if square {position} is empty: {isEmpty}");
        return isEmpty;
    }

    private bool IsOpponentPieceAt(string position)
    {
        if (gameBoard.piecesOnBoard.TryGetValue(position, out GameObject pieceAtPosition))
        {
            bool isOpponent = pieceAtPosition != null && pieceAtPosition.CompareTag(this.CompareTag("WhitePiece") ? "BlackPiece" : "WhitePiece");
            Debug.Log($"Checking if square {position} has an opponent piece: {isOpponent}");
            return isOpponent;
        }
        return false;
    }

    private void HighlightLegalMoves()
    {
        ClearHighlights();
        foreach (var position in GetLegalMoves())
        {
            GameObject square = gameBoard.GetSquare(position);
            GameObject highlight = Instantiate(highlightPrefab, square.transform.position, Quaternion.identity);
            highlight.transform.Translate(0, 0.01f, 0);
            moveHighlights.Add(highlight);
        }
    }

    public void ClearHighlights()
    {
        foreach (var highlight in moveHighlights)
        {
            Destroy(highlight);
        }
        moveHighlights.Clear();
    }

    

    public IEnumerable<string> GetLegalMoves()
    {
        
        if (string.IsNullOrEmpty(currentPosition) || currentPosition.Length != 2)
        {
            Debug.LogError("Invalid currentPosition: " + currentPosition);
            yield break;
        }

        int currentRow;
        try
        {
            currentRow = int.Parse(currentPosition.Substring(1));
        }
        catch (FormatException e)
        {
            Debug.LogError("Error parsing currentRow: " + e.Message);
            yield break;
        }

        char currentColumn = currentPosition[0];

        
        if (currentRow > 1)
        {
            string oneForward = $"{currentColumn}{currentRow - 1}";
            if (IsSquareEmpty(oneForward))
            {
                Debug.Log("Legal move (one forward): " + oneForward);
                yield return oneForward;
            }
        }

       
        if (currentRow == 7)
        {
            string twoForward = $"{currentColumn}{currentRow - 2}";
            string intermediateSquare = $"{currentColumn}{currentRow - 1}";
            if (IsSquareEmpty(twoForward) && IsSquareEmpty(intermediateSquare))
            {
                Debug.Log("Legal move (two forward): " + twoForward);
                yield return twoForward;
            }
        }

        
        int[] captureOffsets = { -1, 1 };
        foreach (var offset in captureOffsets)
        {
            char newColumn = (char)(currentColumn + offset);
            if (newColumn >= 'A' && newColumn <= 'H')
            {
                string diagonalPosition = $"{newColumn}{currentRow - 1}";
                if (IsOpponentPieceAt(diagonalPosition))
                {
                    Debug.Log("Legal move (capture): " + diagonalPosition);
                    yield return diagonalPosition;
                }
            }
        }
    }

    public void StartMoveAnimation(string newPosition)
    {
        StartCoroutine(MovePieceAnimation(newPosition));
    }

    IEnumerator MovePieceAnimation(string newPosition)
    {
        IsAnimationPlaying = true;
        GameObject capturedPiece = null;
        if (IsOpponentPieceAt(newPosition))
        {
            capturedPiece = gameBoard.piecesOnBoard[newPosition];
            gameBoard.piecesOnBoard.Remove(newPosition);
        }

        gameBoard.UpdatePiecePosition(currentPosition, newPosition, gameObject);
        currentPosition = newPosition;

        
        Vector3 startPos = transform.position;
        Vector3 endPos = gameBoard.GetSquare(newPosition).transform.position + gameBoard.pieceOffset;
        Vector3 liftPos = new Vector3(startPos.x, startPos.y + 0.075f, startPos.z);

        float duration = 0.8f;
        float elapsedTime = 0;

       
        while (elapsedTime < duration / 3)
        {
            transform.position = Vector3.Lerp(startPos, liftPos, elapsedTime / (duration / 3));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        
        elapsedTime = 0;
        Vector3 midPos = new Vector3(endPos.x, liftPos.y, endPos.z);
        while (elapsedTime < duration / 3)
        {
            transform.position = Vector3.Lerp(liftPos, midPos, elapsedTime / (duration / 3));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

       
        elapsedTime = 0;
        while (elapsedTime < duration / 3)
        {
            transform.position = Vector3.Lerp(midPos, endPos, elapsedTime / (duration / 3));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;
        if (capturedPiece != null)
        {
            Destroy(capturedPiece);
        }

        ClearHighlights();
        SelectionManager.selectedPiece = null;
        FindObjectOfType<GameLogic>().SwitchTurn();
        Debug.Log(gameObject.name + " moved to " + newPosition);
        IsAnimationPlaying = false;
    }




}