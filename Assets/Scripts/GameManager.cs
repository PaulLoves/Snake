using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    int gridWidth = 10, gridHeight = 10;
    [SerializeField]
    float gameTickLength;
    [SerializeField]
    int gridLayout;

    int score = 0;

    [SerializeField]
    Text scoreText;
    [SerializeField]
    InputField layoutInputField;
    [SerializeField]
    GameObject gameOverButtonObject;

    int[,] gameGrid;

    Snake snake;
    Vector2Int movementDirection;
    bool canChangeDirection = true;

    Obstacles obstacles;

    Food food;

    List<IPlottable> plottables = new List<IPlottable>();

    event Action OnSnakeFoodEncounter;
    event Action OnSnakeObstacleEncounter;

    public int GridWidth => gridWidth;
    public int GridHeight => gridHeight;
    public float GameTickLength => gameTickLength;
    public int[,] GameGrid => gameGrid;
    public int Score
    {
        get
        {
            return score;
        }

        private set
        {
            score = value;
            scoreText.text = "Score: " + score.ToString();
        }
    }

    private void Awake()
    {
        gridWidth = Utils.CeilPower2(gridWidth) + 1;
        gridHeight = Utils.CeilPower2(gridHeight) + 1;

        gridLayout = PlayerPrefs.GetInt("layout");

        gameGrid = new int[gridWidth, gridHeight];
        snake = new Snake();
        obstacles = new Obstacles(gridLayout, gameGrid);
        food = new Food();

        OnSnakeFoodEncounter += delegate
        {
            snake.OnFoodEncounter();
            Score++;
        };

        OnSnakeObstacleEncounter += delegate
        {
            snake.OnObstacleEncounter();
            DisplayGameOverScreen();
        };

        //Always keep this adding order.
        plottables.Add(obstacles);
        plottables.Add(food);
        plottables.Add(snake);
        
        movementDirection = new Vector2Int(1, 0);
    }

    private void Start()
    {
        StartCoroutine(RunGameLoopRoutine());
    }

    private void Update()
    {
        GetUserInput();
    }

    void GetUserInput()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            if (!Utils.OpposingVectors(movementDirection, Vector2Int.up) && canChangeDirection)
            {
                movementDirection = Vector2Int.up;
                canChangeDirection = false;
            }
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            if (!Utils.OpposingVectors(movementDirection, Vector2Int.down) && canChangeDirection)
            {
                movementDirection = Vector2Int.down;
                canChangeDirection = false;
            }
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            if (!Utils.OpposingVectors(movementDirection, Vector2Int.right) && canChangeDirection)
            {
                movementDirection = Vector2Int.right;
                canChangeDirection = false;
            }
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            if (!Utils.OpposingVectors(movementDirection, Vector2Int.left) && canChangeDirection)
            {
                movementDirection = Vector2Int.left;
                canChangeDirection = false;
            }
        }
    }

    void RunGameLoop()
    {
        
            //Start every tick with an empty grid. Not the most optimal, but the easiest way.
            NullGameGrid();

        //Run functionality for each tick of the game loop.

        if (!food.FoodAlreadyGenerated)
            food.GenerateFood(gameGrid);

        try
        {
            snake.Move(movementDirection, gameGrid);

            QuerySnakeHead();

            //Map everythig to the grid.
            foreach (var p in plottables)
            {
                p.PlotOnGrid(gameGrid);
            }
        }
        catch { }
       
    }

    IEnumerator RunGameLoopRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(gameTickLength);
            RunGameLoop();
            canChangeDirection = true;
        }
    }

    void QuerySnakeHead()
    {
        foreach (var plottable in plottables)
        {
            if (plottable is Food)
            {
                for (var i = 0; i < plottable.Points.Count; i++)
                {
                    if (plottable.Points[i] == snake.HeadPoint)
                    {
                        plottable.Points.Remove(snake.HeadPoint);
                        OnSnakeFoodEncounter.Invoke();
                    }
                }
            }
            else
            {
                if (plottable is Obstacles)
                {
                    for (var i = 0; i < plottable.Points.Count; i++)
                    {
                        if (plottable.Points[i] == snake.HeadPoint)
                        {
                            OnSnakeObstacleEncounter();
                        }
                    }
                }
                else //Check for intersecting snake.
                {
                    //Start loop from 1 to avoid counting the snake head when checking intersections.
                    for (var i = 1; i < plottable.Points.Count; i++)
                    {
                        if(plottable.Points[i] == snake.HeadPoint)
                        {
                            OnSnakeObstacleEncounter();
                        }
                    }
                }
            }
        }
    }

    void NullGameGrid()
    {
        gameGrid = new int[gridWidth, gridHeight];
    }

    void DisplayGameOverScreen()
    {
        gameOverButtonObject.SetActive(true);
    }

    public void RetryButton()
    {
        //Set layout value to 0 if there is no text in the input field.
        try
        {
            int index = int.Parse(layoutInputField.text);
            if(index > 2) index = 0;
            PlayerPrefs.SetInt("layout", index);
        }
        catch (Exception)
        {
            PlayerPrefs.SetInt("layout", 0);
        }

        SceneManager.LoadScene("Main");
    }
}

