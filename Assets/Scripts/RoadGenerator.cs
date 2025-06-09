using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class RoadGenerator : MonoBehaviour
{
    public GameObject roadStraightPrefab;
    public GameObject cornerUpRight;
    public GameObject cornerRightDown;
    public GameObject cornerDownLeft;
    public GameObject cornerLeftUp;
    public GameObject towerSlotPrefab;
    public Transform parent;

    public int gridSize = 13;
    public int minRoadLength = 40;
    public float tileSize = 3f;
    public NavMeshSurface surface;
    public Vector3 worldOffset = Vector3.zero;

    private Vector2Int startPos = new Vector2Int(6, -1);  // нижняя центральная
    private Vector2Int endPos = new Vector2Int(6, 13);   // верхняя центральная
    [SerializeField] private Vector2Int entryPoint = new Vector2Int(6, -1);
    [SerializeField] private Vector2Int exitPoint = new Vector2Int(6, 13);

    private List<Vector2Int> path = new List<Vector2Int>();
    private HashSet<Vector2Int> roadPositions = new HashSet<Vector2Int>();
    private List<int> cornerIndices = new List<int>();

    [SerializeField] private int minCornerCellDistance = 1;

    void Start()
    {
        do
        {
            path.Clear();
            roadPositions.Clear();
            cornerIndices.Clear();
            foreach (Transform child in parent)
            {
                Destroy(child.gameObject);
            }
        } while (!TryGeneratePath());

        GenerateTiles();
    }

    public void Generate()
    {
        ClearOld();
        path.Clear();
        roadPositions.Clear();
        cornerIndices.Clear();

        GenerateValidPath();
        GenerateTiles();
    }

    public void BuildRoad()
    {
        surface.BuildNavMesh();
    }

    void ClearOld()
    {
        foreach (Transform child in parent)
            Destroy(child.gameObject);
    }

    void GenerateValidPath()
    {
        int maxAttempts = 50000;
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            path.Clear();
            roadPositions.Clear();
            cornerIndices.Clear();

            if (TryGeneratePath())
            {
                Debug.Log($"Путь создан. Длина: {path.Count}, Поворотов: {cornerIndices.Count}");
                return;
            }

            attempts++;
        }

        Debug.LogError("Не удалось создать подходящий путь");
    }

    bool TryGeneratePath()
    {
        path.Clear();
        roadPositions.Clear();
        cornerIndices.Clear();

        path.Add(startPos);
        roadPositions.Add(startPos);
        Vector2Int current = startPos;
        Vector2Int lastDirection = Vector2Int.up;
        int lastCornerIndex = -2;

        while (path.Count < gridSize * gridSize)
        {
            if (current == endPos)
                break;

            List<Vector2Int> possibleDirections = GetPossibleDirections(current, lastDirection, lastCornerIndex);
            bool moved = false;

            foreach (var direction in Shuffle(possibleDirections))
            {
                Vector2Int next = current + direction;
                if (!IsInBounds(next) || roadPositions.Contains(next))
                    continue;



                // Запрещаем соседние, но не соединенные напрямую дороги
                bool hasAdjacentButDisconnected = false;
                Vector2Int[] neighbors = {
    new Vector2Int(1, 0),
    new Vector2Int(-1, 0),
    new Vector2Int(0, 1),
    new Vector2Int(0, -1)
};
                foreach (var offset in neighbors)
                {
                    Vector2Int neighbor = next + offset;
                    if (roadPositions.Contains(neighbor) && neighbor != current)
                    {
                        hasAdjacentButDisconnected = true;
                        break;
                    }
                }
                if (hasAdjacentButDisconnected)
                    continue;



                bool isCorner = direction != lastDirection;

                if (isCorner && (path.Count - lastCornerIndex) < 2)
                    continue;

                if (isCorner && HasCornerNeighbor(next))
                    continue;

                path.Add(next);
                roadPositions.Add(next);

                if (isCorner)
                {
                    cornerIndices.Add(path.Count - 1); // ✅ добавляем индекс, а не позицию
                    lastCornerIndex = path.Count - 1;
                }

                current = next;
                lastDirection = direction;
                moved = true;
                break;
            }

            if (!moved)
                return false;

            if (Vector2Int.Distance(current, endPos) == 1 && !roadPositions.Contains(endPos))
            {
                path.Add(endPos);
                roadPositions.Add(endPos);
                return path.Count >= minRoadLength;
            }
        }

        return current == endPos && path.Count >= minRoadLength;
    }

    List<T> Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randIndex = Random.Range(i, list.Count);
            (list[i], list[randIndex]) = (list[randIndex], list[i]);
        }
        return list;
    }

    bool HasCornerNeighbor(Vector2Int position)
    {
        foreach (int index in cornerIndices)
        {
            if (index >= 0 && index < path.Count)
            {
                Vector2Int cornerPos = path[index];
                if (Vector2Int.Distance(position, cornerPos) < minCornerCellDistance)
                    return true;
            }
        }
        return false;
    }

    bool IsCornerAt(Vector2Int cell)
    {
        foreach (int index in cornerIndices)
        {
            if (path[index] == cell)
                return true;
        }
        return false;
    }

    List<Vector2Int> GetPossibleDirections(Vector2Int current, Vector2Int lastDirection, int lastCornerIndex)
    {
        List<Vector2Int> directions = new List<Vector2Int>();
        Vector2Int[] allDirections = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

        foreach (var dir in allDirections)
        {
            Vector2Int next = current + dir;

            // Проверяем, что не выходим за границы и не пересекаем путь
            if (IsInsideGrid(next) && !roadPositions.Contains(next))
            {
                // Проверяем минимальное расстояние между поворотами
                if (dir != lastDirection && (path.Count - lastCornerIndex) < 2)
                    continue;

                directions.Add(dir);
            }
        }

        return directions;
    }

    bool IsInsideGrid(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < gridSize && pos.y >= 0 && pos.y < gridSize;
    }
    bool IsInBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < gridSize && pos.y >= 0 && pos.y < gridSize;
    }

    void GenerateTiles()
    {
        PlaceEdgeTile(entryPoint, path[0]);
        PlaceEdgeTile(exitPoint, path[^1]);
        for (int i = 0; i < path.Count; i++)
        {
            Vector2Int pos = path[i];
            Vector3 worldPos = worldOffset + new Vector3(pos.x * tileSize, 0, pos.y * tileSize);

            if (i == 0)
            {
                Vector2Int nextDir = path[i + 1] - path[i];
                Quaternion rotation = GetRotationFromDirection(nextDir);
                Instantiate(roadStraightPrefab, worldPos, rotation, parent);
                continue;
            }

            // Обработка конечной позиции
            if (i == path.Count - 1)
            {
                Vector2Int prevDir = path[i] - path[i - 1];
                Quaternion rotation = GetRotationFromDirection(prevDir);
                Instantiate(roadStraightPrefab, worldPos, rotation, parent);
                continue;
            }
            else
            {
                Vector2Int prevDir = path[i] - path[i - 1];
                Vector2Int nextDir = path[i + 1] - path[i];

                if (prevDir != nextDir)
                {
                    GameObject cornerPrefab = GetCornerPrefab(prevDir, nextDir);
                    if (cornerPrefab != null)
                        Instantiate(cornerPrefab, worldPos, Quaternion.identity, parent);
                }
                else
                {
                    Quaternion rotation = (prevDir.x != 0) ? Quaternion.Euler(0, 90, 0) : Quaternion.identity;
                    Instantiate(roadStraightPrefab, worldPos, rotation, parent);
                }
            }
        }



        // Генерация пустых клеток для башен
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (!roadPositions.Contains(pos))
                {
                    Vector3 worldPos = worldOffset + new Vector3(x * tileSize, 0, y * tileSize);
                    Instantiate(towerSlotPrefab, worldPos, Quaternion.identity, parent);
                }
            }
        }
    }
    
    void PlaceEdgeTile(Vector2Int outside, Vector2Int inside)
    {
        Vector2Int dir = inside - outside;
        Vector3 worldPos = worldOffset + new Vector3(outside.x * tileSize, 0, outside.y * tileSize);

        Quaternion rotation;
        GameObject prefabToUse;

        if (dir.x == 0 || dir.y == 0)
        {
            // Прямая дорога
            rotation = (dir.x != 0) ? Quaternion.Euler(0, 90, 0) : Quaternion.identity;
            prefabToUse = roadStraightPrefab;
        }
        else
        {
            // Угловая дорога — подбираем префаб по направлению
            prefabToUse = GetCornerPrefab(Vector2Int.zero, dir); // можно доработать при необходимости
            rotation = Quaternion.identity; // либо дописать GetRotationFromCorner(...)
        }

        Instantiate(prefabToUse, worldPos, rotation, parent);
    }

    void PlaceCornerEdgeTile(Vector2Int outside, Vector2Int inside)
    {
        Vector2Int dir = inside - outside;
        Vector3 worldPos = worldOffset + new Vector3(outside.x * tileSize, 0, outside.y * tileSize);

        Quaternion rotation;
        GameObject prefabToUse;

        prefabToUse = GetCornerPrefab(Vector2Int.zero, dir); // можно доработать при необходимости
        rotation = Quaternion.identity; // либо дописать GetRotationFromCorner(...)

        Instantiate(prefabToUse, worldPos, rotation, parent);
    }

    void PlaceEntryOrExitTurn(Vector2Int inside, Vector2Int outside)
    {
        Vector2Int dirFromOutside = inside - outside;
        Quaternion rotation;

        if (dirFromOutside.x == 0 && dirFromOutside.y == 1)
            rotation = Quaternion.Euler(0, 0, 0);      // вход сверху
        else if (dirFromOutside.x == 0 && dirFromOutside.y == -1)
            rotation = Quaternion.Euler(0, 180, 0);    // вход снизу
        else if (dirFromOutside.x == 1 && dirFromOutside.y == 0)
            rotation = Quaternion.Euler(0, -90, 0);    // вход слева
        else if (dirFromOutside.x == -1 && dirFromOutside.y == 0)
            rotation = Quaternion.Euler(0, 90, 0);     // вход справа
        else
            rotation = Quaternion.identity; // на случай диагонали (не должна быть)

        // Удалим старую клетку
        Vector3 pos = worldOffset + new Vector3(inside.x * tileSize, 0, inside.y * tileSize);
        foreach (Transform child in parent)
        {
            if (Vector3.Distance(child.position, pos) < 0.1f)
            {
                Destroy(child.gameObject);
                break;
            }
        }

        // Установим новый повёрнутый префаб
        Instantiate(roadStraightPrefab, pos, rotation, parent);
    }

    Quaternion GetRotationFromDirection(Vector2Int dir)
    {
        if (dir == Vector2Int.up) return Quaternion.identity;
        if (dir == Vector2Int.right) return Quaternion.Euler(0, 90, 0);
        if (dir == Vector2Int.down) return Quaternion.Euler(0, 180, 0);
        if (dir == Vector2Int.left) return Quaternion.Euler(0, 270, 0);
        return Quaternion.identity;
    }

    GameObject GetCornerPrefab(Vector2Int prevDir, Vector2Int nextDir)
    {
        // Проверка направлений, которые образуют угол
        if ((prevDir == Vector2Int.down && nextDir == Vector2Int.left) ||
            (prevDir == Vector2Int.right && nextDir == Vector2Int.up))
            return cornerUpRight;

        if ((prevDir == Vector2Int.right && nextDir == Vector2Int.down) ||
            (prevDir == Vector2Int.up && nextDir == Vector2Int.left))
            return cornerRightDown;

        if ((prevDir == Vector2Int.up && nextDir == Vector2Int.right) ||
            (prevDir == Vector2Int.left && nextDir == Vector2Int.down))
            return cornerDownLeft;

        if ((prevDir == Vector2Int.left && nextDir == Vector2Int.up) ||
            (prevDir == Vector2Int.down && nextDir == Vector2Int.right))
            return cornerLeftUp;

        return null;
    }
}