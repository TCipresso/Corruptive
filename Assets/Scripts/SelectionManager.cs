using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{
    public static GameObject selectedPiece;

    public static void DeselectPiece()
    {
        if (selectedPiece != null)
        {
           
            if (selectedPiece.GetComponent<Rook>() != null)
            {
                selectedPiece.GetComponent<Rook>().ClearHighlights();
            }
            else if (selectedPiece.GetComponent<Bishop>() != null)
            {
                selectedPiece.GetComponent<Bishop>().ClearHighlights();
            }
            else if (selectedPiece.GetComponent<Knight>() != null)
            {
                selectedPiece.GetComponent<Knight>().ClearHighlights();
            }
            else if (selectedPiece.GetComponent<Pawn>() != null)
            {
                selectedPiece.GetComponent<Pawn>().ClearHighlights();
            }
            else if (selectedPiece.GetComponent<Queen>() != null)
            {
                selectedPiece.GetComponent<Queen>().ClearHighlights();
            }
            else if (selectedPiece.GetComponent<King>() != null)
            {
                selectedPiece.GetComponent<King>().ClearHighlights();
            }
            

            selectedPiece = null;
        }
    }
}