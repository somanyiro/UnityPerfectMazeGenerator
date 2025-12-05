using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Loading;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using Random = UnityEngine.Random;

public class IterativeDepthFirstGenerator : IMazeGenerator
{
    private List<(int row, int column)> history;

    public bool[,] Generate(int width, int height)
    {
        Cell[,] cellGrid = Helpers.InitializeArray<Cell>(height, width);
        bool[,] mazeGrid = new bool[height * 2 + 1, width * 2 + 1];//a grid for the walls in-between and around the cells
        Stack<Cell> cellStack = new Stack<Cell>();
        history = new List<(int i, int j)>();
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                //tell the cells where they are located in the grid
                cellGrid[y, x].x = x*2+1;
                cellGrid[y, x].y = y*2+1;
                
                mazeGrid[y*2+1, x*2+1] = true;
                //SetCellAndRecord(ref mazeGrid, y*2+1, x*2+1, true);
                
                //give the cells references to their neighbours
                if (y + 1 < height) cellGrid[y, x].topNeighbour = cellGrid[y + 1, x];
                if (y - 1 >= 0) cellGrid[y, x].bottomNeighbour = cellGrid[y - 1, x];
                if (x + 1 < width) cellGrid[y, x].rightNeighbour = cellGrid[y, x + 1];
                if (x - 1 >= 0) cellGrid[y, x].leftNeighbour = cellGrid[y, x - 1];
            }
        }
        
        //iterative generation
        Cell currentCell = cellGrid[Random.Range(0, height),Random.Range(0, width)];
        currentCell.visited = true;
        cellStack.Push(currentCell);

        while (cellStack.Count > 0)
        {
            currentCell = cellStack.Pop();

            int[] directions = {0,1,2,3};
        
            directions.KnuthShuffle();
            
            foreach (int dir in directions)
            {
                switch (dir)
                {
                    case 0:
                        if (currentCell.topNeighbour is not null && currentCell.topNeighbour.visited == false)
                        {
                            SetCellAndRecord(ref mazeGrid, currentCell.y + 1, currentCell.x, true);
                            //mazeGrid[currentCell.y + 1, currentCell.x] = true;
                            currentCell.topNeighbour.visited = true;
                            cellStack.Push(currentCell);
                            cellStack.Push(currentCell.topNeighbour);
                        }
                        break;
                    case 1:
                        if (currentCell.rightNeighbour is not null && currentCell.rightNeighbour.visited == false)
                        {
                            SetCellAndRecord(ref mazeGrid, currentCell.y, currentCell.x + 1, true);
                            //mazeGrid[currentCell.y, currentCell.x + 1] = true;
                            currentCell.rightNeighbour.visited = true;
                            cellStack.Push(currentCell);
                            cellStack.Push(currentCell.rightNeighbour);
                        }
                        break;
                    case 2:
                        if (currentCell.bottomNeighbour is not null && currentCell.bottomNeighbour.visited == false)
                        {
                            SetCellAndRecord(ref mazeGrid, currentCell.y - 1, currentCell.x, true);
                            //mazeGrid[currentCell.y - 1, currentCell.x] = true;
                            currentCell.bottomNeighbour.visited = true;
                            cellStack.Push(currentCell);
                            cellStack.Push(currentCell.bottomNeighbour);
                        }
                        break;
                    case 3:
                        if (currentCell.leftNeighbour is not null && currentCell.leftNeighbour.visited == false)
                        {
                            SetCellAndRecord(ref mazeGrid, currentCell.y, currentCell.x - 1, true);
                            //mazeGrid[currentCell.y, currentCell.x - 1] = true;
                            currentCell.leftNeighbour.visited = true;
                            cellStack.Push(currentCell);
                            cellStack.Push(currentCell.leftNeighbour);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        return mazeGrid;
    }

    public List<(int row, int column)> GetGenerationHistory()
    {
        return history;
    }

    void SetCellAndRecord(ref bool[,] array, int row, int column, bool value)
    {
        array[row, column] = value;
        history.Add((row, column));
    }
    
}
