using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform backPOV; 
    public Transform middlePOV;

    private int currentPOVIndex = 0;
    private Transform[] povs;
    public float transitionDuration;
    private bool isTransitioning = false;
    public GameLogic gameLogic;

    void Start()
    {
        povs = new Transform[] { backPOV, middlePOV };
        StartCoroutine(MoveToPOV(povs[currentPOVIndex]));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W) && !isTransitioning)
        {
            MoveToNextPOV();
        }
        else if (Input.GetKeyDown(KeyCode.S) && !isTransitioning)
        {
            MoveToPreviousPOV();
        }
    }

    void MoveToNextPOV()
    {
        if (currentPOVIndex < povs.Length - 1)
        {
            currentPOVIndex++;
            StartCoroutine(MoveToPOV(povs[currentPOVIndex]));
        }
    }

    void MoveToPreviousPOV()
    {
        if (currentPOVIndex > 0)
        {
            currentPOVIndex--;
            StartCoroutine(MoveToPOV(povs[currentPOVIndex]));
        }
    }

    IEnumerator MoveToPOV(Transform targetPOV)
    {
        isTransitioning = true;
        float elapsedTime = 0;
        Vector3 startingPosition = transform.position;
        Quaternion startingRotation = transform.rotation;

        while (elapsedTime < transitionDuration)
        {
            transform.position = Vector3.Lerp(startingPosition, targetPOV.position, elapsedTime / transitionDuration);
            transform.rotation = Quaternion.Lerp(startingRotation, targetPOV.rotation, elapsedTime / transitionDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPOV.position;
        transform.rotation = targetPOV.rotation;
        isTransitioning = false;
    }

    public void MoveToBackPOV()
    {
        if (!isTransitioning)
        {
            currentPOVIndex = 0; // Index for Back POV
            StartCoroutine(MoveToPOV(povs[currentPOVIndex]));
        }
    }

    public void MoveToMiddlePOV()
    {
        if (!isTransitioning)
        {
            currentPOVIndex = 1; // Index for Middle POV
            StartCoroutine(MoveToPOV(povs[currentPOVIndex]));
        }
    }
}