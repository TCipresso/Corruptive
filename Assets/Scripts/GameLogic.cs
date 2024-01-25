using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameLogic : MonoBehaviour
{
    public enum Player { White, Black }
    public Player currentPlayer = Player.White;
    public GameBoard gameBoard;
    public BotAi botAi;
    public CameraController cameraController;

    private int blackTurnCount = 0;

    public void SwitchTurn()
    {
        
        currentPlayer = (currentPlayer == Player.White) ? Player.Black : Player.White;

        if (currentPlayer == Player.Black)
        {
            UpdateBoardState();
            botAi.MakeMove();
            
           
        }

        CheckForWin();
    }

    public void EndTurn()
    {
       
        if (currentPlayer == Player.Black)
        {
            blackTurnCount++;
            if (blackTurnCount % 2 == 0)
            {
                gameBoard.SpreadCorruption();
            }
        }


        
        //SwitchTurn();
    }

    public void CheckForWin()
    {
        if (!gameBoard.piecesOnBoard.Values.Any(p => p.CompareTag("WhitePiece")))
        {
            Debug.Log("Black wins!");
            
        }
        else if (!gameBoard.piecesOnBoard.Values.Any(p => p.CompareTag("BlackPiece")))
        {
            Debug.Log("White wins!");
           
        }
    }

    public void UpdateBoardState()
    {
        
        var allPieces = GameObject.FindGameObjectsWithTag("WhitePiece");
        allPieces = allPieces.Concat(GameObject.FindGameObjectsWithTag("BlackPiece")).ToArray();

        foreach (var pieceGameObject in allPieces)
        {
            var pieceScripts = pieceGameObject.GetComponents<MonoBehaviour>().Where(script => script.GetType().GetProperty("currentPosition") != null);
            foreach (var script in pieceScripts)
            {
                var currentPosition = (string)script.GetType().GetProperty("currentPosition").GetValue(script);
                if (currentPosition != null && currentPosition.Length == 2)
                {
                    gameBoard.piecesOnBoard[currentPosition] = pieceGameObject;
                }
            }
        }
    }

    public void StartBlackTurn()
    {
        UpdateBoardState();
        botAi.MakeMove();
    }

    public Player GetCurrentPlayer()
    {
        return currentPlayer;
    }


}


