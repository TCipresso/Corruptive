using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BotAi : MonoBehaviour
{
    public GameBoard gameBoard;
    public GameLogic gameLogic;
    public GameObject loadingImage;
    public GameObject faceImage;
    public AudioClip computingSound;
    public AudioClip completeSound;
    private AudioSource audioSource;
    public bool IsAnimationPlaying { get; set; }


    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    public void MakeMove()
    {
        StartCoroutine(ThinkingDelay());
    }

    IEnumerator ThinkingDelay()
    {
        
        IsAnimationPlaying = true;
        loadingImage.SetActive(true);
        faceImage.SetActive(false);

       
        audioSource.PlayOneShot(computingSound);

        float thinkingTime = Random.Range(3.0f, 6.0f);
        yield return new WaitForSeconds(thinkingTime);

        
        loadingImage.SetActive(false);
        faceImage.SetActive(true);

        
        audioSource.Stop();
        audioSource.PlayOneShot(completeSound);

        
        if (TryExecuteCaptureMove())
        {
            gameLogic.EndTurn();
            IsAnimationPlaying = false;
            yield break;
        }

        
        var blackPieces = gameBoard.piecesOnBoard.Values
                                   .Where(p => p != null && p.CompareTag("BlackPiece"))
                                   .OrderBy(_ => UnityEngine.Random.value)
                                   .ToList();

        foreach (var piece in blackPieces)
        {
            
            if (!piece)
                continue;

            Debug.Log($"Selected piece for move: {piece.name}");
            var legalMoves = GetLegalMovesForPiece(piece)
                             .Where(move => IsValidCoordinate(move))
                             .ToList();
            var captureMoves = legalMoves.Where(move => IsCaptureMove(piece, move)).ToList();

            
            if (captureMoves.Any())
            {
                ExecuteMove(piece, captureMoves[UnityEngine.Random.Range(0, captureMoves.Count)]);
                gameLogic.EndTurn();
                IsAnimationPlaying = false;
                yield break;
            }

            
            if (legalMoves.Any())
            {
                ExecuteMove(piece, legalMoves[UnityEngine.Random.Range(0, legalMoves.Count)]);
                gameLogic.EndTurn();
                IsAnimationPlaying = false;
                yield break;
            }
        }
        IsAnimationPlaying = false;
        gameLogic.SwitchTurn();
    }

    private IEnumerable<string> GetLegalMovesForPiece(GameObject piece)
    {
        var legalMoves = new List<string>();

        if (piece.GetComponent<Rook>() != null)
            legalMoves = piece.GetComponent<Rook>().GetLegalMoves().ToList();
        else if (piece.GetComponent<Knight>() != null)
            legalMoves = piece.GetComponent<Knight>().GetLegalMoves().ToList();
        else if (piece.GetComponent<Bishop>() != null)
            legalMoves = piece.GetComponent<Bishop>().GetLegalMoves().ToList();
        else if (piece.GetComponent<BlackPawn>() != null)
            legalMoves = piece.GetComponent<BlackPawn>().GetLegalMoves().ToList();
        else if (piece.GetComponent<King>() != null)
            legalMoves = piece.GetComponent<King>().GetLegalMoves().ToList();
        else if (piece.GetComponent<Queen>() != null)
            legalMoves = piece.GetComponent<Queen>().GetLegalMoves().ToList();

        return legalMoves.Where(move =>
        {
            if (gameBoard.piecesOnBoard.TryGetValue(move, out GameObject pieceAtDestination))
            {
                if (pieceAtDestination.CompareTag(piece.tag))
                    return false;
            }
            return true;
        });
    }


    private bool IsCaptureMove(GameObject piece, string move)
    {
        GameObject targetSquare = gameBoard.GetSquare(move);
        if (targetSquare == null)
        {
            Debug.LogError("Target square not found for move: " + move);
            return false;
        }

        
        if (!gameBoard.piecesOnBoard.TryGetValue(move, out GameObject pieceAtDestination))
        {
            return false;
        }

        
        bool isOpponentPiece = (piece.CompareTag("WhitePiece") && pieceAtDestination.CompareTag("BlackPiece"))
                            || (piece.CompareTag("BlackPiece") && pieceAtDestination.CompareTag("WhitePiece"));

        return isOpponentPiece;
    }

    private void ExecuteMove(GameObject piece, string newPosition)
    {
        if (piece.GetComponent<Rook>() != null)
            piece.GetComponent<Rook>().StartMoveAnimation(newPosition);
        else if (piece.GetComponent<Knight>() != null)
            piece.GetComponent<Knight>().StartMoveAnimation(newPosition);
        else if (piece.GetComponent<Bishop>() != null)
            piece.GetComponent<Bishop>().StartMoveAnimation(newPosition);
        else if (piece.GetComponent<BlackPawn>() != null)
            piece.GetComponent<BlackPawn>().StartMoveAnimation(newPosition);
        else if (piece.GetComponent<King>() != null)
            piece.GetComponent<King>().StartMoveAnimation(newPosition);
        else if (piece.GetComponent<Queen>() != null)
            piece.GetComponent<Queen>().StartMoveAnimation(newPosition);
        
    }



    private string GetPieceCurrentPosition(GameObject piece)
    {
        if (piece.GetComponent<Rook>() != null)
            return piece.GetComponent<Rook>().currentPosition;
        else if (piece.GetComponent<Knight>() != null)
            return piece.GetComponent<Knight>().currentPosition;
        else if (piece.GetComponent<Bishop>() != null)
            return piece.GetComponent<Bishop>().currentPosition;
        else if (piece.GetComponent<BlackPawn>() != null)
            return piece.GetComponent<BlackPawn>().currentPosition;
        else if (piece.GetComponent<King>() != null)
            return piece.GetComponent<King>().currentPosition;
        else if (piece.GetComponent<Queen>() != null)
            return piece.GetComponent<Queen>().currentPosition;

        return null;
    }

    private void UpdatePieceCurrentPosition(GameObject piece, string newPosition)
    {
        if (piece.GetComponent<Rook>() != null)
            piece.GetComponent<Rook>().currentPosition = newPosition;
        else if (piece.GetComponent<Knight>() != null)
            piece.GetComponent<Knight>().currentPosition = newPosition;
        else if (piece.GetComponent<Bishop>() != null)
            piece.GetComponent<Bishop>().currentPosition = newPosition;
        else if (piece.GetComponent<BlackPawn>() != null)
            piece.GetComponent<BlackPawn>().currentPosition = newPosition;
        else if (piece.GetComponent<King>() != null)
            piece.GetComponent<King>().currentPosition = newPosition;
        else if (piece.GetComponent<Queen>() != null)
            piece.GetComponent<Queen>().currentPosition = newPosition;
        
    }

    private bool TryExecuteCaptureMove()
    {
        
        var blackPieces = gameBoard.piecesOnBoard.Values
                                   .Where(p => p != null && p.CompareTag("BlackPiece"))
                                   .ToList();

        foreach (var piece in blackPieces)
        {
            
            if (piece == null || !piece)
                continue;

            var legalMoves = GetLegalMovesForPiece(piece).ToList();
            var captureMoves = legalMoves.Where(move => IsCaptureMove(piece, move)).ToList();

            if (captureMoves.Any())
            {
                ExecuteMove(piece, captureMoves[Random.Range(0, captureMoves.Count)]);
                return true;
            }
        }

        return false;
    }

    private bool IsValidCoordinate(string coordinate)
    {
        if (string.IsNullOrEmpty(coordinate) || coordinate.Length != 2)
        {
            return false;
        }

        char column = coordinate[0];
        int row = int.Parse(coordinate.Substring(1));
        return column >= 'A' && column <= 'H' && row >= 1 && row <= 8;
    }


}