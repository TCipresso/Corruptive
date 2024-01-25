using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cleanse : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Corrupted"))
        {
            Destroy(other.gameObject);
        }
    }
}

