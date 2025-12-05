using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeSpawner : MonoBehaviour
{
    IMazeGenerator mazeGenerator = new IterativeDepthFirstGenerator();

    private bool[,] mazeGrid;
    private GameObject[,] spawnedWalls;
    
    public int mazeWidth = 20;
    public int mazeHeight = 10;
    
    public GameObject wallPrefab;

    public Camera camera;
    public IntegerInputField widthInput;
    public IntegerInputField heightInput;

    private bool cameraIsFree = false;
    
    // Start is called before the first frame update
    void Start()
    {
        SpawnMaze();
        PositionCamera();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            FreeCamera();
    }

    /// <summary>
    /// Deletes already spawned walls
    /// </summary>
    public void ClearMaze()
    {
        int tries = 0;
        if (!Application.isPlaying)
        {
            //At the moment I don't know why, but at editor time, DestroyImmediate doesn't always work
            while (transform.childCount > 0 && tries <= 15)
            {
                for (int j = transform.childCount-1; j >= 0; j--)
                {
                    DestroyImmediate(transform.GetChild(j).gameObject, true);
                }

                tries++;
            }
        }
        else
        {
            for (int i = transform.childCount-1; i >= 0 ; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
    }
    
    /// <summary>
    /// Generates a new maze and spawns the walls instantaneously
    /// </summary>
    public void SpawnMaze()
    {
        mazeGrid = mazeGenerator.Generate(mazeWidth, mazeHeight);
        spawnedWalls = new GameObject[mazeGrid.GetLength(0), mazeGrid.GetLength(1)];
        
        if (wallPrefab is null) Debug.LogError("no wall prefab was set");

        for (int i = 0; i < mazeGrid.GetLength(0); i++)
        {
            for (int j = 0; j < mazeGrid.GetLength(1); j++)
            {
                if (mazeGrid[i,j]) continue;
                var wall = Instantiate(wallPrefab, new Vector3(i, 0, j), Quaternion.identity);
                wall.transform.parent = gameObject.transform;
                spawnedWalls[i, j] = wall;
            }
        }
    }

    /// <summary>
    /// Positions the camera above the maze looking down and scales it so the entire maze fits on screen.
    /// It might rotate the camera because the width is determined by the aspect ratio
    /// </summary>
    public void PositionCamera()
    {
        if (camera is null) return;
        Vector3 centerPosition = new Vector3(mazeGrid.GetLength(0)/2, 0, mazeGrid.GetLength(1)/2);
        camera.transform.position = centerPosition + Vector3.up * 100;

        if (mazeGrid.GetLength(1) > mazeGrid.GetLength(0))
        {
            camera.transform.rotation = Quaternion.Euler(90,0,0);
            camera.orthographicSize = mazeGrid.GetLength(1)/2+1;
        }
        else
        {
            camera.transform.rotation = Quaternion.Euler(90,90,0);
            camera.orthographicSize = mazeGrid.GetLength(0)/2+1;
        }
    }

    /// <summary>
    /// Positions the camera to look at the maze in a diagonal angle
    /// </summary>
    public void FreeCamera()
    {
        if (camera is null) return;
        Vector3 centerPosition = new Vector3(mazeGrid.GetLength(0)/2, 0, mazeGrid.GetLength(1)/2);
        camera.transform.position = Vector3.up * 100;
        camera.transform.LookAt(centerPosition);
    }
    
    /// <summary>
    /// Use this to spawn a maze at runtime
    /// </summary>
    public void RuntimeSpawn()
    {
        if (widthInput is not null) mazeWidth = widthInput.Value;
        if (heightInput is not null) mazeHeight = heightInput.Value;

        ClearMaze();
        SpawnMaze();
        PositionCamera();
    }

    /// <summary>
    /// Generates a maze and shows the process over time via a coroutine
    /// </summary>
    public void StepByStepSpawn()
    {
        StopCoroutine(StepByStepRoutine());
        StartCoroutine(StepByStepRoutine());
    }

    /// <summary>
    /// Routine to delete walls according to the generation history
    /// </summary>
    IEnumerator StepByStepRoutine()
    {
        if (widthInput is not null) mazeWidth = widthInput.Value;
        if (heightInput is not null) mazeHeight = heightInput.Value;

        ClearMaze();
        
        mazeGrid = mazeGenerator.Generate(mazeWidth, mazeHeight);
        spawnedWalls = new GameObject[mazeGrid.GetLength(0), mazeGrid.GetLength(1)];
        
        if (wallPrefab is null) Debug.LogError("no wall prefab was set");

        for (int i = 0; i < mazeGrid.GetLength(0); i++)
        {
            for (int j = 0; j < mazeGrid.GetLength(1); j++)
            {
                if (i % 2 == 1 && j % 2 == 1)
                    continue;
                
                var wall = Instantiate(wallPrefab, new Vector3(i, 0, j), Quaternion.identity);
                wall.transform.parent = gameObject.transform;
                spawnedWalls[i, j] = wall;
            }
        }
        
        PositionCamera();

        var generationHistory = mazeGenerator.GetGenerationHistory();

        foreach (var step in generationHistory)
        {
            Destroy(spawnedWalls[step.row, step.column]);
            yield return new WaitForSeconds(0.01f);
        }
        
    }
    
}
