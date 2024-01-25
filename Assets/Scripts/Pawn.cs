using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Pawn : MonoBehaviour
{
    public string currentPosition { get; set; }
    private static Pawn selectedPawn;
    public GameObject highlightPrefab;
    public bool IsAnimationPlaying { get; private set; }

    private GameBoard gameBoard;
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

    void OnMouseDown()
    {
        
        GameLogic gameLogic = FindObjectOfType<GameLogic>();

        // Check if it's the correct turn for this piece
        if ((this.CompareTag("WhitePiece") && gameLogic.currentPlayer != GameLogic.Player.White) ||
            (this.CompareTag("BlackPiece") && gameLogic.currentPlayer != GameLogic.Player.Black))
        {
            return; // Not this piece's turn
        }

        // Play sound based on the piece's color
        if (this.CompareTag("WhitePiece"))
        {
            audioSource.PlayOneShot(selectWhiteSound);
        }
        else if (this.CompareTag("BlackPiece"))
        {
            audioSource.PlayOneShot(selectBlackSound);
        }

        // Check if this piece is already selected
        if (SelectionManager.selectedPiece == gameObject)
        {
            // Clear highlights and deselect the piece
            ClearHighlights();
            SelectionManager.selectedPiece = null;
            Debug.Log(gameObject.name + " on " + currentPosition + " is deselected.");
        }
        else
        {
            // Deselect any currently selected piece and clear its highlights
            SelectionManager.DeselectPiece();

            // Select this piece
            SelectionManager.selectedPiece = gameObject;
            HighlightLegalMoves();
            Debug.Log(gameObject.name + " on " + currentPosition + " is selected.");
        }
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
                        SelectionManager.selectedPiece = null;  // Deselect the piece after moving
                    }
                }
            }
        }
    }
    public void MoveTo(string newPosition)
    {
        if (IsLegalMove(newPosition))
        {
            audioSource.PlayOneShot(selectBlackSound);
            StartCoroutine(MovePieceAnimation(newPosition));

            // Check if the square is corrupted and the piece is not already corrupted
            if (gameBoard.IsSquareCorrupted(newPosition) && !isCorrupted)
            {
                SetCorrupted(true);  // Mark the piece as corrupted
                gameBoard.ApplyCorruptionEffect(gameObject);  // Apply the corruption effect
            }
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

        // Determine direction multiplier (1 for white, -1 for black)
        int direction = this.CompareTag("WhitePiece") ? 1 : -1;

        // Forward move: One square forward
        if (currentColumn == newColumn && newRow == currentRow + direction)
        {
            // Move is legal only if the square is empty
            return IsSquareEmpty(newPosition);
        }

        // First move: Two squares forward
        bool isPawnOnStartingRow = (this.CompareTag("WhitePiece") && currentRow == 2) || (this.CompareTag("BlackPiece") && currentRow == 7);
        if (currentColumn == newColumn && isPawnOnStartingRow && newRow == currentRow + 2 * direction)
        {
            string intermediateSquare = $"{currentColumn}{currentRow + direction}";
            return IsSquareEmpty(newPosition) && IsSquareEmpty(intermediateSquare);
        }

        // Diagonal capture: One square diagonally forward
        if (Math.Abs(newColumn - currentColumn) == 1 && newRow == currentRow + direction)
        {
            // Move is legal only if capturing an opponent's piece
            return IsOpponentPieceAt(newPosition);
        }

        return false; // Not a legal move
    }

    private bool IsSquareEmpty(string position)
    {
        bool isEmpty = !gameBoard.piecesOnBoard.ContainsKey(position);
        Debug.Log($"Checking if square {position} is empty: {isEmpty}");
        Debug.Log($"Checking if square {position} is empty. GameBoard is null: {gameBoard == null}");
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
        ClearHighlights(); // Clear existing highlights

        // Highlight legal moves
        foreach (var position in GetLegalMoves())
        {
            GameObject square = gameBoard.GetSquare(position);
            GameObject highlight = Instantiate(highlightPrefab, square.transform.position, Quaternion.identity);
            highlight.transform.Translate(0, 0.01f, 0); // Slightly above the board
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
        List<string> legalMoves = new List<string>();

        int currentRow = int.Parse(currentPosition.Substring(1));
        char currentColumn = currentPosition[0];

        // One square forward
        string oneForward = $"{currentColumn}{currentRow + 1}";
        if (IsSquareEmpty(oneForward))
        {
            legalMoves.Add(oneForward);
        }

        // Two squares forward on first move
        bool isPawnOnStartingRow = (this.CompareTag("WhitePiece") && currentRow == 2) || (this.CompareTag("BlackPiece") && currentRow == 7);
        if (isPawnOnStartingRow)
        {
            string twoForward = $"{currentColumn}{currentRow + 2}";
            string intermediateSquare = $"{currentColumn}{currentRow + 1}";
            if (IsSquareEmpty(twoForward) && IsSquareEmpty(intermediateSquare))
            {
                legalMoves.Add(twoForward);
            }
        }

        // Capturing diagonally
        int[] captureOffsets = { -1, 1 };
        foreach (var offset in captureOffsets)
        {
            char newColumn = (char)(currentColumn + offset);
            string diagonalPosition = $"{newColumn}{currentRow + 1}";

            if (newColumn >= 'A' && newColumn <= 'H' && IsOpponentPieceAt(diagonalPosition))
            {
                legalMoves.Add(diagonalPosition);
            }
        }

        return legalMoves;
    }

    IEnumerator MovePieceAnimation(string newPosition)
    {
        IsAnimationPlaying = true;
        ClearHighlights();
        // Update the game board for the move and capture logic
        GameObject capturedPiece = null;
        if (IsOpponentPieceAt(newPosition))
        {
            capturedPiece = gameBoard.piecesOnBoard[newPosition];
            gameBoard.piecesOnBoard.Remove(newPosition);
        }

        gameBoard.UpdatePiecePosition(currentPosition, newPosition, gameObject);
        currentPosition = newPosition;

        // Animation setup
        Vector3 startPos = transform.position;
        Vector3 endPos = gameBoard.GetSquare(newPosition).transform.position + gameBoard.pieceOffset;
        Vector3 liftPos = new Vector3(startPos.x, startPos.y + 0.075f, startPos.z);

        float duration = 0.8f;
        float elapsedTime = 0;

        // Lift the piece up
        while (elapsedTime < duration / 3)
        {
            transform.position = Vector3.Lerp(startPos, liftPos, elapsedTime / (duration / 3));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Move to the target position
        elapsedTime = 0;
        Vector3 midPos = new Vector3(endPos.x, liftPos.y, endPos.z);
        while (elapsedTime < duration / 3)
        {
            transform.position = Vector3.Lerp(liftPos, midPos, elapsedTime / (duration / 3));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Lower the piece down
        elapsedTime = 0;
        while (elapsedTime < duration / 3)
        {
            transform.position = Vector3.Lerp(midPos, endPos, elapsedTime / (duration / 3));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Final position adjustment
        transform.position = endPos;

        // Destroy the captured piece at the end of the animation
        if (capturedPiece != null)
        {
            Destroy(capturedPiece);
        }

        // Clear highlights and switch turn
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

    // Method to set the piece's corruption status
    public void SetCorrupted(bool status)
    {
        isCorrupted = status;
        // Add any additional logic that should happen when the piece gets corrupted
    }

    private void CheckAndApplyCorruption()
    {
        // Check if the piece is white, not already corrupted, and on a corrupted square
        if (this.CompareTag("WhitePiece") && !isCorrupted && gameBoard.IsSquareCorrupted(currentPosition))
        {
            SetCorrupted(true);
            gameBoard.ApplyCorruptionEffect(gameObject); // Apply the corruption effect
        }
    }


}
