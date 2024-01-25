using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class King : MonoBehaviour
{
    public string currentPosition { get; set; }
    public bool IsAnimationPlaying { get; private set; }
    private static King selectedPiece;
    private GameBoard gameBoard;
    public GameObject highlightPrefab;
    private List<GameObject> moveHighlights = new List<GameObject>();
    [SerializeField] private AudioClip selectWhiteSound;
    [SerializeField] private AudioClip selectBlackSound;
    private AudioSource audioSource;
    public bool isCorrupted = false;
    public bool isWhite = false;
    private BotAi bot;
    private GameLogic gameLogic;




    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        gameBoard = FindObjectOfType<GameBoard>();
        bot = FindObjectOfType<BotAi>();
        gameLogic = FindObjectOfType<GameLogic>();
    }

    void Update()
    {
        if (isWhite && !AnyPieceAnimating())
        {
            CleanseCorruption(currentPosition);
        }
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

    bool AnyPieceAnimating()
    {
        foreach (var pieceEntry in gameBoard.piecesOnBoard)
        {
            var piece = pieceEntry.Value;
            if (piece != null)
            {
                
                if ((piece.GetComponent<Pawn>()?.IsAnimationPlaying == true) || (piece.GetComponent<Rook>()?.IsAnimationPlaying == true) || (piece.GetComponent<Knight>()?.IsAnimationPlaying == true) || (piece.GetComponent<Bishop>()?.IsAnimationPlaying == true) ||
                    (piece.GetComponent<King>()?.IsAnimationPlaying == true) || (piece.GetComponent<Queen>()?.IsAnimationPlaying == true) || (piece.GetComponent<BlackPawn>()?.IsAnimationPlaying == true))
                {
                    return true;
                }
            }
        }
        return false;
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

            
            

            
            CleanseCorruption(newPosition);
        }
        else
        {
            Debug.Log("Illegal move for " + gameObject.name);
        }
    }

    private void CleanseCorruption(string kingPosition)
    {
        List<string> cleansingZone = GetCleansingZoneSquares(kingPosition);

        foreach (string position in cleansingZone)
        {
            
            if (gameBoard.corruptedSquares.Contains(position))
            {
                gameBoard.corruptedSquares.Remove(position);
                
            }

           
            if (gameBoard.piecesOnBoard.TryGetValue(position, out GameObject piece) && piece.CompareTag("BlackPiece"))
            {
                Destroy(piece);
                GameObject newWhitePiece = gameBoard.InstantiateWhitePieceAtPosition(piece, position);
                if (newWhitePiece != null)
                {
                    gameBoard.piecesOnBoard[position] = newWhitePiece;
                    
                }
            }
        }
    }

    private List<string> GetCleansingZoneSquares(string kingPosition)
    {
        List<string> zoneSquares = new List<string>();
        int column = kingPosition[0] - 'A';
        int row = int.Parse(kingPosition[1].ToString()) - 1;

        for (int colOffset = -1; colOffset <= 1; colOffset++)
        {
            for (int rowOffset = -1; rowOffset <= 1; rowOffset++)
            {
                int newColumn = column + colOffset;
                int newRow = row + rowOffset;

                if (newColumn >= 0 && newColumn < 8 && newRow >= 0 && newRow < 8)
                {
                    string newPosition = $"{(char)('A' + newColumn)}{newRow + 1}";
                    zoneSquares.Add(newPosition);
                }
            }
        }

        return zoneSquares;
    }


    public IEnumerable<string> GetLegalMoves()
    {
        List<string> legalMoves = new List<string>();

        int[] rowOffsets = { 1, 1, 1, 0, 0, -1, -1, -1 };
        int[] colOffsets = { -1, 0, 1, -1, 1, -1, 0, 1 };

        for (int i = 0; i < 8; i++)
        {
            int newRow = currentPosition[1] - '1' + rowOffsets[i];
            char newCol = (char)(currentPosition[0] + colOffsets[i]);
            if (newRow >= 0 && newRow < 8 && newCol >= 'A' && newCol <= 'H')
            {
                string newPosition = $"{newCol}{newRow + 1}";
                if (IsSquareEmpty(newPosition) || IsOpponentPieceAt(newPosition))
                {
                    legalMoves.Add(newPosition);
                }
            }
        }
        return legalMoves;
    }
    private bool IsSquareEmpty(string position)
    {
        
        return !gameBoard.piecesOnBoard.ContainsKey(position);
    }

    private bool IsOpponentPieceAt(string position)
    {
        
        if (gameBoard.piecesOnBoard.TryGetValue(position, out GameObject pieceAtPosition))
        {
            return pieceAtPosition != null && pieceAtPosition.CompareTag(this.CompareTag("WhitePiece") ? "BlackPiece" : "WhitePiece");
        }
        return false;
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

        int rowDifference = Math.Abs(currentRow - newRow);
        int columnDifference = Math.Abs(currentColumn - newColumn);

        
        if (rowDifference <= 1 && columnDifference <= 1)
        {
            
            return !IsOwnPieceAt(newPosition);
        }

        return false;
    }

    private bool IsOwnPieceAt(string position)
    {
        if (gameBoard.piecesOnBoard.TryGetValue(position, out GameObject pieceAtPosition))
        {
            
            return pieceAtPosition != null && pieceAtPosition.CompareTag(gameObject.tag);
        }
        return false;
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
