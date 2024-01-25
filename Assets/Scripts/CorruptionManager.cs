using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorruptionManager : MonoBehaviour
{
    public GameObject corruptionPrefab;
    public GameBoard gameBoard;
    private HashSet<string> corruptedSquares = new HashSet<string>();

    void Start()
    {
        gameBoard = FindObjectOfType<GameBoard>();
        InitializeCorruption();
    }

    private void InitializeCorruption()
    { 
        CorruptSquare("E8");
        string randomSquareInRow5 = GetRandomSquareInRow5();
        CorruptSquare(randomSquareInRow5);
    }

    private string GetRandomSquareInRow5()
    {
        char column = (char)('A' + Random.Range(0, 8));
        return $"{column}5";
    }

    private void CorruptSquare(string square)
    {
        if (corruptedSquares.Contains(square))
            return;

        corruptedSquares.Add(square);
        GameObject squareObj = gameBoard.GetSquare(square);
        if (squareObj == null)
        {
            Debug.LogError($"Square object not found for {square}");
            return;
        }
        Vector3 position = squareObj.transform.position;
        Instantiate(corruptionPrefab, new Vector3(position.x, position.y + 0.01f, position.z), Quaternion.identity);
    }

    
}
