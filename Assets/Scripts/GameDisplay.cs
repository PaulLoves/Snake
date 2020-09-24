using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDisplay : MonoBehaviour
{
    [SerializeField]
    GameObject plane, cube;
    [SerializeField]
    Color obstacleColor, snakeColor, foodColor;

    GameManager manager;
    Dictionary<Vector2Int, GameObject> pointAndCubePairs = new Dictionary<Vector2Int, GameObject>();

    public Dictionary<Vector2Int, GameObject> PointAndCubePairs => pointAndCubePairs;

    private void Awake()
    {
        manager = FindObjectOfType<GameManager>();
    }

    private void Start()
    {
        BuildPlayground();
        StartCoroutine(UpdatePlaygroundRoutine());
    }

    void BuildPlayground()
    {
        //Convert to float dimensions for convenience.
        float width = manager.GridWidth, height = manager.GridHeight;

        GameObject p = Instantiate(plane);
        p.transform.position = new Vector3(width / 2 - 0.5f, 0, height / 2 - 0.5f);
        p.transform.rotation = Quaternion.Euler(90, 0, 0);
        p.transform.localScale = new Vector3(width, height, 1);

        for (int x = 0; x < manager.GridWidth; x++)
        {
            for (int z = 0; z < manager.GridHeight; z++)
            {
                GameObject c = Instantiate(cube);
                c.transform.position = new Vector3(x, 0.5f, z);
                c.transform.localScale = Vector3.zero;
                pointAndCubePairs.Add(new Vector2Int(x, z), c);
            }
        }
    }

    void UpdatePlayground()
    {
        int[,] grid = manager.GameGrid;

        for (int x = 0; x < manager.GridWidth; x++)
        {
            for (int y = 0; y < manager.GridHeight; y++)
            {
                GameObject c = pointAndCubePairs[new Vector2Int(x, y)];
                CubeFade cubeFade = c.GetComponent<CubeFade>();
                Renderer cubeRenderer = c.GetComponent<Renderer>();

                switch (grid[x, y])
                {
                    //Empty
                    case 0:
                        cubeFade.FadeOut();
                        break;

                    //Obstacle
                    case 1:
                        cubeFade.FadeIn();
                        cubeRenderer.material.color = obstacleColor;
                        break;

                    //Snake
                    case 2:
                        cubeFade.FadeIn();
                        cubeRenderer.material.color = snakeColor;
                        break;

                    //Food
                    case 3:
                        cubeFade.FadeIn();
                        cubeRenderer.material.color = foodColor;
                        break;
                }
            }
        }
    }

    IEnumerator UpdatePlaygroundRoutine()
    {
        while (true)
        {
            UpdatePlayground();
            yield return new WaitForSeconds(manager.GameTickLength);
        }
    }
}
