using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleCanvasHelper : MonoBehaviour
{
    //public Vector2Int gridDimension; 
    //public Vector2 gridInitPosition;
    //public Vector2 standardCanvasSize;
    //public float singleGridSize;

    // Start is called before the first frame update
    void Start()
    {
        //Internals.gridDimension = gridDimension;
        //Internals.gridInitPosition = gridInitPosition;
        //Internals.standardCanvasSize = standardCanvasSize;
        //Internals.singleGridSize = singleGridSize;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Return the grid system coordinate based on the mouse position
    /// </summary>
    /// <param name="mousePosition">The current mouse position in the screen coordinate system</param>
    /// <param name="gridDimension">The size of the grid</param>
    /// <returns></returns>
    public static Vector2Int getGridPosition(Vector2 mousePosition,Vector2Int gridDimension)
    {
        // Get the scaler to scale properly according to the current resolution
        float scalerX = Screen.width / Internals.standardCanvasSize.x;
        
        // Get the offset height if the game window is not in 16:9
        float height = (Screen.height - Screen.width / 16.0f * 9.0f) / 2.0f;

        // Obtain the grid coordinate accroding to the mouse postion and the scaled pixel size of each single block
        int x = (int)((mousePosition.x - Internals.gridInitPosition.x * scalerX) / Internals.singleGridSize / scalerX);
        int y = (int)((mousePosition.y - height - Internals.gridInitPosition.y * scalerX) / Internals.singleGridSize / scalerX);

        // Check if the result is in the grid
        if (x >= 0 && x < gridDimension.x && y >= 0 && y < gridDimension.y)
        {
            // Result is valid
            return new Vector2Int(x, y);
        }
        else {
            // Result is not valid
            return new Vector2Int(-1, -1);
        }
    }

    /// <summary>
    /// Return the position in the canvas coordinate system based on the position in the grid system.
    /// </summary>
    /// <param name="gridPostion">The position in the grid system</param>
    /// <returns></returns>
    public static Vector2 getPositionFromGrid(Vector2Int gridPostion) {

        //Debug.Log(Screen.width);
        //Debug.Log(Internals.standardCanvasSize.x);

        // Get the scaler to scale properly according to the current resolution
        float scalerX = Screen.width / Internals.standardCanvasSize.x;

        // Get the offset height if the game window is not in 16:9
        float height = (Screen.height - Screen.width / 16.0f * 9.0f) / 2.0f;

        // Obtain the canvas coordinates accroding to the grid coordinates and the scaled pixel size of each single block
        float x = (Internals.gridInitPosition.x + (gridPostion.x + 0.5f) * Internals.singleGridSize) * scalerX;
        float y = (Internals.gridInitPosition.y + (gridPostion.y + 0.5f) * Internals.singleGridSize) * scalerX + height;

        return new Vector2(x, y);
    }
}

public static class Internals {
    public static string transitionName = "Window";
    public static bool startMovingPieces = false; // To check if the user is moving the pieces
    public static Vector2Int gridDimension; // The size of the grid
    public static Vector2 gridInitPosition; // The bottom left corner position of the grid in canvas system
    public static Vector2 standardCanvasSize; // The referenced canvas size
    public static float singleGridSize; // The pixel size of a single rectangular block
}