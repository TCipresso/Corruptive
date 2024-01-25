using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameBoard : MonoBehaviour
{
    public GameObject[] boardSquares;

    public GameObject whitePawn, whiteRook, whiteBishop, whiteKnight, whiteQueen, whiteKing;
    public GameObject blackPawn, blackRook, blackBishop, blackKnight, blackQueen, blackKing;
    public GameObject corruptionPrefab;
    public HashSet<string> corruptedSquares = new HashSet<string>();
    private int corruptionTurnCount = 0;
    [SerializeField] private AudioClip corruptionSound;
    private AudioSource audioSource;
    public AudioClip CORRUPTEDSFX;


    public Vector3 pieceOffset = new Vector3(-0.0325f, 0, 0.0325f);
    public Dictionary<string, GameObject> piecesOnBoard = new Dictionary<string, GameObject>();
    
    public void Start()
    {
        audioSource = GetComponent<AudioSource>();
        InitializeBoard();
        GameStart();
        InitializeCorruption();

    }

    void InitializeBoard()
    {
        boardSquares = new GameObject[64];

        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject square = transform.GetChild(i).gameObject;

            
            string squareName = square.name;
            int column = squareName[0] - 'A'; 
            int row = squareName[1] - '1'; 

            int index = column * 8 + row;
            boardSquares[index] = square;
        }
    }

    private void InitializeCorruption()
    {
        CorruptSquare("E8");

        // Corrupt a random square in row 5
        string randomSquareInRow5 = GetRandomSquareInRow5();
        CorruptSquare(randomSquareInRow5);
    }

    private string GetRandomSquareInRow5()
    {
        char column = (char)('A' + UnityEngine.Random.Range(0, 8));
        return $"{column}5";
    }

    public bool IsSquareCorrupted(string position)
    {
        return corruptedSquares.Contains(position);
    }

    private void CorruptSquare(string square)
    {
        if (corruptedSquares.Contains(square))
            return;  

        corruptedSquares.Add(square);
        GameObject squareObj = GetSquare(square);
        if (squareObj != null)
        {
            Instantiate(corruptionPrefab, new Vector3(squareObj.transform.position.x, squareObj.transform.position.y + 0.005f, squareObj.transform.position.z), Quaternion.identity);

            
            if (piecesOnBoard.TryGetValue(square, out GameObject pieceOnSquare) && pieceOnSquare.CompareTag("WhitePiece") && !IsPieceAlreadyCorrupted(pieceOnSquare))
            {
                ApplyCorruptionEffect(pieceOnSquare);
                MarkPieceAsCorrupted(pieceOnSquare);
            }
        }
        else
        {
            Debug.LogError("Square object not found for " + square);
        }
    }


    
    public GameObject GetSquare(string coordinate)
    {
        int column = coordinate[0] - 'A';
        int row = coordinate[1] - '1';
        int index = column * 8 + row;
        if (index < 0 || index >= boardSquares.Length)
        {
            Debug.LogError($"Index out of range for coordinate: {coordinate}. Calculated index: {index}");
            return null;
        }
        return boardSquares[index];
    }


    public void GameStart()
    {
        
        for (int i = 0; i < 8; i++)
        {
            string pawnPosition = ((char)('A' + i)).ToString() + "2";
            GameObject whitePawnObject = Instantiate(whitePawn, GetSquare(pawnPosition).transform.position + pieceOffset, Quaternion.identity);
            Pawn whitePawnScript = whitePawnObject.GetComponent<Pawn>();
            if (whitePawnScript != null)
            {
                whitePawnScript.currentPosition = pawnPosition;
                piecesOnBoard[pawnPosition] = whitePawnObject;  // Add the pawn to the board tracker
            }
        }

        
        for (int i = 0; i < 8; i++)
        {
            string pawnPosition = ((char)('A' + i)).ToString() + "7";
            GameObject blackPawnObject = Instantiate(blackPawn, GetSquare(pawnPosition).transform.position + pieceOffset, Quaternion.identity);
            BlackPawn blackPawnScript = blackPawnObject.GetComponent<BlackPawn>();
            if (blackPawnScript != null)
            {
                blackPawnScript.currentPosition = pawnPosition;
                piecesOnBoard[pawnPosition] = blackPawnObject; 
            }
        }


        // Set up Rooks
        InstantiateAndTrackRook("A1", whiteRook);
        InstantiateAndTrackRook("H1", whiteRook);
        InstantiateAndTrackRook("A8", blackRook);
        InstantiateAndTrackRook("H8", blackRook);

        // Set up Knights
        InstantiateAndTrackKnight("B1", whiteKnight);
        InstantiateAndTrackKnight("G1", whiteKnight);
        InstantiateAndTrackKnight("B8", blackKnight);
        InstantiateAndTrackKnight("G8", blackKnight);

        // Set up Bishops
        InstantiateAndTrackBishop("C1", whiteBishop);
        InstantiateAndTrackBishop("F1", whiteBishop);
        InstantiateAndTrackBishop("C8", blackBishop);
        InstantiateAndTrackBishop("F8", blackBishop);


        // Set up Queens
        InstantiateAndTrackQueen("D1", whiteQueen);
        InstantiateAndTrackQueen("D8", blackQueen);

        // Set up Kings
        InstantiateAndTrackKing("E1", whiteKing);
        InstantiateAndTrackKing("E8", blackKing);
    }


    public void UpdatePiecePosition(string oldPosition, string newPosition, GameObject piece)
    {
        if (oldPosition != null && piecesOnBoard.ContainsKey(oldPosition))
        {
            piecesOnBoard.Remove(oldPosition);
        }

        piecesOnBoard[newPosition] = piece;
    }

    public GameObject GetPieceAtPosition(string position)
    {
        if (piecesOnBoard.ContainsKey(position))
        {
            return piecesOnBoard[position];
        }
        return null;
    }

    public bool IsSquareOccupied(string position)
    {
        return piecesOnBoard.ContainsKey(position);
    }

    private void InstantiateAndTrackRook(string position, GameObject rookPrefab)
    {
        GameObject rookObject = Instantiate(rookPrefab, GetSquare(position).transform.position + pieceOffset, Quaternion.identity);
        Rook rookScript = rookObject.GetComponent<Rook>();
        if (rookScript != null)
        {
            rookScript.currentPosition = position;
            piecesOnBoard[position] = rookObject;
        }
    }

    private void InstantiateAndTrackBishop(string position, GameObject bishopPrefab)
    {
        GameObject bishopObject = Instantiate(bishopPrefab, GetSquare(position).transform.position + pieceOffset, Quaternion.identity);
        Bishop bishopScript = bishopObject.GetComponent<Bishop>();
        if (bishopScript != null)
        {
            bishopScript.currentPosition = position;
            piecesOnBoard[position] = bishopObject;
        }
    }

    private void InstantiateAndTrackKnight(string position, GameObject knightPrefab)
    {
        GameObject knightObject = Instantiate(knightPrefab, GetSquare(position).transform.position + pieceOffset, Quaternion.identity);
        Knight knightScript = knightObject.GetComponent<Knight>();
        if (knightScript != null)
        {
            knightScript.currentPosition = position;
            piecesOnBoard[position] = knightObject;
        }
    }

    private void InstantiateAndTrackQueen(string position, GameObject queenPrefab)
    {
        GameObject queenObject = Instantiate(queenPrefab, GetSquare(position).transform.position + pieceOffset, Quaternion.identity);
        Queen queenScript = queenObject.GetComponent<Queen>();
        if (queenScript != null)
        {
            queenScript.currentPosition = position;
            piecesOnBoard[position] = queenObject;
        }
    }

    private void InstantiateAndTrackKing(string position, GameObject kingPrefab)
    {
        GameObject kingObject = Instantiate(kingPrefab, GetSquare(position).transform.position + pieceOffset, Quaternion.identity);
        King kingScript = kingObject.GetComponent<King>();
        if (kingScript != null)
        {
            kingScript.currentPosition = position;
            piecesOnBoard[position] = kingObject;
        }
    }

    public void SpreadCorruption()
    {
        audioSource.PlayOneShot(corruptionSound);
        List<string> newCorruptedSquares = new List<string>();

       
        if (corruptionTurnCount % 2 == 0)
        {
            
            newCorruptedSquares.AddRange(GetPlusPatternCorruptedSquares());
        }
        else
        {
            
            newCorruptedSquares.AddRange(GetXPatternCorruptedSquares());
        }

        
        foreach (string square in newCorruptedSquares)
        {
            CorruptedSquare(square);
        }

       
        corruptionTurnCount++;
    }


    private List<string> GetPlusPatternCorruptedSquares()
    {
        List<string> newCorruptedSquares = new List<string>();

        foreach (string square in corruptedSquares)
        {
            
            char column = square[0];
            int row = int.Parse(square.Substring(1));

            
            if (row < 8)
            {
                string northSquare = $"{column}{row + 1}";
                if (!corruptedSquares.Contains(northSquare))
                {
                    newCorruptedSquares.Add(northSquare);
                }
            }

            
            if (row > 1)
            {
                string southSquare = $"{column}{row - 1}";
                if (!corruptedSquares.Contains(southSquare))
                {
                    newCorruptedSquares.Add(southSquare);
                }
            }

            
            if (column < 'H')
            {
                string eastSquare = $"{(char)(column + 1)}{row}";
                if (!corruptedSquares.Contains(eastSquare))
                {
                    newCorruptedSquares.Add(eastSquare);
                }
            }

            
            if (column > 'A')
            {
                string westSquare = $"{(char)(column - 1)}{row}";
                if (!corruptedSquares.Contains(westSquare))
                {
                    newCorruptedSquares.Add(westSquare);
                }
            }
        }

        return newCorruptedSquares;
    }

    private List<string> GetXPatternCorruptedSquares()
    {
        List<string> newCorruptedSquares = new List<string>();

        foreach (string square in corruptedSquares)
        {
            
            char column = square[0];
            int row = int.Parse(square.Substring(1));

            
            if (row < 8 && column < 'H' && (corruptedSquares.Contains($"{column}{row + 1}") || corruptedSquares.Contains($"{(char)(column + 1)}{row}")))
            {
                string neSquare = $"{(char)(column + 1)}{row + 1}";
                if (!corruptedSquares.Contains(neSquare))
                {
                    newCorruptedSquares.Add(neSquare);
                }
            }

            
            if (row > 1 && column < 'H' && (corruptedSquares.Contains($"{column}{row - 1}") || corruptedSquares.Contains($"{(char)(column + 1)}{row}")))
            {
                string seSquare = $"{(char)(column + 1)}{row - 1}";
                if (!corruptedSquares.Contains(seSquare))
                {
                    newCorruptedSquares.Add(seSquare);
                }
            }

            
            if (row > 1 && column > 'A' && (corruptedSquares.Contains($"{column}{row - 1}") || corruptedSquares.Contains($"{(char)(column - 1)}{row}")))
            {
                string swSquare = $"{(char)(column - 1)}{row - 1}";
                if (!corruptedSquares.Contains(swSquare))
                {
                    newCorruptedSquares.Add(swSquare);
                }
            }

            
            if (row < 8 && column > 'A' && (corruptedSquares.Contains($"{column}{row + 1}") || corruptedSquares.Contains($"{(char)(column - 1)}{row}")))
            {
                string nwSquare = $"{(char)(column - 1)}{row + 1}";
                if (!corruptedSquares.Contains(nwSquare))
                {
                    newCorruptedSquares.Add(nwSquare);
                }
            }
        }

        return newCorruptedSquares;
    }

    private void CorruptedSquare(string square)
    {
        
        if (corruptedSquares.Contains(square))
            return;

        
        corruptedSquares.Add(square);

        
        GameObject squareObject = GetSquare(square);
        if (squareObject == null)
        {
            Debug.LogError($"Square object not found for {square}");
            return;
        }

        
        Vector3 position = squareObject.transform.position;
        Instantiate(corruptionPrefab, new Vector3(position.x, position.y + 0.01f, position.z), Quaternion.identity);

       
    }

    public void ApplyCorruptionEffect(GameObject piece)
    {
        if (piece == null)
            return;

        ClearPieceHighlights(piece);

       
        if (piece.GetComponent<Pawn>() != null && piece.CompareTag("WhitePiece"))
        {
            
            SwitchSides(piece);
            Debug.Log("Side Switch");
        }
        else
        {
            
            int effectIndex = UnityEngine.Random.Range(0, 3);
            switch (effectIndex)
            {
                case 0:
                    SwitchSides(piece);
                    Debug.Log("Side Switch");
                    break;
                case 1:
                    DowngradeToPawn(piece);
                    Debug.Log("Downgrade to Pawn");
                    break;
                case 2:
                    RandomTeleport(piece);
                    Debug.Log("TelePOrt");
                    break;
                    
            }
        }

        
        audioSource.PlayOneShot(CORRUPTEDSFX);
    }

    public void SwitchSides(GameObject whitePiece)
    {
        
        string currentPosition = GetPositionStringFromPiece(whitePiece);

        
        piecesOnBoard.Remove(currentPosition);
        Destroy(whitePiece);

        
        GameObject blackPiece = InstantiateBlackPiece(whitePiece, currentPosition);

        
        if (blackPiece != null)
        {
            piecesOnBoard[currentPosition] = blackPiece;
            blackPiece.tag = "BlackPiece"; 
        }
        else
        {
            Debug.LogError("Failed to instantiate a black piece for " + currentPosition);
        }
    }

    private void DowngradeToPawn(GameObject piece)
    {
        if (piece == null)
            return;

        
        string currentPosition = GetPositionStringFromPiece(piece);

        
        piecesOnBoard.Remove(currentPosition);
        Destroy(piece);

        
        GameObject newPawn = Instantiate(whitePawn, GetSquare(currentPosition).transform.position + pieceOffset, Quaternion.identity);
        Debug.Log("WHITE");

        
        Pawn pawnScript = newPawn.GetComponent<Pawn>();
        if (pawnScript != null)
        {
            pawnScript.currentPosition = currentPosition;
            pawnScript.tag = "WhitePiece";
           
        }
        else
        {
            Debug.LogError("Pawn script not found on the instantiated white pawn.");
        }

       
        piecesOnBoard[currentPosition] = newPawn;

        Debug.Log("Piece downgraded to white pawn at position " + currentPosition);
    }



    private void RandomTeleport(GameObject piece)
    {
        if (piece == null)
            return;

        
        List<string> emptySquares = new List<string>();
        for (char column = 'A'; column <= 'H'; column++)
        {
            for (int row = 1; row <= 8; row++)
            {
                string position = $"{column}{row}";
                if (!piecesOnBoard.ContainsKey(position))
                {
                    emptySquares.Add(position);
                }
            }
        }

        
        if (emptySquares.Count > 0)
        {
            string randomEmptySquare = emptySquares[UnityEngine.Random.Range(0, emptySquares.Count)];
            string currentPosition = GetPositionStringFromPiece(piece);
            piecesOnBoard.Remove(currentPosition);
            Destroy(piece);
            GameObject newPiece = InstantiateWhitePieceAtPosition(piece, randomEmptySquare);
            if (newPiece != null)
            {
                
                piecesOnBoard[randomEmptySquare] = newPiece;
                Debug.Log("New piece instantiated at " + randomEmptySquare);
            }
            else
            {
                Debug.LogError("Failed to instantiate new piece at " + randomEmptySquare);
            }
        }
        else
        {
            Debug.Log("No empty squares available for teleportation.");
        }
    }

    public GameObject InstantiateWhitePieceAtPosition(GameObject originalPiece, string position)
    {
        GameObject newPiece = null;
        GameObject squareObject = GetSquare(position);

        if (squareObject == null)
        {
            Debug.LogError("Square object not found for position: " + position);
            return null;
        }

        
        if (originalPiece.GetComponent<Pawn>() != null)
        {
            newPiece = Instantiate(whitePawn, squareObject.transform.position + pieceOffset, Quaternion.identity);
            newPiece.GetComponent<Pawn>().currentPosition = position;
        }
        else if (originalPiece.GetComponent<Rook>() != null)
        {
            newPiece = Instantiate(whiteRook, squareObject.transform.position + pieceOffset, Quaternion.identity);
            newPiece.GetComponent<Rook>().currentPosition = position;
        }
        else if (originalPiece.GetComponent<Knight>() != null)
        {
            newPiece = Instantiate(whiteKnight, squareObject.transform.position + pieceOffset, Quaternion.identity);
            newPiece.GetComponent<Knight>().currentPosition = position;
        }
        else if (originalPiece.GetComponent<Bishop>() != null)
        {
            newPiece = Instantiate(whiteBishop, squareObject.transform.position + pieceOffset, Quaternion.identity);
            newPiece.GetComponent<Bishop>().currentPosition = position;
        }
        else if (originalPiece.GetComponent<Queen>() != null)
        {
            newPiece = Instantiate(whiteQueen, squareObject.transform.position + pieceOffset, Quaternion.identity);
            newPiece.GetComponent<Queen>().currentPosition = position;
        }
        else if (originalPiece.GetComponent<King>() != null)
        {
            newPiece = Instantiate(whiteKing, squareObject.transform.position + pieceOffset, Quaternion.identity);
            newPiece.GetComponent<King>().currentPosition = position;
        }

        if (newPiece == null)
        {
            Debug.LogError("Piece type not recognized for teleportation.");
            return null;
        }

        piecesOnBoard[position] = newPiece;

        return newPiece;
    }


    private void UpdatePieceCurrentPosition(GameObject piece, string newPosition)
    {
        var pieceScript = piece.GetComponent<MonoBehaviour>();
        if (pieceScript != null)
        {
            var currentPositionProperty = pieceScript.GetType().GetProperty("currentPosition");
            if (currentPositionProperty != null)
            {
                currentPositionProperty.SetValue(pieceScript, newPosition);
            }
            else
            {
                Debug.LogError("currentPosition property not found in " + pieceScript.GetType().Name);
            }
        }
        else
        {
            Debug.LogError("Piece script not found on the teleported object.");
        }
    }

    private void ClearPieceHighlights(GameObject piece)
    {
        if (piece.GetComponent<Knight>() != null)
            piece.GetComponent<Knight>().ClearHighlights();
        else if (piece.GetComponent<Pawn>() != null)
            piece.GetComponent<Pawn>().ClearHighlights();
        else if (piece.GetComponent<King>() != null)
            piece.GetComponent<King>().ClearHighlights();
        else if (piece.GetComponent<Queen>() != null)
            piece.GetComponent<Queen>().ClearHighlights();
        else if (piece.GetComponent<Bishop>() != null)
            piece.GetComponent<Bishop>().ClearHighlights();
    }



    private GameObject InstantiateBlackPiece(GameObject whitePiece, string position)
    {
        GameObject newPiece = null;

        if (whitePiece.GetComponent<Pawn>() != null)
            newPiece = InstantiateAndTrackPiece(position, blackPawn, typeof(BlackPawn));
        else if (whitePiece.GetComponent<Rook>() != null)
            newPiece = InstantiateAndTrackPiece(position, blackRook, typeof(Rook));
        else if (whitePiece.GetComponent<Knight>() != null)
            newPiece = InstantiateAndTrackPiece(position, blackKnight, typeof(Knight));
        else if (whitePiece.GetComponent<Bishop>() != null)
            newPiece = InstantiateAndTrackPiece(position, blackBishop,typeof(Bishop));
        else if (whitePiece.GetComponent<King>() != null)
            newPiece = InstantiateAndTrackPiece(position, blackKing, typeof(King));
        else if (whitePiece.GetComponent<Queen>() != null)
            newPiece = InstantiateAndTrackPiece(position, blackQueen, typeof(Queen));


        if (newPiece == null)
        {
            Debug.LogError("Failed to instantiate black piece for " + whitePiece.name);
        }

        return newPiece;
    }

    private GameObject InstantiateAndTrackPiece(string position, GameObject piecePrefab, Type pieceType)
    {
        GameObject pieceObject = Instantiate(piecePrefab, GetSquare(position).transform.position + pieceOffset, Quaternion.identity);
        var pieceScript = pieceObject.GetComponent(pieceType) as MonoBehaviour;
        if (pieceScript != null)
        {
            var currentPositionProperty = pieceType.GetProperty("currentPosition");
            if (currentPositionProperty != null)
            {
                currentPositionProperty.SetValue(pieceScript, position);
            }
            else
            {
                Debug.LogError("currentPosition property not found in " + pieceType.Name);
            }
        }
        else
        {
            Debug.LogError("Piece script not found on the instantiated object.");
        }

        piecesOnBoard[position] = pieceObject;
        return pieceObject;
    }

    private string GetPositionStringFromPiece(GameObject piece)
    {
       
        if (piece.GetComponent<Pawn>() != null)
            return piece.GetComponent<Pawn>().currentPosition;
        else if (piece.GetComponent<Rook>() != null)
            return piece.GetComponent<Rook>().currentPosition;
        else if (piece.GetComponent<Knight>() != null)
            return piece.GetComponent<Knight>().currentPosition;
        else if (piece.GetComponent<Bishop>() != null)
            return piece.GetComponent<Bishop>().currentPosition;
        else if (piece.GetComponent<King>() != null)
            return piece.GetComponent<King>().currentPosition;
        else if (piece.GetComponent<Queen>() != null)
            return piece.GetComponent<Queen>().currentPosition;
        

        Debug.LogError("Piece type not recognized");
        return null;
    }

    private bool IsPieceAlreadyCorrupted(GameObject piece)
    {

        var pawnScript = piece.GetComponent<Pawn>();
        if (pawnScript != null) return pawnScript.isCorrupted;

        var rookScript = piece.GetComponent<Rook>();
        if (rookScript != null) return rookScript.isCorrupted;

        var knightScript = piece.GetComponent<Knight>();
        if (knightScript != null) return knightScript.IsCorrupted();

        var bishopScript = piece.GetComponent<Bishop>();
        if (bishopScript != null) return bishopScript.isCorrupted;

        var kingScript = piece.GetComponent<King>();
        if (kingScript != null) return kingScript.isCorrupted;

        var QueenScript = piece.GetComponent<Queen>();
        if (QueenScript != null) return QueenScript.isCorrupted;

        return false; 
    }

    private void MarkPieceAsCorrupted(GameObject piece)
    {

        var pawnScript = piece.GetComponent<Pawn>();
        if (pawnScript != null) { pawnScript.SetCorrupted(true); return; }

        var rookScript = piece.GetComponent<Rook>();
        if (rookScript != null) { rookScript.SetCorrupted(true); return; }

        var knightScript = piece.GetComponent<Knight>();
        if (knightScript != null) { knightScript.SetCorrupted(true); return; }

        var bishopScript = piece.GetComponent<Bishop>();
        if (bishopScript != null) { bishopScript.SetCorrupted(true); return; }

        var kingScript = piece.GetComponent<King>();
        if (kingScript != null) { kingScript.SetCorrupted(true); return; }

        var QueenScript = piece.GetComponent<Queen>();
        if (QueenScript != null) { QueenScript.SetCorrupted(true); return; }

       
    }

    


}

