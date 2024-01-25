using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Queen : MonoBehaviour
{
    public string currentPosition { get; set; }
    public bool IsAnimationPlaying { get; private set; }
    private static Queen selectedPiece;
    private GameBoard gameBoard;
    public GameObject highlightPrefab;
    private List<GameObject> moveHighlights = new List<GameObject>();
    [SerializeField] private AudioClip selectWhiteSound;
    [SerializeField] private AudioClip selectBlackSound;
    private AudioSource audioSource;
    public bool isCorrupted = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        gameBoard = FindObjectOfType<GameBoard>();
    }

    void Update()
    {
        CheckAndApplyCorruption();
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

    void OnMouseDown()
    {
        GameLogic gameLogic = FindObjectOfType<GameLogic>();

        
        if ((this.CompareTag("WhitePiece") && gameLogic.currentPlayer != GameLogic.Player.White) ||
            (this.CompareTag("BlackPiece") && gameLogic.currentPlayer != GameLogic.Player.Black))
        {
            return; 
        }

        
        if (this.CompareTag("WhitePiece"))
        {
            audioSource.PlayOneShot(selectWhiteSound);
        }
        else if (this.CompareTag("BlackPiece"))
        {
            audioSource.PlayOneShot(selectBlackSound);
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


    private void HighlightLegalMoves()
    {
        ClearHighlights();

        foreach (var move in GetLegalMoves())
        {
            GameObject square = gameBoard.GetSquare(move);
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

    public void MoveTo(string newPosition)
    {
        if (IsLegalMove(newPosition))
        {
            audioSource.PlayOneShot(selectBlackSound);
            StartCoroutine(MovePieceAnimation(newPosition));

            
            if (gameBoard.IsSquareCorrupted(newPosition) && !isCorrupted)
            {
                SetCorrupted(true); 
                gameBoard.ApplyCorruptionEffect(gameObject); 
            }
        }
        else
        {
            Debug.Log("Illegal move for " + gameObject.name);
        }
    }


    public IEnumerable<string> GetLegalMoves()
    {
        List<string> legalMoves = new List<string>();

        // Horizontal and Vertical Moves 
        AddLineMoves(legalMoves, 1, 0);   // Up
        AddLineMoves(legalMoves, -1, 0);  // Down
        AddLineMoves(legalMoves, 0, 1);   // Right
        AddLineMoves(legalMoves, 0, -1);  // Left

        // Diagonal Moves 
        AddDiagonalMoves(legalMoves, 1, 1);   // UpRight
        AddDiagonalMoves(legalMoves, 1, -1);  // UpLeft
        AddDiagonalMoves(legalMoves, -1, 1); // DownRight
        AddDiagonalMoves(legalMoves, -1, -1); // DownLeft
        return legalMoves;
    }

    private void AddLineMoves(List<string> moves, int rowIncrement, int columnIncrement)
    {
        int currentRow = int.Parse(currentPosition.Substring(1));
        char currentColumn = currentPosition[0];

        int newRow = currentRow;
        char newColumn = currentColumn;

        while (true)
        {
            newRow += rowIncrement;
            newColumn = (char)(newColumn + columnIncrement);

            if (newRow >= 1 && newRow <= 8 && newColumn >= 'A' && newColumn <= 'H')
            {
                string newPosition = newColumn.ToString() + newRow;
                if (IsSquareEmpty(newPosition))
                {
                    moves.Add(newPosition);
                }
                else
                {
                    if (IsOpponentPieceAt(newPosition))
                    {
                        moves.Add(newPosition); 
                    }
                    break;
                }
            }
            else
            {
                break; 
            }
        }
    }

    private void AddDiagonalMoves(List<string> moves, int rowIncrement, int columnIncrement)
    {
        int currentRow = int.Parse(currentPosition.Substring(1));
        char currentColumn = currentPosition[0];

        int newRow = currentRow;
        char newColumn = currentColumn;

        while (true)
        {
            newRow += rowIncrement;
            newColumn = (char)(newColumn + columnIncrement);

            if (newRow >= 1 && newRow <= 8 && newColumn >= 'A' && newColumn <= 'H')
            {
                string newPosition = newColumn.ToString() + newRow;
                if (IsSquareEmpty(newPosition))
                {
                    moves.Add(newPosition);
                }
                else
                {
                    if (IsOpponentPieceAt(newPosition))
                    {
                        moves.Add(newPosition); 
                    }
                    break; 
                }
            }
            else
            {
                break; 
            }
        }
    }

    private bool IsOpponentPieceAt(string position)
    {
        if (gameBoard.piecesOnBoard.TryGetValue(position, out GameObject pieceAtPosition))
        {
           
            return pieceAtPosition != null && pieceAtPosition.CompareTag(this.CompareTag("WhitePiece") ? "BlackPiece" : "WhitePiece");
        }
        return false;
    }


    private bool IsSquareEmpty(string position)
    {
        return !gameBoard.piecesOnBoard.ContainsKey(position);
    }

    private bool IsLegalMove(string newPosition)
    {
        if (currentPosition.Length != 2 || newPosition.Length != 2)
        {
            Debug.LogError("Position string is not correctly formatted.");
            return false;
        }

        int currentRow = int.Parse(currentPosition.Substring(1));
        int newRow = int.Parse(newPosition.Substring(1));
        char currentColumn = currentPosition[0];
        char newColumn = newPosition[0];

        bool isHorizontalMove = currentRow == newRow;
        bool isVerticalMove = currentColumn == newColumn;
        bool isDiagonalMove = Math.Abs(currentRow - newRow) == Math.Abs(currentColumn - newColumn);

        if (!isHorizontalMove && !isVerticalMove && !isDiagonalMove)
        {
            
            return false;
        }

        
        if (IsOwnPieceAt(newPosition))
        {
            return false;
        }

        
        return IsPathClear(currentPosition, newPosition);
    }

    private bool IsOwnPieceAt(string position)
    {
        if (gameBoard.piecesOnBoard.TryGetValue(position, out GameObject pieceAtPosition))
        {
            
            return pieceAtPosition != null && pieceAtPosition.CompareTag(gameObject.tag);
        }
        return false;
    }

    private bool IsPathClear(string start, string end)
    {
        int startRow = int.Parse(start.Substring(1));
        int endRow = int.Parse(end.Substring(1));
        char startColumn = start[0];
        char endColumn = end[0];

        int rowIncrement = startRow < endRow ? 1 : (startRow > endRow ? -1 : 0);
        int columnIncrement = startColumn < endColumn ? 1 : (startColumn > endColumn ? -1 : 0);

        int currentRow = startRow + rowIncrement;
        char currentColumn = (char)(startColumn + columnIncrement);

        while (currentRow != endRow || currentColumn != endColumn)
        {
            string currentPosition = currentColumn.ToString() + currentRow;

            if (!IsSquareEmpty(currentPosition))
            {
                return false; 
            }

            currentRow += rowIncrement;
            currentColumn = (char)(currentColumn + columnIncrement);
        }

        return true; 
    }

    IEnumerator MovePieceAnimation(string newPosition)
    {
        IsAnimationPlaying = true;
        ClearHighlights();
        
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

    public void StartMoveAnimation(string newPosition)
    {
        StartCoroutine(MovePieceAnimation(newPosition));
    }
    public bool IsCorrupted()
    {
        return isCorrupted;
    }

    
    public void SetCorrupted(bool status)
    {
        isCorrupted = status;
        
    }

    private void CheckAndApplyCorruption()
    {
        
        if (this.CompareTag("WhitePiece") && !isCorrupted && gameBoard.IsSquareCorrupted(currentPosition))
        {
            SetCorrupted(true);
            gameBoard.ApplyCorruptionEffect(gameObject);
        }
    }




}