using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sizetester : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject square;
    void Start()
    {
        Debug.Log("Square size: " + square.transform.localScale);
    }
}