public class Snake : IPlottable
{
    const int INDEX = 2;
    List<Vector2Int> points = new List<Vector2Int>();

    public List<Vector2Int> Points => points;
    public Vector2Int HeadPoint => points[0];

    //Default constructor.
    public Snake()
    {
        points.Add(new Vector2Int(1, 1));
        points.Add(new Vector2Int(2, 1));
    }

    public void Move(Vector2Int dir, int[,] grid)
    {
            Vector2Int newPoint = new Vector2Int(HeadPoint.x + dir.x, HeadPoint.y + dir.y);

            if (newPoint.x >= grid.GetLength(0))
            {
                newPoint = new Vector2Int(0, newPoint.y);
            }
            else if (newPoint.x < 0)
            {
                newPoint = new Vector2Int(grid.GetLength(0) - 1, newPoint.y);
            }

            if (newPoint.y >= grid.GetLength(1))
            {
                newPoint = new Vector2Int(newPoint.x, 0);
            }
            else if (newPoint.y < 0)
            {
                newPoint = new Vector2Int(newPoint.x, grid.GetLength(1) - 1);
            }

            points.Insert(0, newPoint);
            points.RemoveAt(points.Count - 1);
    }

    public void AddSegment()
    {
        points.Insert(points.Count - 1, points[points.Count - 1]);
    }

    public void PlotOnGrid(int[,] grid)
    {
        foreach (var p in points)
        {
            grid[p.x, p.y] = INDEX;       
        }
    }

    public void OnFoodEncounter()
    {
        AddSegment();
    }

    public void OnObstacleEncounter()
    {
        points.Clear();
    }
}

public class Obstacles: IPlottable
{
    const int INDEX = 1;
    List<Vector2Int> points = new List<Vector2Int>();

    public List<Vector2Int> Points => points;

    public Obstacles(int preset, int[,] grid)
    {
        switch (preset)
        {
            //0 - Unbordered.
            //Bordered.
            case 1:

                for (var i = 0; i < grid.GetLength(0); i++)
                {
                    points.Add(new Vector2Int(i, 0));
                }

                for (var i = 0; i < grid.GetLength(0); i++)
                {
                    points.Add(new Vector2Int(i, grid.GetLength(1) - 1));
                }

                for (var i = 1; i < grid.GetLength(1) - 1; i++)
                {
                    points.Add(new Vector2Int(0, i));
                }

                for (var i = 1; i < grid.GetLength(1) - 1; i++)
                {
                    points.Add(new Vector2Int(grid.GetLength(1) - 1, i));
                }

                break;

            //Cross.
            case 2:

                for (var i = 1; i < grid.GetLength(0)-1; i++)
                {
                    points.Add(new Vector2Int(i, grid.GetLength(1) / 2));
                }

                for (var i = 1; i < grid.GetLength(0)-1; i++)
                {
                    points.Add(new Vector2Int(grid.GetLength(0) / 2, i));
                }

                break;
        }
    }

    public void PlotOnGrid(int[,] grid)
    {
        foreach (var p in points)
        {
            grid[p.x, p.y] = INDEX;
        }
    }
}

public class Food : IPlottable
{
    const int INDEX = 3;
    List<Vector2Int> points = new List<Vector2Int>();

    public List<Vector2Int> Points => points;
    public bool FoodAlreadyGenerated { get => points.Count > 0; }

    public void PlotOnGrid(int[,] grid)
    {
        foreach (var p in points)
        {
            grid[p.x, p.y] = INDEX;
        }
    }

    public void GenerateFood(int[,] grid)
    {
        int x = Random.Range(0, grid.GetLength(0));
        int y = Random.Range(0, grid.GetLength(1));

        if (grid[x, y] == 0)
        {
            points.Add(new Vector2Int(x, y));
        }
        else
        {
            GenerateFood(grid);
        }
    }
}

interface IPlottable
{
    List<Vector2Int> Points { get; }
    void PlotOnGrid(int[,] grid);
}