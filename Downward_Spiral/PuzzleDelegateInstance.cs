using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleDelegateInstance : MonoBehaviour, PuzzleCanvasDelegate
{
    [SerializeField]
    GameObject nextPuzzle;

    public void PuzzleCanvasDidPassVerification()
    {
        if (nextPuzzle != null)
        {
            nextPuzzle.SetActive(true);
        }
        gameObject.SetActive(false);
    }
}
