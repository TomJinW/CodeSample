using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class PuzzleCanvas : MonoBehaviour
{
    public TMPro.TMP_Text indicator;

    public GameObject testObject;
    public GameObject specialBlockPrefab;
    public GameObject placeHolderBlockPrefab;
    public GameObject puzzle1Canvas;
    public GameObject puzzle2Canvas;
    public Vector2Int[] fixedBlockPositions;
    public Vector2Int[] specialBlockPositions;

    
    public PuzzleCanvasDelegate Delegate;
    public string puzzleTitle = "Untitled";

    // Magic Number
    public int[,] savedMap = new int[10, 10];

    private List<GameObject> fixedBlocks = new List<GameObject>();
    private List<GameObject> specialBlocks = new List<GameObject>();


    private DateTime puzzleStartDateTime = DateTime.Now;

    public void StartPuzzleTimer() {
        puzzleStartDateTime = DateTime.Now;
    }

    private string GenerateMapString() {
        string output = "";
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                if (savedMap[j, 9 - i] == 1)
                {
                    output += "x";
                }
                else
                {
                    output += "o";
                }

            }
            output += "\n";
        }
        return output;
    }

    private void WriteStatisticData() {
        double seconds = DateTime.Now.Subtract(puzzleStartDateTime).TotalSeconds;
        string text =@$"
{{
  ""puzzleTitle"":""{puzzleTitle}"",
  ""timeSpentInSeconds"":""{seconds}"",
  ""puzzleMap"":""
{GenerateMapString()}""";
        var pieces = GetComponentsInChildren<Pieces>();
        foreach (Pieces piece in pieces) {
            text += "\n\"piecePlacedPositions\":\"";
            foreach (var position in piece.placedPositions) {
                text += $"({position.x},{position.y});";
            }
            text += "\"";
        }

        text += "\n}";
        DateTime dt = DateTime.Now;
        long unixTime = ((DateTimeOffset)dt).ToUnixTimeSeconds();
        Debug.Log(Application.dataPath);
        File.WriteAllText(Application.dataPath + $"/{puzzleTitle}.{unixTime}.json", text);
    }

    // Clear the map
    void setEmptyMap(int row, int column) {
        for (int i = 0; i < row; i++)
            for (int j = 0; j < column; j++)
                savedMap[i, j] = 0;
    }

    // Start is called before the first frame update
    void Start()
    {
        setEmptyMap(Internals.gridDimension.x, Internals.gridDimension.y);
        GenerateBlocks(fixedBlockPositions, placeHolderBlockPrefab, fixedBlocks);
        GenerateBlocks(specialBlockPositions, specialBlockPrefab, specialBlocks);
        StartPuzzleTimer();
        // gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // Debug output for displaying the grid map
        if (Input.GetKeyDown(KeyCode.Escape)) {
            string output = "";
            for (int i = 0; i < 10; i++) {
                for (int j = 0; j < 10; j++)
                {
                    if (savedMap[j, 9 - i] == 1)
                    {
                        output += "x";
                    }
                    else {
                        output += "o";
                    }
                   
                }
                output += "\n";
            }
            Debug.Log(output);
        }

    }


    /// <summary>
    /// Validate if there is a path between any of the special points.
    /// </summary>
    public bool ValidatePath() {
        bool result = true;
        for (int i = 0; i < specialBlockPositions.Length - 1; i++) {
            for (int j = i + 1; j < specialBlockPositions.Length; j++) {
                // Create an empty list for saving the explored blocks. This is needed for the recursive method working.
                List<Vector2Int> exploredBlocks = new List<Vector2Int>();
                // Check if there is a path
                result = result && SearchPath(specialBlockPositions[i], specialBlockPositions[j], savedMap, Internals.gridDimension, exploredBlocks);
            }
        }


        if (result)
        {
            WriteStatisticData();
            Delegate?.PuzzleCanvasDidPassVerification();
            Debug.Log("Found");
            //puzzle1Canvas.gameObject.SetActive(false);
            //puzzle2Canvas.gameObject.SetActive(true);
            //gameObject.SetActive(false);

            if (gameObject == puzzle1Canvas)
            {
                puzzle2Canvas?.SetActive(true);
            }
            gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("Not Found");
        }
        return result;
    }

    /// <summary>
    /// Return a bool value to state if the user can place a single block in the map to avoid overlapping
    /// </summary>
    /// <param name="position">The block position in the grid coordinate</param>
    /// <param name="gridDimension">The size of the grid</param>
    /// <returns>if current position is free for placement</returns>
    public bool AllowPlacement(Vector2Int position,Vector2Int gridDimension) {

        // Check if the position is out of bounds
        if (position.x < 0 || position.y < 0) {
            PlacementNotAllowed();
            return false;
        }
        if (position.x >= gridDimension.x || position.y >= gridDimension.y) {
            PlacementNotAllowed();
            return false;
        }

        bool result = savedMap[position.x, position.y] == 0;
        if (!result) {
            PlacementNotAllowed();
        }
        return result;
    }

    public void PlacementNotAllowed() {
        indicator.text = "Overlapped with something else!";
        StartCoroutine(DisableText());
    }

       IEnumerator DisableText()
    {
        yield return new WaitForSeconds(2);
        indicator.text = "";
    }

    /// <summary>
    /// Place a single block to the map
    /// You should use AllowPlacement() to check if the position is free
    /// </summary>
    /// <param name="position">>The block position in the grid coordinate</param>
    public void PlaceSingleBlock(Vector2Int position)
    {
        savedMap[position.x, position.y] = 1;
    }

    /// <summary>
    /// Remove a single block in the map
    /// </summary>
    /// <param name="position">>The block position in the grid coordinate</param>
    public void ResetSingleBlock(Vector2Int position) {
        savedMap[position.x, position.y] = 0;
    }

    /// <summary>
    /// Remove all the pieces from the map
    /// </summary>
    public void ResetAllPieces() {
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Pieces");
        foreach (GameObject obj in objects) {
            Pieces singlePiece = obj.GetComponent<Pieces>();
            singlePiece.resetToDefaultPosition();
        }
   }

    /// <summary>
    /// Remove all preset blocks from the map
    /// The GameObject should have a component of the class Blocks
    /// </summary>
    /// <param name="storedData">The stored blocks</param>
    public void ResetAllBlocks(List<GameObject> storedData)
    {
        foreach (GameObject block in storedData)
        {
            ResetSingleBlock(block.GetComponent<Blocks>().gridPosition);
            Destroy(block);
        }
        storedData.Clear();
    }


    /// <summary>
    /// Generate and place preset blocks using the stored data
    /// </summary>
    /// <param name="positions"> Positions of the stored blocks</param>
    /// <param name="prefab">The sample image object for placing on the canvas</param>
    /// <param name="storedData">The data structure of block objects to store in</param>
    public void GenerateBlocks(Vector2Int[] positions,GameObject prefab,List<GameObject> storedData) {

        // Iterate each object in the list
        foreach (Vector2Int position in positions) {
            // Get the position in the canvas coordinate
            Vector2 canvasPosition = PuzzleCanvasHelper.getPositionFromGrid(position);
            // Generate a single block
            GameObject singleBlock = Instantiate(prefab);
            // IMPORTANT: must set the parent first, attach the object to the canvas and then change other properties
            singleBlock.transform.SetParent(transform);
            // Change the position and the scale
            singleBlock.transform.position = canvasPosition;
            singleBlock.transform.SetSiblingIndex(1);
            singleBlock.transform.localScale = new Vector3(1, 1, 1);
            singleBlock.GetComponent<Blocks>().gridPosition = position;

            // Register the block to the map system
            PlaceSingleBlock(position);

            // Add to the data list for further usage
            storedData.Add(singleBlock);
        }
    }

    /// <summary>
    /// To check if the current position has a block
    /// It will also check if the current block is not explored before
    /// </summary>
    /// <param name="map">The map data structure</param>
    /// <param name="x">The x position in the grid coordinate</param>
    /// <param name="y">The y position in the grid coordinate</param>
    /// <param name="gridDimension">The size of the grid</param>
    /// <param name="data">The list for return the valid data</param>
    /// <param name="exploredBlocks">The list for explored blocks</param>
    private void AddNeighbor(int[,] map, int x, int y, Vector2Int gridDimension,List<Vector2Int> data, List<Vector2Int> exploredBlocks) {
        if (x.ifInGrid(gridDimension.x) && y.ifInGrid(gridDimension.y) && !exploredBlocks.Exists(block => block.x == x && block.y == y))
        {
            if (map[x, y] == 1) data.Add(new Vector2Int(x, y));
        }
    }

    /// <summary>
    /// Search the rectangular neighbors in the current position
    /// </summary>
    /// <param name="map">The map data structure</param>
    /// <param name="position">The position for searching the neighbors</param>
    /// <param name="gridDimension">The grid size</param>
    /// <param name="exploredBlocks">The list for storing explored blocks</param>
    /// <returns>The vaild, undiscoverd positions of the neighbors for the current position</returns>
    private List<Vector2Int> SearchRectNeighbors(int[,] map, Vector2Int position, Vector2Int gridDimension, List<Vector2Int> exploredBlocks) {
        // Create a list for storing output
        List<Vector2Int> output = new List<Vector2Int>();

        // Check if the position itself is valid in the grid
        if (!position.x.ifInGrid(gridDimension.x) || !position.y.ifInGrid(gridDimension.y))
        {
            return output;
        }

        // Add 4 rectangular neighbors
        AddNeighbor(map, position.x - 1, position.y, gridDimension, output, exploredBlocks);
        AddNeighbor(map, position.x + 1, position.y, gridDimension, output, exploredBlocks);
        AddNeighbor(map, position.x, position.y + 1, gridDimension, output, exploredBlocks);
        AddNeighbor(map, position.x, position.y - 1, gridDimension, output, exploredBlocks);
        return output;
    }

    /// <summary>
    /// Search if there is a path from point A to point B in the grid system
    /// </summary>
    /// <param name="start">The position of the starting point</param>
    /// <param name="end">The position of the destination</param>
    /// <param name="map">The map data structure</param>
    /// <param name="gridDimension">The grid size</param>
    /// <param name="exploredBlocks">The list for storing explored blocks, this should be an initialized empty list if called from outside</param>
    /// <returns>if there is a path</returns>
    public bool SearchPath(Vector2Int start, Vector2Int end, int[,] map, Vector2Int gridDimension,List<Vector2Int> exploredBlocks) {

        // Add current positions to the explored ones
        exploredBlocks.Add(start);

        //Adding objects to show the search path
        //Vector2 canvasPosition = PuzzleCanvasHelper.getPositionFromGrid(start);
        //GameObject singleBlock = Instantiate(testObject);
        //singleBlock.transform.SetParent(transform);
        //singleBlock.transform.position = canvasPosition;
        //singleBlock.transform.localScale = new Vector3(1, 1, 1);

        // Search the rectangular neighbors
        List<Vector2Int> neighbors = SearchRectNeighbors(map, start, gridDimension, exploredBlocks);

        // Devide the problem into four sub problem
        bool result = false;
        foreach (Vector2Int position in neighbors) {
            if (position == end)
            {
                // if the vaild neignbor is the destination then we find the path
                return true;
            }
            else{
                // IfPath(currentPosition,destination) = IfPath(neighborLeft,destination) || IfPath(NeighborRight,destination) || IfPath(NeighborUp,destination) || IfPath(NeighborDown,destination)
                result = result || SearchPath(position, end, map, gridDimension, exploredBlocks);
            }
        }
        return result;
    }
}
