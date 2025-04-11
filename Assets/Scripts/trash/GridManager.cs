using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int gridWidth = 10; // Nombre de colonnes
    public int gridHeight = 10; // Nombre de lignes
    public float cellSize = 1f; // Taille des cellules
    public GameObject cellPrefab; // Préfabriqué pour chaque cellule

    private GameObject[,] gridCells; // Stocker les cellules

    void Start()
    {
        Debug.Log($"GridManager: Starting grid generation with width={gridWidth}, height={gridHeight}, cellSize={cellSize}");
        GenerateGrid();
    }

    void GenerateGrid()
    {
        gridCells = new GameObject[gridWidth, gridHeight];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                // Calculer la position dans le monde
                Vector3 position = new Vector3(x * cellSize, 0, y * cellSize);

                // Créer une cellule
                GameObject cell = Instantiate(cellPrefab, position, Quaternion.identity, transform);
                cell.name = $"Cell_{x}_{y}";

                Debug.Log($"GridManager: Created cell at ({x}, {y}) with position {position}");

                // Stocker la cellule
                gridCells[x, y] = cell;
            }
        }
        Debug.Log("GridManager: Grid generation complete.");
    }
}
