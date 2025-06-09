using UnityEngine;
using System.Collections.Generic;

public class PathVisualizer : MonoBehaviour
{
    const int SIZE = 13;
    const int REQUIRED_LENGTH = 49;
    const int REQUIRED_TURNS = 10;

    public int pixelsPerCell = 40;
    public Color backgroundColor = Color.white;
    public Color pathColor = Color.green;
    public Color startColor = Color.blue;
    public Color endColor = Color.red;
    public Color turnColor = Color.yellow;

    private Vector2Int startPos;
    private Vector2Int endPos;
    private Texture2D pathTexture;
    private List<Vector2Int> directions = new List<Vector2Int>()
    {
        new Vector2Int(-1, 0),  // Вверх
        new Vector2Int(0, 1),   // Вправо
        new Vector2Int(1, 0),   // Вниз
        new Vector2Int(0, -1)   // Влево
    };

    void Start()
    {
        startPos = new Vector2Int(SIZE - 1, SIZE / 2);
        endPos = new Vector2Int(0, SIZE / 2);

        List<Vector2Int> path = FindValidPath();
        if (path != null)
        {
            VisualizePath(path);
            Debug.Log("Путь найден! Длина: " + path.Count + ", Поворотов: " + CountTurns(path));
        }
        else
        {
            Debug.LogError("Путь не найден");
        }
    }

    List<Vector2Int> FindValidPath()
    {
        bool[,] visited = new bool[SIZE, SIZE];
        List<Vector2Int> currentPath = new List<Vector2Int>();

        return DFS(startPos, 0, 0, -1, visited, currentPath);
    }

    List<Vector2Int> DFS(Vector2Int pos, int steps, int turns, int lastDir, bool[,] visited, List<Vector2Int> currentPath)
    {
        // Проверка завершения
        if (steps == REQUIRED_LENGTH - 1 && pos == endPos && turns == REQUIRED_TURNS)
        {
            return new List<Vector2Int>(currentPath) { pos };
        }

        if (steps >= REQUIRED_LENGTH || turns > REQUIRED_TURNS)
            return null;

        visited[pos.x, pos.y] = true;
        currentPath.Add(pos);

        for (int i = 0; i < directions.Count; i++)
        {
            Vector2Int newPos = pos + directions[i];

            if (IsValidPosition(newPos) && !visited[newPos.x, newPos.y])
            {
                int newTurns = turns;
                if (lastDir != -1 && i != lastDir)
                {
                    newTurns++;
                    if (newTurns > REQUIRED_TURNS)
                        continue;
                }

                var result = DFS(newPos, steps + 1, newTurns, i, visited, currentPath);
                if (result != null)
                    return result;
            }
        }

        visited[pos.x, pos.y] = false;
        currentPath.RemoveAt(currentPath.Count - 1);
        return null;
    }

    bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < SIZE && pos.y >= 0 && pos.y < SIZE;
    }

    int CountTurns(List<Vector2Int> path)
    {
        if (path.Count < 3) return 0;

        int turns = 0;
        Vector2Int prevDir = path[1] - path[0];

        for (int i = 2; i < path.Count; i++)
        {
            Vector2Int currDir = path[i] - path[i - 1];
            if (currDir != prevDir)
            {
                turns++;
                prevDir = currDir;
            }
        }
        return turns;
    }

    void VisualizePath(List<Vector2Int> path)
    {
        int textureSize = SIZE * pixelsPerCell;
        pathTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);

        // Заливка фона
        Color[] pixels = new Color[textureSize * textureSize];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = backgroundColor;
        pathTexture.SetPixels(pixels);

        // Рисуем сетку
        DrawGrid();

        // Рисуем путь и отмечаем повороты
        for (int i = 0; i < path.Count; i++)
        {
            bool isTurn = i > 1 && i < path.Count - 1 &&
                         (path[i] - path[i - 1]) != (path[i + 1] - path[i]);

            Color color = pathColor;
            if (i == 0) color = startColor;
            else if (i == path.Count - 1) color = endColor;
            else if (isTurn) color = turnColor;

            DrawCell(path[i], color);
        }

        pathTexture.Apply();

        // Создаем объект для отображения
        GameObject display = new GameObject("PathDisplay");
        SpriteRenderer renderer = display.AddComponent<SpriteRenderer>();
        renderer.sprite = Sprite.Create(
            pathTexture,
            new Rect(0, 0, textureSize, textureSize),
            new Vector2(0.5f, 0.5f),
            pixelsPerCell
        );

        // Центрируем камеру
        Camera.main.transform.position = new Vector3(
            textureSize * 0.5f / pixelsPerCell,
            textureSize * 0.5f / pixelsPerCell,
            -10
        );
        Camera.main.orthographicSize = textureSize * 0.6f / pixelsPerCell;
    }

    void DrawCell(Vector2Int cellPos, Color color)
    {
        int xStart = cellPos.y * pixelsPerCell;
        int yStart = (SIZE - 1 - cellPos.x) * pixelsPerCell;

        for (int x = xStart + 1; x < xStart + pixelsPerCell - 1; x++)
        {
            for (int y = yStart + 1; y < yStart + pixelsPerCell - 1; y++)
            {
                pathTexture.SetPixel(x, y, color);
            }
        }
    }

    void DrawGrid()
    {
        Color gridColor = new Color(0.8f, 0.8f, 0.8f);

        // Вертикальные линии
        for (int x = 0; x <= SIZE; x++)
        {
            int pixelX = x * pixelsPerCell;
            for (int y = 0; y < SIZE * pixelsPerCell; y++)
            {
                pathTexture.SetPixel(pixelX, y, gridColor);
            }
        }

        // Горизонтальные линии
        for (int y = 0; y <= SIZE; y++)
        {
            int pixelY = y * pixelsPerCell;
            for (int x = 0; x < SIZE * pixelsPerCell; x++)
            {
                pathTexture.SetPixel(x, pixelY, gridColor);
            }
        }
    }
}